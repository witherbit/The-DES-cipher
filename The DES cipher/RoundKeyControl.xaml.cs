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
    /// Логика взаимодействия для RoundKeyControl.xaml
    /// </summary>
    public partial class RoundKeyControl : UserControl
    {
        private bool _isHex { get; set; }
        public RoundKeyControl(RoundKey key, bool isHex)
        {
            InitializeComponent();
            rC0.Text = $"C{key.Round - 1}";
            rD0.Text = $"D{key.Round - 1}";
            rC.Text = $"<<{Tables.Shift[key.Round-1]} => C{key.Round}";
            rD.Text = $"<<{Tables.Shift[key.Round - 1]} => D{key.Round}";
            rCD.Text = $"CD{key.Round}";
            rPC2.Text = $"PC2 [K{key.Round}]";
            if (key.Round == 16)
            {
                Grid.SetRowSpan(arrowRoundLeft, 2);
                Grid.SetRowSpan(arrowRoundRight, 2);
                arrowRoundLeft.Margin = new Thickness(80, 0, 0, 10);
                arrowRoundRight.Margin = new Thickness(0, 0, 80, 10);
            }
            _isHex = isHex;
            SetColor(key.Round);
            //Round.Text = key.Round.ToString();
            //SetValue(PC1, key.PC1);
            SetValue(C0, key.C0);
            SetValue(C, key.C);
            SetValue(D0, key.D0);
            SetValue(D, key.D);
            SetValue(PrePC2, key.PrePC2);
            SetValue(PC2, key.Key);
            if (!_isHex)
            {
                //PC1.Text = PC1.Text.Insert(28, " ");
                PrePC2.Text = PrePC2.Text.Insert(28, " ");
            }
            //Shift.Text = Tables.Shift[key.Round - 1].ToString();
        }

        private void SetValue(TextBlock textBlock, byte[] value)
        {
            if (_isHex)
                textBlock.Text = value.BytesToString(" ", true).ToUpper();
            else
                textBlock.Text = value.BitsToString();
        }
        public void SetColor(int round)
        {
            switch (round)
            {
                case 1:
                    C0.Foreground = "#dfdfdf".ToColor();
                    D0.Foreground = "#dfdfdf".ToColor();
                    C.Foreground = "#ffadad".ToColor();
                    D.Foreground = "#adfff9".ToColor();
                    break;
                case 2:
                    C0.Foreground = "#ffadad".ToColor();
                    D0.Foreground = "#adfff9".ToColor();
                    C.Foreground = "#ffadfb".ToColor();
                    D.Foreground = "#adffd4".ToColor();
                    break;
                case 3:
                    C0.Foreground = "#ffadfb".ToColor();
                    D0.Foreground = "#adffd4".ToColor();
                    C.Foreground = "#c0adff".ToColor();
                    D.Foreground = "#bbffad".ToColor();
                    break;
                case 4:
                    C0.Foreground = "#c0adff".ToColor();
                    D0.Foreground = "#bbffad".ToColor();
                    C.Foreground = "#ade0ff".ToColor();
                    D.Foreground = "#ebffad".ToColor();
                    break;
                case 5:
                    C0.Foreground = "#ade0ff".ToColor();
                    D0.Foreground = "#ebffad".ToColor();
                    C.Foreground = "#adffe9".ToColor();
                    D.Foreground = "#ffd9ad".ToColor();
                    break;
                case 6:
                    C0.Foreground = "#adffe9".ToColor();
                    D0.Foreground = "#ffd9ad".ToColor();
                    C.Foreground = "#adffbc".ToColor();
                    D.Foreground = "#ffadad".ToColor();
                    break;
                case 7:
                    C0.Foreground = "#adffbc".ToColor();
                    D0.Foreground = "#ffadad".ToColor();
                    C.Foreground = "#e5ffad".ToColor();
                    D.Foreground = "#ffadfd".ToColor();
                    break;
                case 8:
                    C0.Foreground = "#e5ffad".ToColor();
                    D0.Foreground = "#ffadfd".ToColor();
                    C.Foreground = "#ffadad".ToColor();
                    D.Foreground = "#adfff9".ToColor();
                    break;
                case 9:
                    C0.Foreground = "#ffadad".ToColor();
                    D0.Foreground = "#adfff9".ToColor();
                    C.Foreground = "#ffadfb".ToColor();
                    D.Foreground = "#adffd4".ToColor();
                    break;
                case 10:
                    C0.Foreground = "#ffadfb".ToColor();
                    D0.Foreground = "#adffd4".ToColor();
                    C.Foreground = "#c0adff".ToColor();
                    D.Foreground = "#bbffad".ToColor();
                    break;
                case 11:
                    C0.Foreground = "#c0adff".ToColor();
                    D0.Foreground = "#bbffad".ToColor();
                    C.Foreground = "#ade0ff".ToColor();
                    D.Foreground = "#ebffad".ToColor();
                    break;
                case 12:
                    C0.Foreground = "#ade0ff".ToColor();
                    D0.Foreground = "#ebffad".ToColor();
                    C.Foreground = "#adffe9".ToColor();
                    D.Foreground = "#ffd9ad".ToColor();
                    break;
                case 13:
                    C0.Foreground = "#adffe9".ToColor();
                    D0.Foreground = "#ffd9ad".ToColor();
                    C.Foreground = "#adffbc".ToColor();
                    D.Foreground = "#ffadad".ToColor();
                    break;
                case 14:
                    C0.Foreground = "#adffbc".ToColor();
                    D0.Foreground = "#ffadad".ToColor();
                    C.Foreground = "#e5ffad".ToColor();
                    D.Foreground = "#ffadfd".ToColor();
                    break;
                case 15:
                    C0.Foreground = "#e5ffad".ToColor();
                    D0.Foreground = "#ffadfd".ToColor();
                    C.Foreground = "#ffadad".ToColor();
                    D.Foreground = "#adfff9".ToColor();
                    break;
                case 16:
                    C0.Foreground = "#ffadad".ToColor();
                    D0.Foreground = "#adfff9".ToColor();
                    C.Foreground = "#dfdfdf".ToColor();
                    D.Foreground = "#dfdfdf".ToColor();
                    break;
            }
        }

        private void Copy(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            Clipboard.SetText(textBlock.Text);
        }
    }
}
