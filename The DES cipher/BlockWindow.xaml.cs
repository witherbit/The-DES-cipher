using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using The_DES_cipher.DES;

namespace The_DES_cipher
{
    /// <summary>
    /// Логика взаимодействия для BlockWindow.xaml
    /// </summary>
    public partial class BlockWindow : Window
    {
        public BlockWindow(List<Block> blocks, bool isHex)
        {
            InitializeComponent();
            if (isHex)
                Title += " [Bytes]";
            else
                Title += " [Bits]";
            foreach (Block block in blocks)
            {
                stackPanelMain.Children.Add(new BlockControl(block, isHex, blocks.IndexOf(block)) { Margin = new Thickness(0, 10, 0, 10)});
            }
        }
    }
}
