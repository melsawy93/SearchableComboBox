using System;
using System.Windows;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Data;

namespace SearchableComboBox
{
    public class SearchableComboBox : ComboBox
    {
        #region Constructors
        public SearchableComboBox()
        {
            //this.Loaded += SearchableCB_Loaded;
            this.DropDownClosed += ClearSearchTerm;
        }

        static SearchableComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchableComboBox), new FrameworkPropertyMetadata(typeof(SearchableComboBox)));
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

        #region Error and border visibilty
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
                searchableComboBox._collectionViewSource.View.Refresh();
                searchableComboBox.UpdateVisibility();
            }
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
    }

}

