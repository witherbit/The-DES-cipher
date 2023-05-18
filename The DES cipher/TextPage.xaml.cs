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
    /// Логика взаимодействия для TextPage.xaml
    /// </summary>
    public partial class TextPage : Page
    {
        public Des Des { get; private set; }
        private byte[] _lastResult { get; set; }
        public TextPage()
        {
            InitializeComponent();
            Des = new Des();
            comboBoxMode.SelectedIndex = 0;
            comboBoxEncoding.SelectedIndex = 0;
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
            Des.Key = textBoxPassword.Text;
            if(Des.Mode == Mode.CBC)
            {
                Des.IV = textBoxIV.Text;
            }
            _lastResult = Des.Encryption(textBoxInput.Text.StringToBits(Des.Encoding));
            SetOutput();
            textBox_PreviewTextInput(sender, null);
        }

        private void buttonDecrypt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Des.Key = textBoxPassword.Text;
                if (Des.Mode == Mode.CBC)
                {
                    Des.IV = textBoxIV.Text;
                }
                textBoxOutput.Text = Des.Decrypt(textBoxInput.Text);
                _lastResult = null;
                textBox_PreviewTextInput(sender, null);
            }
            catch
            {
                MessageBox.Show("Невозможно расшифровать строку, так как та имела неверный формат.", "Ошибка");
            }
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            textBoxInput.Text = string.Empty;
            textBoxPassword.Text = string.Empty;
            textBoxIV.Text = string.Empty;
            textBoxOutput.Text = string.Empty;
            textBox_PreviewTextInput(sender, null);
        }

        private void buttonCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textBoxOutput.Text);
        }

        private void buttonKeys_Click(object sender, RoutedEventArgs e)
        {
            if (Des.RoundKeys == null)
                return;
            var window = new RoundKeysWindow(Des, (bool)checkBoxBitBytesTable.IsChecked);
            window.Show();
        }

        private void buttonBlocks_Click(object sender, RoutedEventArgs e)
        {
            if (Des.Blocks == null)
                return;
            var window = new BlockWindow(Des.Blocks, (bool)checkBoxBitBytesTable.IsChecked);
            window.Show();
        }

        private void textBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            textBox_PreviewTextInput(sender, null);
        }

        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(!textBoxInput.Text.IsNullOrEmpty() && !textBoxPassword.Text.IsNullOrEmpty() && textBoxPassword.Text.Length > 7)
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
            if (!textBoxOutput.Text.IsNullOrEmpty())
            {
                buttonCopy.IsEnabled = true;
                buttonBlocks.IsEnabled = true;
                buttonKeys.IsEnabled = true;
            }
            else
            {
                buttonCopy.IsEnabled = false;
                buttonBlocks.IsEnabled = false;
                buttonKeys.IsEnabled = false;
            }
        }

        private void checkBoxBytesOutput_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxBytesOutput.Content = "Вывод в байтах";
            SetOutput();
        }

        private void checkBoxBytesOutput_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxBytesOutput.Content = "Вывод в Base64";
            SetOutput();
        }

        private void SetOutput()
        {
            if (_lastResult == null)
                return;
            if ((bool)checkBoxBytesOutput.IsChecked)
                textBoxOutput.Text = string.Concat(_lastResult.Select(x => x.ToString("X2") + " "));
            else
                textBoxOutput.Text = Convert.ToBase64String(_lastResult);
        }

        private void checkBoxBitBytesTable_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxBitBytesTable.Content = "Таблица в байтах";
        }

        private void checkBoxBitBytesTable_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxBitBytesTable.Content = "Таблица в битах";
        }
    }
}
