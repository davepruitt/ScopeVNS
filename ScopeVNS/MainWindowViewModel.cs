using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ScopeVNS
{
    /// <summary>
    /// This is the view-model class for the main window.
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindowViewModel()
        {
            //Subscribe to receive notifications from the ScopeVNS messaging system
            ScopeVNSMessagingSystem.GetInstance().PropertyChanged += ReactToNotificationsFromMessagingSystem;
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// This returns the background color of the window.
        /// The background color may change in the event of an error being detected.
        /// </summary>
        public SolidColorBrush WindowBackgroundColor
        {
            get
            {
                var msgs = ScopeVNSMessagingSystem.GetInstance().Messages;
                if (!string.IsNullOrEmpty(msgs))
                {
                    return new SolidColorBrush(Colors.Red);
                }
                else
                {
                    return new SolidColorBrush(Colors.White);
                }
            }
        }

        /// <summary>
        /// Global messages to be displayed to the user in the main window
        /// </summary>
        public string Messages
        {
            get
            {
                return ScopeVNSMessagingSystem.GetInstance().Messages;
            }
        }

        #endregion

        #region Private methods

        private void ReactToNotificationsFromMessagingSystem(object sender, PropertyChangedEventArgs e)
        {
            //Notify the UI that the Messages property has changed
            NotifyPropertyChanged("Messages");

            //The background color of the window may change with the messages system
            NotifyPropertyChanged("WindowBackgroundColor");
        }

        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}
