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
using The_DES_cipher.DES;

namespace The_DES_cipher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static MainWindow Instance { get; private set; }
        TextPage textPage;
        FilePage filePage;
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            textPage = new TextPage();
            filePage = new FilePage();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Pages.Content = textPage;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Pages.Content = filePage;
        }
    }
}
