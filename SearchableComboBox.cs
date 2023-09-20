using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;

namespace SearchableComboBox
{
    /// <summary>
    /// Represents a ComboBox with search functionality.
    /// </summary>
    public class SearchableComboBox : ComboBox
    {
        #region Fields and Constructors

        private readonly DispatcherTimer _debounceTimer;
        private bool debug = true;
        private TextBox _searchTermTextBox;
        private CollectionViewSource _collectionViewSource;
        private object _lastSelectedItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchableComboBox"/> class.
        /// </summary>
        public SearchableComboBox()
        {
            Debug.WriteLineIf(debug, "Constructor");
            _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _debounceTimer.Tick += DebounceTimer_Tick;
            this.DropDownClosed += ClearSearchTerm;
        }

        static SearchableComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchableComboBox), new FrameworkPropertyMetadata(typeof(SearchableComboBox)));
        }

        public override void OnApplyTemplate()
        {
            Debug.WriteLineIf(debug, "OnApplyTemplate");
            base.OnApplyTemplate();
            _searchTermTextBox = Template.FindName("SearchTermTextBox", this) as TextBox;
        }

        #endregion

        #region CustomSort Support

        public static readonly DependencyProperty CustomSortProperty =
            DependencyProperty.Register(
                "CustomSort",
                typeof(IComparer),
                typeof(SearchableComboBox),
                new PropertyMetadata(null, OnCustomSortChanged));

        /// <summary>
        /// Gets or sets the custom sort comparer.
        /// </summary>
        public IComparer CustomSort
        {
            get => (IComparer)GetValue(CustomSortProperty);
            set => SetValue(CustomSortProperty, value);
        }

        private static void OnCustomSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var comboBox = d as SearchableComboBox;
            if (comboBox._collectionViewSource != null && comboBox._collectionViewSource.View is ListCollectionView lcv)
            {
                lcv.CustomSort = e.NewValue as IComparer;
            }
        }

        #endregion

        #region IsSearchEnabled

        public static readonly DependencyProperty IsSearchEnabledProperty =
            DependencyProperty.Register(
                "IsSearchEnabled",
                typeof(bool),
                typeof(SearchableComboBox),
                new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value indicating whether search is enabled.
        /// </summary>
        public bool IsSearchEnabled
        {
            get => (bool)GetValue(IsSearchEnabledProperty);
            set => SetValue(IsSearchEnabledProperty, value);
        }

        #endregion

        #region IsNullable

        public static readonly DependencyProperty IsNullableProperty =
            DependencyProperty.Register("IsNullable", typeof(bool), typeof(SearchableComboBox), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value indicating whether the value is nullable.
        /// </summary>
        public bool IsNullable
        {
            get => (bool)GetValue(IsNullableProperty);
            set => SetValue(IsNullableProperty, value);
        }

        #endregion

        #region Placeholder Text

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(SearchableComboBox), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the placeholder text.
        /// </summary>
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        #endregion

        #region OnItemsSourceChanged event

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            Debug.WriteLineIf(debug, "OnItemsSourceChanged");
            base.OnItemsSourceChanged(oldValue, newValue);

            if (_collectionViewSource == null)
            {
                _collectionViewSource = new CollectionViewSource { Source = newValue };
                _collectionViewSource.View.Filter = FilterPredicate;

                if (_collectionViewSource.View is ListCollectionView lcv && CustomSort != null)
                {
                    lcv.CustomSort = CustomSort;
                }

                ItemsSource = _collectionViewSource.View;
            }
        }

        #endregion

        #region Changes only when selected

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            Debug.WriteLineIf(debug, "OnSelectionChanged");
            Debug.WriteLineIf(debug, "Added Items Count: " + e.AddedItems.Count);
            Debug.WriteLineIf(debug, "Removed Items Count: " + e.RemovedItems.Count);

            if (e.AddedItems.Count > 0)
            {
                _lastSelectedItem = e.AddedItems[0];
            }

            base.OnSelectionChanged(e);
            Debug.WriteLineIf(debug, "SelectionBoxItem " + this.SelectionBoxItem);
        }

        #endregion

        #region Error and border visibility

        public static readonly DependencyProperty NotFoundLabelVisibilityProperty =
            DependencyProperty.Register("NotFoundLabelVisibility", typeof(Visibility), typeof(SearchableComboBox), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty DropDownBorderVisibilityProperty =
            DependencyProperty.Register("DropDownBorderVisibility", typeof(Visibility), typeof(SearchableComboBox), new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Gets or sets the visibility of the not found label.
        /// </summary>
        public Visibility NotFoundLabelVisibility
        {
            get => (Visibility)GetValue(NotFoundLabelVisibilityProperty);
            set => SetValue(NotFoundLabelVisibilityProperty, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the dropdown border.
        /// </summary>
        public Visibility DropDownBorderVisibility
        {
            get => (Visibility)GetValue(DropDownBorderVisibilityProperty);
            set => SetValue(DropDownBorderVisibilityProperty, value);
        }

        private void UpdateVisibility()
        {
            Debug.WriteLineIf(debug, "UpdateVisibility");
            if (_collectionViewSource.View.IsEmpty)
            {
                NotFoundLabelVisibility = Visibility.Visible;
                DropDownBorderVisibility = Visibility.Collapsed;
            }
            else
            {
                NotFoundLabelVisibility = Visibility.Collapsed;
                DropDownBorderVisibility = Visibility.Visible;
            }
        }

        #endregion

        #region Filtering logic

        public static readonly DependencyProperty SearchTermProperty =
            DependencyProperty.Register("SearchTerm", typeof(string), typeof(SearchableComboBox), new PropertyMetadata(string.Empty, OnSearchTermChanged));

        /// <summary>
        /// Gets or sets the search term.
        /// </summary>
        public string SearchTerm
        {
            get => (string)GetValue(SearchTermProperty);
            set => SetValue(SearchTermProperty, value);
        }

        private static void OnSearchTermChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SearchableComboBox searchableComboBox)
            {
                searchableComboBox.DebounceFilter();
            }
        }

        private void DebounceFilter()
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void DebounceTimer_Tick(object sender, EventArgs e)
        {
            _debounceTimer.Stop();

            _collectionViewSource.View.Refresh();
            UpdateVisibility();
        }

        private bool FilterPredicate(object item)
        {
            if (string.IsNullOrEmpty(SearchTerm))
            {
                return true;
            }

            string strItem = item.ToString();
            return strItem.IndexOf(SearchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion

        #region Clear Search

        private void ClearSearchTerm(object sender, EventArgs e)
        {
            Debug.WriteLineIf(debug, "ClearSearchTerm");
            SearchTerm = string.Empty;
            _collectionViewSource.View.Refresh();

            if (_lastSelectedItem != null)
            {
                SelectedItem = _lastSelectedItem;
            }
        }

        #endregion
    }
}
