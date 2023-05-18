using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для RoundBlockControl.xaml
    /// </summary>
    public partial class RoundBlockControl : UserControl
    {
        bool _isHex { get; set; }
        public RoundBlockControl(int round, byte[] l, byte[] r, byte[] ep, byte[] key, byte[] xor, byte[] sBox, byte[] p, bool isHex)
        {
            InitializeComponent();
            Height = 300;
            _isHex = isHex;
            if (round == 0)
            {
                arrowRightTopWhite.Visibility = Visibility.Visible;
                arrowRightTopGreen.Visibility = Visibility.Collapsed;
            }
            else
            {
                arrowRightTopWhite.Visibility = Visibility.Collapsed;
                arrowRightTopGreen.Visibility = Visibility.Visible;
            }
            rL.Text = $"L{round}";
            rR.Text = $"R{round}";
            rK.Text = $"K{round + 1}";
            _isHex = isHex;
            SetValue(L, l);
            SetValue(R, r);
            SetValue(EP, ep);
            SetValue(K, key);
            SetValue(XOR, xor);
            SetValue(SBox, sBox);
            SetValue(P, p);
            SplitText(EP, 6);
            SplitText(K, 6);
            SplitText(XOR, 6);
            SplitText(SBox, 4);
        }
        private void SetValue(TextBlock textBlock, byte[] value)
        {
            if (_isHex)
                textBlock.Text = value.BytesToString(" ", true).ToUpper();
            else
                textBlock.Text = value.BitsToString();
        }
        private void SplitText(TextBlock textBlock, int size)
        {
            var result = (from Match m in Regex.Matches(textBlock.Text, @".{1," + size + "}")
                          select m.Value).ToList();
            textBlock.Text = string.Empty;
            foreach(var r in result)
            {
                textBlock.Text += r + " ";
            }
            textBlock.Text = textBlock.Text.Remove(textBlock.Text.Length-1, 1);
        }
        private void Copy(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            Clipboard.SetText(textBlock.Text);
        }
    }
}
