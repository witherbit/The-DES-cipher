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
using System.Windows.Shapes;
using The_DES_cipher.DES;

namespace The_DES_cipher
{
    /// <summary>
    /// Логика взаимодействия для RoundKeysWindow.xaml
    /// </summary>
    public partial class RoundKeysWindow : Window
    {
        public RoundKeysWindow(Des des, bool isHex)
        {
            InitializeComponent();
            var bits = BitExtensions.StringToBits(des.Key, Encoding.ASCII);
            if (isHex)
            {
                Title += " [Bytes]";
                key.Text = bits.BytesToString(" ", true).ToUpper();
                PC1.Text = des.RoundKeys[0].PC1.BytesToString(" ", true).ToUpper();
            }
            else
            {
                Title += " [Bits]";
                key.Text = bits.BitsToString();
                PC1.Text = des.RoundKeys[0].PC1.BitsToString();
            }
            foreach (var key in des.RoundKeys)
                SetControl(key, isHex);
        }

        public void SetControl(RoundKey key, bool isHex)
        {
            var control = new RoundKeyControl(key, isHex)
            {
                Height = 250
            };
            stackPanelMain.Children.Add(control);
        }

        private void Copy(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            Clipboard.SetText(textBlock.Text);
        }
    }
}
