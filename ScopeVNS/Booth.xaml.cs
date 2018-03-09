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

namespace ScopeVNS
{
    /// <summary>
    /// Interaction logic for Booth.xaml
    /// </summary>
    public partial class Booth : Window
    {
        public Booth()
        {
            InitializeComponent();
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            //Start or stop collecting data
            BoothViewModel viewModel = this.DataContext as BoothViewModel;
            viewModel.IsRunning = !viewModel.IsRunning;

            if (viewModel.IsRunning)
            {
                viewModel.Run();
            }
            else
            {
                viewModel.Stop();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //empty
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MessagesTextBox.Focus();
            MessagesTextBox.CaretIndex = MessagesTextBox.Text.Length;
            MessagesTextBox.ScrollToEnd();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BoothViewModel viewModel = this.DataContext as BoothViewModel;
            if (viewModel != null)
            {
                if (!viewModel.ClosePermanently)
                {
                    e.Cancel = true;
                    viewModel.CloseBoothWindow();
                }
            }
        }
    }
}
