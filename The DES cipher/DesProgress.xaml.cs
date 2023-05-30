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
    /// Логика взаимодействия для DesProgress.xaml
    /// </summary>
    public partial class DesProgress : UserControl
    {
        public event EventHandler<Exception> OnStop;
        string Save { get; set; }
        string Source { get; set; }
        int Count { get; set; }
        Des Des { get; set; }
        bool IsDecrypt { get; set; }
        public DesProgress(string source, string savefilename, bool isDecryption, Des des)
        {
            InitializeComponent();
            Save = savefilename;
            Source = source;
            IsDecrypt = isDecryption;
            Des = new Des();
            Des.Key = des.Key;
            Des.IV = des.IV;
            Des.Encoding = des.Encoding;
            Des.Mode = des.Mode;
            uiFileName.Text = System.IO.Path.GetFileName(Save);
        }

        public async void Start()
        {
            await Task.Run(() =>
            {
                Des.Start += (sender, e) =>
                {
                    this.Invoke(() =>
                    {
                        Count = e;
                        uiBlocksCount.Text = $"0 / {Count} блоков";
                    });
                };
                Des.BlockProcess += (sender, e) =>
                {
                    this.Invoke(() =>
                    {
                        uiBlocksCount.Text = $"{e} / {Count} блоков";
                        uiProgressBar.Value = GetPercentage(e, Count);
                    });
                };
                Des.Stop += Stop;
                if (IsDecrypt)
                {
                    try
                    {
                        Des.FileDecrypt(Source, Save);
                    }
                    catch (Exception ex)
                    {
                        Stop(this, ex);
                    }
                    
                }
                else
                {
                    try
                    {
                        Des.FileEncrypt(Source, Save);
                    }
                    catch (Exception ex)
                    {
                        Stop(this, ex);
                    }
                }
            });
        }

        private void Stop(object sender, Exception e)
        {
            this.Invoke(() =>
            {
                OnStop?.Invoke(this, e);
            });
        }
        private double GetPercentage(int current, int max)
        {
            return 100d * current / max;
        }
    }
}
