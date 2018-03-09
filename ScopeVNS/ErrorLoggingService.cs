using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeVNS
{
    /// <summary>
    /// This is the ScopeVNS error logging service.  It logs errors out to a file when it detects them
    /// </summary>
    public class ErrorLoggingService
    {
        #region Singleton class

        private static ErrorLoggingService _instance = null;

        private ErrorLoggingService ()
        {
            //empty constructor
        }

        public static ErrorLoggingService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ErrorLoggingService();
            }

            return _instance;
        }

        #endregion

        #region Private data members

        private string _error_log_file = "errors.txt";
        private object _error_log_lock = new object();

        #endregion

        #region Method to log errors

        /// <summary>
        /// Logs an exception to the error log file
        /// </summary>
        public void LogExceptionError ( Exception e )
        {
            lock(_error_log_lock)
            {
                try
                {
                    StreamWriter writer = new StreamWriter(_error_log_file, true);
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd:HH:mm:ss");
                    string stacktrace = e.StackTrace;
                    string outermost_exception = e.Message;

                    string innermost_exception = string.Empty;
                    var base_except = e.GetBaseException();
                    if (base_except != null)
                    {
                        innermost_exception = base_except.Message;
                    }

                    string function = e.TargetSite.Name;

                    writer.WriteLine("NEW ERROR DETECTED");
                    writer.WriteLine(timestamp);
                    writer.WriteLine("Stack trace: " + stacktrace);
                    writer.WriteLine("Outermost exception message: " + outermost_exception);
                    writer.WriteLine("Innermost exception message: " + innermost_exception);
                    writer.WriteLine("Function name: " + function);
                    writer.WriteLine("END OF NEW ERROR");
                    writer.WriteLine();

                    writer.Close();
                }
                catch
                {
                    ScopeVNSMessagingSystem.GetInstance().AddMessage("Unable to log error to file");
                }
            }
        }

        /// <summary>
        /// Logs a string to the error log file.
        /// </summary>
        public void LogStringError (string msg)
        {
            lock(_error_log_lock)
            {
                try
                {
                    StreamWriter writer = new StreamWriter(_error_log_file, true);
                    writer.WriteLine(msg);
                    writer.Close();
                }
                catch
                {
                    ScopeVNSMessagingSystem.GetInstance().AddMessage("Unable to log error to file");
                }
            }
        }

        #endregion
    }
}
