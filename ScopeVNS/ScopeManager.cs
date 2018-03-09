using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeVNS
{
    /// <summary>
    /// This is a singleton class that will manage all of the oscilloscopes connected to the computer.
    /// </summary>
    public class ScopeManager
    {
        #region Singleton

        private static ScopeManager _instance = null;
        private static Object syncRoot = new Object();

        /// <summary>
        /// Singleton constructor
        /// </summary>
        private ScopeManager()
        {
            //Connect to all of the oscilloscopes
            ConnectToScopes();

            //Load the configuration parameters for each scope. This should be done AFTER connecting to the oscilloscopes.
            LoadConfigurationParameters();
        }

        /// <summary>
        /// Returns the singleton instance of the scope manager
        /// </summary>
        /// <returns>ScopeManager object</returns>
        public static ScopeManager GetInstance ()
        {
            if (_instance == null)
            {
                lock (syncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new ScopeManager();
                    }
                }
            }
            
            return _instance;
        }

        #endregion

        #region Private data members

        private string _configFile = "scopevns.config";
        private List<PicoScope> _scopes = new List<PicoScope>();

        #endregion

        #region Public data members

        public double PlotYLimit_Min = -20.0d;
        public double PlotYLimit_Max = 20.0d;
        public string PrimaryDataPath = string.Empty;
        public string SecondaryDataPath = string.Empty;
        public string GroupID = string.Empty;
        public bool IsPassiveCollectionEnabled = false;
        
        /// <summary>
        /// The collection of all of the oscilloscopes
        /// </summary>
        public List<PicoScope> Scopes
        {
            get
            {
                return _scopes;
            }
            private set
            {
                _scopes = value;
            }
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Converts a string of the form "number units" to a signed 64-bit integer in units of microseconds.
        /// Examples:
        /// Input: "1000 us", Output: 1000
        /// Input: "1 ms", Output: 1000
        /// Input: "1000000 ns", Output: 1000
        /// Acceptable units for the parameter are "ns", "us", "ms", and "s".
        /// </summary>
        /// <param name="n">String parameter to be converted to a 64-bit integer in units of microseconds</param>
        /// <returns>Signed 64-bit integer representing the number of microseconds that was represented in the string parameter</returns>
        private long ConvertStringNumberWithUnitsToMicroseconds (string n)
        {
            //Split the string where there is a space
            var n_parts = n.Split(new char[] { ' ' });

            var n_number = n_parts[0].Trim();
            var n_units = n_parts[1].Trim();

            //Try to parse the number specified in the string parameter
            Int64.TryParse(n_number, out long result);
            
            if (n_units.Equals("s", StringComparison.OrdinalIgnoreCase))
            {
                result = result * 1000 * 1000;
            }
            else if (n_units.Equals("ms", StringComparison.OrdinalIgnoreCase))
            {
                result = result * 1000;
            }
            else if (n_units.Equals("us", StringComparison.OrdinalIgnoreCase))
            {
                //do nothing
            }
            else if (n_units.Equals("ns", StringComparison.OrdinalIgnoreCase))
            {
                result = result / 1000;
            }

            return result;
        }

        private void LoadConfigurationParameters()
        {
            //Read the config file
            var config_file_lines = TxBDC_Common.ConfigurationFileLoader.LoadConfigurationFile(_configFile);

            //Only go on if there are lines in the config file
            if (config_file_lines.Count > 0)
            {
                //Before doing on, let's make sure the config file is the correct version
                var first_line_parts = config_file_lines[0].Split(new char[] { ':' }, 2).ToList();
                if (first_line_parts[0].Trim().Equals("Version", StringComparison.OrdinalIgnoreCase) && first_line_parts[1].Trim().Equals("5"))
                {
                    //Iterate over every line and parse the config file
                    foreach (var line in config_file_lines)
                    {
                        //Fetch the parts of this line
                        var this_line_parts = line.Split(new char[] { ':' }, 2).ToList();
                        var line_identifier = this_line_parts[0];
                        if (line_identifier.Equals("Booth Definition", StringComparison.OrdinalIgnoreCase))
                        {
                            var scope_parameters = this_line_parts[1];
                            var scope_parameters_split = scope_parameters.Split(new char[] { ',' }).ToList();

                            string serial_code = scope_parameters_split[0].Trim();
                            string booth_name = scope_parameters_split[1].Trim();
                            string pre_trig = scope_parameters_split[2].Trim();
                            string post_trig = scope_parameters_split[3].Trim();
                            string sample_duration = scope_parameters_split[4].Trim();
                            string trig_voltage = scope_parameters_split[5].Trim();
                            string trig_type = scope_parameters_split[6].Trim();
                            string refractory_period = scope_parameters_split[7].Trim();
                            string display_type_1 = scope_parameters_split[8].Trim();
                            string display_type_2 = scope_parameters_split[9].Trim();
                            
                            long pre_trig_micros = ConvertStringNumberWithUnitsToMicroseconds(pre_trig);
                            long post_trig_micros = ConvertStringNumberWithUnitsToMicroseconds(post_trig);
                            long sample_dur_micros = ConvertStringNumberWithUnitsToMicroseconds(sample_duration);
                            long refractory_period_micros = ConvertStringNumberWithUnitsToMicroseconds(refractory_period);

                            Double.TryParse(trig_voltage, out double trigger_voltage);
                            Enum.TryParse(trig_type, out TriggerType trigger_type);
                            Enum.TryParse(display_type_1, out DisplayType session_display_type);
                            Enum.TryParse(display_type_2, out DisplayType_IndividualTrace trace_display_type);

                            //Fetch the correct scope
                            var scope_object = Scopes.Where(x => x.SerialCode.Equals(serial_code)).FirstOrDefault();
                            if (scope_object != null)
                            {
                                scope_object.SetTriggeringParameters(pre_trig_micros, post_trig_micros, sample_dur_micros, trigger_voltage, trigger_type, refractory_period_micros);
                                scope_object.BoothName = booth_name;
                                scope_object.ScopeSessionDisplayType = session_display_type;
                                scope_object.ScopeTraceDisplayType = trace_display_type;
                            }
                        }
                        else if (line_identifier.Equals("Primary Path", StringComparison.OrdinalIgnoreCase))
                        {
                            this.PrimaryDataPath = this_line_parts[1].Trim();
                        }
                        else if (line_identifier.Equals("Secondary Path", StringComparison.OrdinalIgnoreCase))
                        {
                            this.SecondaryDataPath = this_line_parts[1].Trim();
                        }
                        else if (line_identifier.Equals("Group ID", StringComparison.OrdinalIgnoreCase))
                        {
                            this.GroupID = this_line_parts[1].Trim();
                        }
                        else if (line_identifier.Equals("Enable Passive Collection", StringComparison.OrdinalIgnoreCase))
                        {
                            Boolean.TryParse(this_line_parts[1], out this.IsPassiveCollectionEnabled);
                        }
                    }
                }
            }
        }
        
        private void ConnectToScopes()
        {
            bool done = false;
            short scopeHandle = 0;

            //Open all the 2204A PicoScope units
            while (!done)
            {
                //Open a connection to an oscilloscope
                scopeHandle = PicoScopeLibrary.Imports.OpenUnit();

                if (scopeHandle <= 0)
                {
                    done = true;
                }
                else
                {
                    PicoScope_2204A s = new PicoScope_2204A(scopeHandle);
                    Scopes.Add(s);
                }
            }

            //Now open all the 2206B PicoScope units
            done = false;

            while (!done)
            {
                var success = PS2000ACSConsole.Imports.OpenUnit(out scopeHandle, null);

                if (scopeHandle <= 0)
                {
                    done = true;
                }
                else
                {
                    PicoScope_2206B s2 = new PicoScope_2206B(scopeHandle);
                    Scopes.Add(s2);
                }
            }

            //Iterate through each scope and get its serial code.
            foreach (var s in Scopes)
            {
                s.FetchSerialCode();
            }
        }

        #endregion
    }
}
