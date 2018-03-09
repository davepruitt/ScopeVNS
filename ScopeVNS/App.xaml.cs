using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ScopeVNS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string MutexName = "##||ScopeVNS||##";
        private readonly Mutex _mutex;
        bool createdNew;

        public App()
        {
            _mutex = new Mutex(true, MutexName, out createdNew);
            if (!createdNew)
            {
                MessageBox.Show("This program is already running. Please close all other instances and start again.");
                Application.Current.Shutdown(0);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!createdNew)
            {
                return;
            }
            else
            {
                //Start up the scope manager, connect to each scope, and load the configuration file
                var s_manager = ScopeManager.GetInstance();

                //Iterate over each scope and start a background thread for each
                foreach (var s in s_manager.Scopes)
                {
                    s.StartBackgroundWorker();
                }

                //Create the main window
                var mw = new ScopeVNS.MainWindow();
                mw.Show();
            }
        }
    }
}
