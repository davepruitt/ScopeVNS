using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ScopeVNS
{
    /// <summary>
    /// View-model class for an individual booth
    /// </summary>
    public class BoothViewModel : INotifyPropertyChanged
    {
        #region Private data members

        bool _close_permanently = false;

        public bool ClosePermanently
        {
            get
            {
                return _close_permanently;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new booth view-model object
        /// </summary>
        public BoothViewModel(PicoScope s)
        {
            _scope = s;
            _scope.PropertyChanged += _scope_PropertyChanged;
            
            //Create a plot model for this booth and give it a set of axes.
            BoothPlotModel = new PlotModel { Title = "VNS Traces" };
            LinearAxis y_axis = new LinearAxis();
            y_axis.Minimum = ScopeManager.GetInstance().PlotYLimit_Min;
            y_axis.Maximum = ScopeManager.GetInstance().PlotYLimit_Max;
            y_axis.Position = AxisPosition.Left;
            BoothPlotModel.Axes.Add(y_axis);
        }

        private void _scope_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("MostRecentStimulationTrain"))
            {
                //Record the new stim
                _trials.Add(_scope.MostRecentStimulationTrain);
                var most_recent_timestamp = DateTime.Now;
                var num_stims = _trials.Count;

                //Clear the plot
                BoothPlotModel.Series.Clear();

                //Obtain indices of trials we want to display
                List<int> _trace_indices = new List<int>();
                switch (_scope.ScopeSessionDisplayType)
                {
                    case DisplayType.AllTraces:
                        _trace_indices = Enumerable.Range(0, _trials.Count).ToList();
                        break;
                    case DisplayType.MostRecentTenTraces:
                        _trace_indices = Enumerable.Range(Math.Max(0, _trials.Count - 10), Math.Min(_trials.Count, 10)).ToList();
                        break;
                    case DisplayType.MostRecentTrace:
                        _trace_indices.Add(_trials.Count - 1);
                        break;
                }

                //Grab data from each trial, transform it, and create a LineSeries object to display it
                for (int i = 0; i < _trace_indices.Count; i++)
                {
                    //Grab the data from the stim train. Make sure to call ToList() to make a copy of it before we transform it
                    var idx = _trace_indices[i];
                    var train_data = _trials[idx].Data.ToList();

                    //Calculate how many elements to display from the current trace
                    int elements_to_take = train_data.Count;
                    switch (_scope.ScopeTraceDisplayType)
                    {
                        case DisplayType_IndividualTrace.FirstHundredSamples:
                            elements_to_take = Math.Min(100, train_data.Count);
                            break;
                        case DisplayType_IndividualTrace.FirstThousandSamples:
                            elements_to_take = Math.Min(1000, train_data.Count);
                            break;
                        case DisplayType_IndividualTrace.FirstTenth:
                            elements_to_take = Convert.ToInt32(0.1d * Convert.ToDouble(train_data.Count));
                            break;
                        case DisplayType_IndividualTrace.FirstQuarter:
                            elements_to_take = Convert.ToInt32(0.25d * Convert.ToDouble(train_data.Count));
                            break;
                    }

                    //Grab the elements we want to take and display
                    train_data = train_data.Take(elements_to_take).ToList();

                    //Plot this data
                    LineSeries new_line = new LineSeries();
                    for (int j = 0; j < train_data.Count; j++)
                    {
                        new_line.Points.Add(new DataPoint(j, train_data[j]));
                    }

                    BoothPlotModel.Series.Add(new_line);
                    BoothPlotModel.InvalidatePlot(true);
                }
                
                //Add a message to the list of messages
                Messages = Messages + most_recent_timestamp.ToString("HH:mm:ss") + " - Stimulation detected (" + num_stims.ToString() + ")\n";

                //Notify the GUI that the plot has changed
                NotifyPropertyChanged("BoothPlotModel");
            }
        }

        #endregion
        
        #region Private data members

        private PicoScope _scope = null;
        private string _ratName = string.Empty;
        private List<StimulationTrain> _trials = new List<StimulationTrain>();

        private Booth _boothWindow = null;
        private PlotModel _boothPlotModel = null;
        private bool _canBeRun = false;
        private bool _isRunning = false;
        private string _messages = string.Empty;
        
        #endregion

        #region Properties

        /// <summary>
        /// A string description of the pico scope type
        /// </summary>
        public string PicoScopeTypeDescription
        {
            get
            {
                return PicoScopeTypeConverter.ConvertToStringDescription(_scope.ScopeType);
            }
        }

        /// <summary>
        /// The path to the image for this scope
        /// </summary>
        public string ScopeImageSource
        {
            get
            {
                return _scope.ImagePath;
            }
        }

        /// <summary>
        /// The OxyPlot plot model for this booth
        /// </summary>
        public PlotModel BoothPlotModel
        {
            get
            {
                return _boothPlotModel;
            }
            set
            {
                _boothPlotModel = value;
            }
        }

        /// <summary>
        /// The number of this booth
        /// </summary>
        public string BoothNumber
        {
            get
            {
                return _scope.BoothName;
            }
        }

        /// <summary>
        /// The booth name, just the booth number with the word "Booth" in front of it.
        /// </summary>
        public string BoothName
        {
            get
            {
                return "Booth " + BoothNumber;
            }
        }

        /// <summary>
        /// The serial code of the oscilloscope
        /// </summary>
        public string ScopeSerialCode
        {
            get
            {
                return _scope.SerialCode;
            }
        }

        /// <summary>
        /// The rat name for this booth
        /// </summary>
        public string RatName
        {
            get
            {
                return _ratName;
            }
            set
            {
                _ratName = value;
                _ratName = _ratName.Trim();
                _ratName = TxBDC_Common.StringHelperMethods.CleanInput(_ratName);
                _ratName = _ratName.ToUpper();

                _canBeRun = true;

                NotifyPropertyChanged("RatName");
                NotifyPropertyChanged("StartButtonEnabled");
                NotifyPropertyChanged("StartButtonTextColor");
            }
        }

        /// <summary>
        /// Whether or not a session is currently running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                _isRunning = value;

                if (!_isRunning)
                {
                    _canBeRun = false;
                }

                NotifyPropertyChanged("IsRunning");
                NotifyPropertyChanged("StartButtonText");
                NotifyPropertyChanged("StartButtonTextColor");
                NotifyPropertyChanged("StartButtonEnabled");
            }
        }

        /// <summary>
        /// The serial code of this scope
        /// </summary>
        public string ScopeID
        {
            get
            {
                return _scope.SerialCode;
            }
        }

        /// <summary>
        /// A list of messages to be displayed to the user.
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

        /// <summary>
        /// Whether or not the start button should be enabled
        /// </summary>
        public bool StartButtonEnabled
        {
            get
            {
                return (_canBeRun && (!string.IsNullOrEmpty(RatName)));
            }
        }

        /// <summary>
        /// The color of the start button
        /// </summary>
        public SolidColorBrush StartButtonTextColor
        {
            get
            {
                if (StartButtonEnabled)
                {
                    if (IsRunning)
                    {
                        return new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        return new SolidColorBrush(Colors.Green);
                    }

                }
                else
                {
                    return new SolidColorBrush(Colors.DarkGray);
                }
            }
        }

        /// <summary>
        /// The text of the start button
        /// </summary>
        public string StartButtonText
        {
            get
            {
                if (IsRunning)
                {
                    return "STOP";
                }
                else
                {
                    return "START";
                }
            }
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

        #region Interaction with scope object

        public void Run()
        {
            //Clear the plot
            BoothPlotModel.Series.Clear();
            BoothPlotModel.InvalidatePlot(true);

            //Clear the trials array
            _trials.Clear();

            //Tell the scope we are running
            _scope.StartSession();

            //Add a message saying we are running
            Messages = "Running...\n";
        }

        public void Stop()
        {
            //Get the scope manager
            var scopeManager = ScopeManager.GetInstance();
            
            //Get the user's desired us/sample
            uint desired_us_per_sample = Convert.ToUInt32(this._scope.NanosecondsPerSample / 1000.0d);

            //For streaming mode, simply set the actual us/sample equal to the desired us/sample
            uint actual_us_per_sample = desired_us_per_sample;
            
            //Display peak voltage data to the user
            DisplayPeakVoltageData(_trials);

            //Save data
            try
            {
                Messages = Messages + "Saving data to primary path...";
                SaveScopeData.SaveData(scopeManager.PrimaryDataPath, this.RatName, _scope.BoothName, _scope.SerialCode, "A", _trials, actual_us_per_sample);
                Messages = Messages + "done\n";
            }
            catch
            {
                Messages = Messages + "\n";
                Messages = Messages + "Error while trying to save to primary data path!!!";
            }

            try
            {
                Messages = Messages + "Saving data to secondary path...";
                SaveScopeData.SaveData(scopeManager.SecondaryDataPath, this.RatName, _scope.BoothName, _scope.SerialCode, "A", _trials, actual_us_per_sample);
                Messages = Messages + "done\n";
            }
            catch
            {
                Messages = Messages + "\n";
                Messages = Messages + "Error while trying to save to primary data path!!!";
            }

            //Stop streaming from the scope (this function call may be uneccessary, i'm not sure)
            _scope.StopSession();
        }

        private void DisplayPeakVoltageData(List<StimulationTrain> trials)
        {
            if (trials.Count > 0)
            {
                //Calculate the median max voltage
                var max_voltages = trials.Select(d => d.Data.Max()).ToList();
                double median_max = TxBDC_Common.TxBDC_Math.Median(max_voltages);

                //Calculate the median min voltage
                var min_voltages = trials.Select(d => d.Data.Min()).ToList();
                double median_min = TxBDC_Common.TxBDC_Math.Median(min_voltages);

                //Calculate the median peak-to-peak voltage
                var peak_to_peak_voltages = max_voltages.Zip(min_voltages, (one, two) => one - two).ToList();
                double median_pk_to_pk = TxBDC_Common.TxBDC_Math.Median(peak_to_peak_voltages);

                //Send that data to the messages
                Messages = Messages + "Max voltage (median): " + median_max.ToString("#.##") + "\n";
                Messages = Messages + "Peak-to-peak voltage (median): " + median_pk_to_pk.ToString("#.##") + "\n";
            }
        }

        #endregion

        #region Window management

        public void ShowBoothWindow ()
        {
            if (_boothWindow == null)
            {
                _boothWindow = new Booth();
                _boothWindow.DataContext = this;
                _boothWindow.Show();
            }

            _boothWindow.Visibility = System.Windows.Visibility.Visible;
        }

        public void CloseBoothWindow ()
        {
            if (_boothWindow != null)
            {
                _boothWindow.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public void CloseBoothWindowPermanently ()
        {
            if (_boothWindow != null)
            {
                _close_permanently = true;
                _boothWindow.Close();
            }
        }

        #endregion
    }
}
