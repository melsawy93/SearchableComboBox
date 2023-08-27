using System;
using System.Collections.Generic;
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
    public partial class MainWindow : Window
    {
        public List<Item> Items { get; }

        public MainWindow()
        {
            InitializeComponent();
            Items = GenerateItems();
            
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

    }
}
