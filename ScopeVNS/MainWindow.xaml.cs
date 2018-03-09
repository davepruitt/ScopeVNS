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

using PicoScopeLibrary;
using System.ComponentModel;

namespace ScopeVNS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Get an instance of the scope manager.  This should also create the first instance, 
            //which should also configure all of the oscilloscopes.
            var scopeManager = ScopeManager.GetInstance();
            
            //Start streaming on all scopes
            foreach (PicoScope s in scopeManager.Scopes)
            {
                //Create a view model for the booth/scope
                BoothViewModel vm = new BoothViewModel(s);

                //Add the booth/scope to the items control of this window
                BoothList.Items.Add(vm);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                BoothViewModel vm = b.DataContext as BoothViewModel;

                if (vm != null)
                {
                    vm.ShowBoothWindow();
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Close all booth windows
            foreach (var i in this.BoothList.Items)
            {
                var vm = i as BoothViewModel;
                if (vm != null)
                {
                    vm.CloseBoothWindowPermanently();
                }
            }

            //Get an instance of the scope manager.  This should also create the first instance, 
            //which should also configure all of the oscilloscopes.
            var scopeManager = ScopeManager.GetInstance();

            //Stop streaming on all scopes
            foreach (PicoScope s in scopeManager.Scopes)
            {
                s.ShutdownOscilloscope();
            }

            //Close all file streams from the booth save manager
            BoothSaveManager b = BoothSaveManager.GetInstance();
            b.CloseAllStreams();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MessagesTextBox.Focus();
            MessagesTextBox.CaretIndex = MessagesTextBox.Text.Length;
            MessagesTextBox.ScrollToEnd();
        }
    }
}
