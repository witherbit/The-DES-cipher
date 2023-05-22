using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Логика взаимодействия для FilePage.xaml
    /// </summary>
    public partial class FilePage : Page
    {
        public Des Des { get; private set; }
        public FilePage()
        {
            InitializeComponent();
            Des = new Des();
            comboBoxMode.SelectedIndex = 0;
            comboBoxEncoding.SelectedIndex = 1;
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "All files (*.*)|*.*|Text files (*.txt)|*.txt|DES files (*.des)|*.des";
                ofd.Title = "Открыть файл";
                if (!(bool)ofd.ShowDialog())
                    return;
                textBoxFile.Text = ofd.FileName;
                FileInfo info = new FileInfo(ofd.FileName);
                uiFileSize.Text = FileSizeToString(info.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Не удалось открыть файл");
            }
            textBox_PreviewTextInput(sender, null);
        }
        private void comboBoxMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (comboBoxMode.SelectedIndex)
            {
                case 0:
                    Des.Mode = Mode.ECB;
                    break;
                case 1:
                    Des.Mode = Mode.CBC;
                    break;
            }
            textBox_PreviewTextInput(sender, null);
        }

        private void comboBoxEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (comboBoxEncoding.SelectedIndex)
            {
                case 0:
                    Des.Encoding = Encoding.ASCII;
                    break;
                case 1:
                    Des.Encoding = Encoding.UTF8;
                    break;
                case 2:
                    Des.Encoding = Encoding.Unicode;
                    break;
            }
        }
        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.Pages.Content = null;
        }

        private void buttonEncrypt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "DES files (*.des)|*.des";
                sfd.Title = "Зашифровать файл";
                if (!(bool)sfd.ShowDialog())
                    return;
                Des.Key = textBoxPassword.Text;
                if (Des.Mode == Mode.CBC)
                {
                    Des.IV = textBoxIV.Text;
                }
                File.WriteAllBytes(sfd.FileName , Convert.FromBase64String(Des.Encrypt(Convert.ToBase64String(File.ReadAllBytes(textBoxFile.Text)))));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Не удалось записать файл");
            }
            textBox_PreviewTextInput(sender, null);
        }

        private void buttonDecrypt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "All files (*.*)|*.*|Text files (*.txt)|*.txt";
                sfd.Title = "Расшифровать файл";
                if (!(bool)sfd.ShowDialog())
                    return;
                Des.Key = textBoxPassword.Text;
                if (Des.Mode == Mode.CBC)
                {
                    Des.IV = textBoxIV.Text;
                }
                File.WriteAllBytes(sfd.FileName, Convert.FromBase64String(Des.Decrypt(Convert.ToBase64String(File.ReadAllBytes(textBoxFile.Text)))));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Не удалось записать файл");
            }
            textBox_PreviewTextInput(sender, null);
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            textBoxFile.Text = string.Empty;
            textBoxPassword.Text = string.Empty;
            textBoxIV.Text = string.Empty;
            uiFileSize.Text = "0 байт";
            textBox_PreviewTextInput(sender, null);
        }
        private void textBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            textBox_PreviewTextInput(sender, null);
        }

        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!textBoxFile.Text.IsNullOrEmpty() && !textBoxPassword.Text.IsNullOrEmpty() && textBoxPassword.Text.Length > 7)
            {
                if (Des.Mode == Mode.ECB || (Des.Mode == Mode.CBC && !textBoxIV.Text.IsNullOrEmpty()))
                {
                    buttonEncrypt.IsEnabled = true;
                    buttonDecrypt.IsEnabled = true;
                }
                else
                {
                    buttonEncrypt.IsEnabled = false;
                    buttonDecrypt.IsEnabled = false;
                }
            }
            else
            {
                buttonEncrypt.IsEnabled = false;
                buttonDecrypt.IsEnabled = false;
            }
        }

        static string FileSizeToString(long byteCount)
        {
            string[] suf = { "байт", "КБ", "МБ", "ГБ", "ТБ", "ПБ", "ЕБ" }; //
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }
    }
}
