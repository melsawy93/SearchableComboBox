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
        private bool _isDropDownOpen = false;
        private TextBlock _placeholderTextBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchableComboBox"/> class.
        /// </summary>
        public SearchableComboBox()
        {
            Debug.WriteLineIf(debug, "Constructor");
            _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _debounceTimer.Tick += DebounceTimer_Tick;
            this.DropDownClosed += OnDropDownClosed;
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
            _placeholderTextBlock = Template.FindName("PlaceholderTextBlock", this) as TextBlock;
            UpdatePlaceholderVisibility();
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

        #region Placeholder Text

        public static readonly DependencyProperty PlaceholderStyleProperty =
            DependencyProperty.Register("PlaceholderStyle", typeof(Style), typeof(SearchableComboBox), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the style for the placeholder text block.
        /// </summary>
        public Style PlaceholderStyle
        {
            get => (Style)GetValue(PlaceholderStyleProperty);
            set => SetValue(PlaceholderStyleProperty, value);
        }



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

        private void UpdatePlaceholderVisibility()
        {
            if (_placeholderTextBlock == null) return;

            if (SelectedItem == null || SelectedItem.Equals(string.Empty))
            {
                _placeholderTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                _placeholderTextBlock.Visibility = Visibility.Collapsed;
            }
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

            if (_isDropDownOpen && e.AddedItems.Count == 0 && e.RemovedItems.Count > 0)
            {
                // If the dropdown is open and the selection was removed due to filtering, revert the selection to the last selected item
                SelectedItem = _lastSelectedItem;
                return; // Exit early to avoid additional processing and raising events
            }

            if (e.AddedItems.Count > 0)
            {
                _lastSelectedItem = e.AddedItems[0];
            }

            UpdatePlaceholderVisibility();
            base.OnSelectionChanged(e);
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

            if (strItem.IndexOf(SearchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        protected override void OnDropDownOpened(EventArgs e)
        {
            Debug.WriteLineIf(debug, "OnDropDownOpened");
            _isDropDownOpen = true;
            _lastSelectedItem = SelectedItem;  // Store the current selected item.

            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                _searchTermTextBox?.Focus();
            }), DispatcherPriority.ContextIdle);

            base.OnDropDownOpened(e);
        }

        #endregion

        #region Clear Search

        private void OnDropDownClosed(object sender, EventArgs e)
        {
            Debug.WriteLineIf(debug, "OnDropDownClosed");
            SearchTerm = string.Empty;
            _collectionViewSource.View.Refresh();

            if (_lastSelectedItem != null)
            {
                SelectedItem = _lastSelectedItem;
            }
            _isDropDownOpen = false;
        }

        #endregion
    }
}
