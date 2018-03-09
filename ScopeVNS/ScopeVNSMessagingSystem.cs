using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeVNS
{
    /// <summary>
    /// Singleton class implementing a global messaging system for the ScopeVNS program
    /// </summary>
    public class ScopeVNSMessagingSystem :  INotifyPropertyChanged
    {
        #region Singleton class

        private static ScopeVNSMessagingSystem _instance = null;

        private ScopeVNSMessagingSystem()
        {
            //empty
        }

        public static ScopeVNSMessagingSystem GetInstance ()
        {
            if (_instance == null)
            {
                _instance = new ScopeVNSMessagingSystem();
            }

            return _instance;
        }

        #endregion

        #region Private data members

        private string _messages = string.Empty;

        #endregion

        #region Public data members

        /// <summary>
        /// The messages currently in the system
        /// </summary>
        public string Messages
        {
            get
            {
                return _messages;
            }
            set
            {
                _messages = value;
                NotifyPropertyChanged("Messages");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a message
        /// </summary>
        public void AddMessage (string msg)
        {
            Messages += msg + "\n";
        }

        /// <summary>
        /// Clears all messages in the system
        /// </summary>
        public void ClearMessages ()
        {
            Messages = string.Empty;
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
