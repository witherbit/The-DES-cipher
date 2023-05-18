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
using Block = The_DES_cipher.DES.Block;

namespace The_DES_cipher
{
    /// <summary>
    /// Логика взаимодействия для BlockControl.xaml
    /// </summary>
    public partial class BlockControl : UserControl
    {
        public BlockControl(Block block, bool isHex, int round)
        {
            InitializeComponent();
            rSource.Text = $"Input [{round}]";
            SetValue(Source, block.Source, isHex);
            SetValue(IP, block.IP, isHex);
            SetValue(L, block.L[15], isHex);
            SetValue(R, block.R[15], isHex);
            SetValue(LR, block.LR, isHex);
            SetValue(IPInverse, block.IPInverse, isHex);
            if(!isHex)
                LR.Text = LR.Text.Insert(32, " ");
            for (int i = 0; i < 16; i++)
            {
                if(i == 0)
                    Rounds.Children.Add(new RoundBlockControl(i, block.L0, block.R0, block.EP[i], block.RoundKey[i], block.XorToRoundKey[i], block.SBox[i], block.SBoxPermutation[i], isHex));
                else
                    Rounds.Children.Add(new RoundBlockControl(i, block.L[i - 1], block.R[i - 1], block.EP[i], block.RoundKey[i], block.XorToRoundKey[i], block.SBox[i], block.SBoxPermutation[i], isHex));
            }
        }
        private void SetValue(TextBlock textBlock, byte[] value, bool isHex)
        {
            if (isHex)
                textBlock.Text = value.BytesToString(" ", true).ToUpper();
            else
                textBlock.Text = value.BitsToString();
        }
        private void Copy(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            Clipboard.SetText(textBlock.Text);
        }
    }
}
