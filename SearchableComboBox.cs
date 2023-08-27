using System;
using System.Windows;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Input;

namespace SearchableComboBox
{
    public class SearchableComboBox : ComboBox
    {
        #region Constructors
        private readonly DispatcherTimer _debounceTimer;

        public SearchableComboBox()
        {
            _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _debounceTimer.Tick += DebounceTimer_Tick;
            this.DropDownClosed += ClearSearchTerm;
        }

        static SearchableComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchableComboBox), new FrameworkPropertyMetadata(typeof(SearchableComboBox)));
        }
       
        private TextBox _searchTermTextBox;
        public override void OnApplyTemplate()
        {
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
                new PropertyMetadata(null, OnCustomSortChanged)
            );

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

        #region OnItemsSourceChanged event
        private CollectionViewSource _collectionViewSource;

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
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
            if (e.AddedItems.Count <= 0)
            {
                e.Handled = true;
            }
            else
            {
                base.OnSelectionChanged(e);
            }
        }
        #endregion

        #region Error and border visibility
        public static readonly DependencyProperty NotFoundLabelVisibilityProperty =
            DependencyProperty.Register("NotFoundLabelVisibility", typeof(Visibility), typeof(SearchableComboBox), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty DropDownBorderVisibilityProperty =
            DependencyProperty.Register("DropDownBorderVisibility", typeof(Visibility), typeof(SearchableComboBox), new PropertyMetadata(Visibility.Visible));

        public Visibility NotFoundLabelVisibility
        {
            get => (Visibility)GetValue(NotFoundLabelVisibilityProperty);
            set => SetValue(NotFoundLabelVisibilityProperty, value);
        }

        public Visibility DropDownBorderVisibility
        {
            get => (Visibility)GetValue(DropDownBorderVisibilityProperty);
            set => SetValue(DropDownBorderVisibilityProperty, value);
        }

        private void UpdateVisibility()
        {
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

            string strItem = item as string;
            if (strItem == null)
            {
                strItem = item.ToString();
            }

            return strItem.IndexOf(SearchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ClearSearchTerm(object sender, EventArgs e)
        {
            SearchTerm = string.Empty;
        }
        #endregion

        #region Prevent Focus on Dropdown List
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Escape)
            {
                return;
            }

            if (_searchTermTextBox != null)
            {
                if (!_searchTermTextBox.IsFocused)
                {
                    _searchTermTextBox.Focus();
                }
            }
            else
            {
                // Handle the event only if the TextBox isn't focused
                e.Handled = true;
            }
        }
        #endregion
    }
}
