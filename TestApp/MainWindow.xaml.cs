using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public List<Item> Items { get; }

        private Item _selectedItem;

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {   
            Items = GenerateItems();
            InitializeComponent();
            DataContext = this;
        }

        private List<Item> GenerateItems()
        {
            var items = new List<Item>();
            for (int i = 1; i <= 20; i++)
            {
                var item = new Item($"Item {i}");
                items.Add(item);
            }

            for (int i = 1; i <= 20; i++)
            {
                var item = new Item($"Apple {i}");
                items.Add(item);
            }

            for (int i = 1; i <= 20; i++)
            {
                var item = new Item($"Banana {i}");
                items.Add(item);
            }

            for (int i = 1; i <= 20; i++)
            {
                var item = new Item($"AppleandBanana {i}");
                items.Add(item);
            }

            for (int i = 1; i <= 20; i++)
            {
                var item = new Item($"Trees {i}");
                items.Add(item);
            }

            for (int i = 1; i <= 20; i++)
            {
                var item = new Item($"ATrees {i}");
                items.Add(item);
            }

            for (int i = 1; i <= 20; i++)
            {
                var item = new Item($"ABTrees {i}");
                items.Add(item);
            }
            return items;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem = null;
        }
    }
}
