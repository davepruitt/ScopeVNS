using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TxBDC_Common;

namespace ScopeVNS
{
    /// <summary>
    /// A base class that represents any type of picoscope this program can connect to
    /// </summary>
    public class PicoScope : NotifyPropertyChangedObject
    {
        #region Private data members

        private short _scope_handle = -1;
        private long _trigger_delay = -100;
        private long _recording_duration = 500_000;
        private long _nanoseconds_per_sample = 10;
        private double _trigger_voltage = 1.0d;
        private long _refractory_period = 0;
        private TriggerType _trigger_type = TriggerType.FallingEdge;
        protected BackgroundWorker _backgroundWorker;
        private ScopeState _scope_state = ScopeState.NotRecording;
        private TrialState _trial_state = TrialState.Idle;
        private short _timebase = -1;
        private string _serial_code = string.Empty;
        private string _booth_name = string.Empty;
        private StimulationTrain _most_recent_stim = null;

        private DisplayType _display_type = DisplayType.MostRecentTrace;
        private DisplayType_IndividualTrace _trace_display_type = DisplayType_IndividualTrace.EntireTrace;

        protected double MaxVoltage = 20;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a PicoScope object. This is a base class that can be used by all types of PicoScopes.
        /// </summary>
        /// <param name="scope_handle"></param>
        public PicoScope (short scope_handle)
        {
            //Set the oscilloscope handle
            ScopeHandle = scope_handle;
        }

        #endregion

        #region Public Properties

        public DisplayType_IndividualTrace ScopeTraceDisplayType
        {
            get
            {
                return _trace_display_type;
            }
            set
            {
                _trace_display_type = value;
            }
        }

        public DisplayType ScopeSessionDisplayType
        {
            get
            {
                return _display_type;
            }
            set
            {
                _display_type = value;
            }
        }

        public virtual int ScopeMaxValue
        {
            get
            {
                return 0;
            }
        }

        public virtual string ImagePath
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual PicoScopeType ScopeType
        {
            get
            {
                return PicoScopeType.Unknown;
            }
        }

        public short ScopeHandle
        {
            get
            {
                return _scope_handle;
            }
            set
            {
                _scope_handle = value;
            }
        }

        public long TriggerDelay
        {
            get
            {
                return _trigger_delay;
            }
            set
            {
                _trigger_delay = value;
            }
        }

        public long RecordingDuration
        {
            get
            {
                return _recording_duration;
            }
            set
            {
                _recording_duration = value;
            }
        }

        public long NanosecondsPerSample
        {
            get
            {
                return _nanoseconds_per_sample;
            }
            set
            {
                _nanoseconds_per_sample = value;
            }
        }

        public double TriggerVoltage
        {
            get
            {
                return _trigger_voltage;
            }
            set
            {
                _trigger_voltage = value;
            }
        }

        public TriggerType ScopeTriggerType
        {
            get
            {
                return _trigger_type;
            }
            set
            {
                _trigger_type = value;
            }
        }

        public ScopeState State
        {
            get
            {
                return _scope_state;
            }
            set
            {
                _scope_state = value;
            }
        }

        public TrialState TrialCollectionState
        {
            get
            {
                return _trial_state;
            }
            set
            {
                _trial_state = value;
            }
        }

        protected short Timebase
        {
            get
            {
                return _timebase;
            }
            set
            {
                _timebase = value;
            }
        }

        public string SerialCode
        {
            get
            {
                return _serial_code;
            }
            protected set
            {
                _serial_code = value;
            }
        }

        public string BoothName
        {
            get
            {
                return _booth_name;
            }
            set
            {
                _booth_name = value;
            }
        }

        public StimulationTrain MostRecentStimulationTrain
        {
            get
            {
                return _most_recent_stim;
            }
            protected set
            {
                _most_recent_stim = value;
            }
        }

        public long RefractoryPeriod
        {
            get
            {
                return _refractory_period;
            }
            set
            {
                _refractory_period = value;
            }
        }

        #endregion

        #region Background Worker Methods

        public void StartSession()
        {
            State = ScopeState.Recording;
            TrialCollectionState = TrialState.SetupForNextTrigger;
        }

        public void StopSession()
        {
            State = ScopeState.NotRecording;
            TrialCollectionState = TrialState.Idle;
        }

        public void StopBackgroundWorker()
        {
            if (_backgroundWorker != null)
            {
                if (_backgroundWorker.IsBusy)
                {
                    _backgroundWorker.CancelAsync();
                }
            }
        }

        public void StartBackgroundWorker()
        {
            //Set up the background thread
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.ProgressChanged += NotifyUIOfNewData;
            _backgroundWorker.RunWorkerCompleted += CancelBackgroundWorker;
            _backgroundWorker.DoWork += HandleStreaming;
            _backgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Sets triggering parameters for this oscilloscope
        /// </summary>
        /// <param name="trigger_delay">The trigger delay, in units of microseconds. This CAN be negative.</param>
        /// <param name="recording_duration">The total recording duration after the trigger, in units of microseconds. This CANNOT be negative.</param>
        /// <param name="microseconds_per_sample">The total number of microseconds represented by each sample that is collected.</param>
        /// <param name="trigger_voltage">The voltage at which we want to trigger the oscilloscope.</param>
        /// <param name="rp">The refractory period in microseconds</param>
        public void SetTriggeringParameters(long trigger_delay, long recording_duration, long desired_microseconds_per_sample, double trigger_voltage, TriggerType trigger_type, long rp)
        {
            //Set the default oscilloscope triggering properties
            TriggerDelay = trigger_delay;
            RecordingDuration = recording_duration;
            TriggerVoltage = trigger_voltage;
            ScopeTriggerType = trigger_type;
            RefractoryPeriod = rp;

            //Set the timebase
            var nearest_available_nanoseconds_per_sample = this.GetNearestNanosecondsPerSample(desired_microseconds_per_sample * 1000);
            NanosecondsPerSample = nearest_available_nanoseconds_per_sample;
            Timebase = this.GetTimebase(NanosecondsPerSample);
        }

        public virtual string FetchSerialCode ()
        {
            throw new NotImplementedException();
        }

        protected virtual short GetTimebase(Int64 nanoseconds_per_sample)
        {
            throw new NotImplementedException();
        }

        protected virtual List<Int64> GetAllPossibleNanosecondsPerSample()
        {
            throw new NotImplementedException();
        }

        protected virtual Int64 GetNearestNanosecondsPerSample(Int64 ns)
        {
            throw new NotImplementedException();
        }

        protected virtual void SetOscilloscopeChannel ()
        {
            throw new NotImplementedException();
        }

        protected virtual void SetupTrigger ()
        {
            throw new NotImplementedException();
        }

        protected virtual void RunBlock ()
        {
            throw new NotImplementedException();
        }

        protected virtual bool CheckIfReady ()
        {
            throw new NotImplementedException();
        }

        protected virtual short [] RetrieveOscilloscopeData ()
        {
            throw new NotImplementedException();
        }

        public virtual void ShutdownOscilloscope()
        {
            throw new NotImplementedException();
        }

        private void HandleStreaming(object sender, DoWorkEventArgs e)
        {
            Stopwatch refractory_period_timer = new Stopwatch();

            //Set the oscilloscope's channel
            SetOscilloscopeChannel();

            //Set up the trigger for this scope
            SetupTrigger();
            
            //Loop as long as the background thread has not been cancelled
            while (!_backgroundWorker.CancellationPending)
            {
                if (State == ScopeState.Recording)
                {
                    switch (TrialCollectionState)
                    {
                        case TrialState.SetupForNextTrigger:

                            RunBlock();
                            TrialCollectionState = TrialState.WaitForTrigger;

                            break;
                        case TrialState.WaitForTrigger:

                            bool r = CheckIfReady();
                            if (r)
                            {
                                TrialCollectionState = TrialState.FinalizingCollectionAfterTrigger;
                                refractory_period_timer.Restart();
                            }

                            break;
                        case TrialState.FinalizingCollectionAfterTrigger:

                            //Collect the most recent stimulation train
                            var values = RetrieveOscilloscopeData();
                            
                            //Create a StimulationTrain object to the store the data for the collected stim train
                            StimulationTrain t = new StimulationTrain();
                            t.StimulationTime = DateTime.Now;
                            t.Data = values.Select(d => MaxVoltage * (double)d / (double)this.ScopeMaxValue).ToList();

                            //Store the most recent stimulation train
                            MostRecentStimulationTrain = t;

                            //Notify that we have a new stimulation train
                            BackgroundPropertyChanged("MostRecentStimulationTrain");
                            
                            //Set the state to set up for a new stim train
                            TrialCollectionState = TrialState.WaitingOnRefractoryPeriod;

                            break;
                        case TrialState.WaitingOnRefractoryPeriod:

                            if (refractory_period_timer.ElapsedMilliseconds >= (RefractoryPeriod / 1000))
                            {
                                TrialCollectionState = TrialState.SetupForNextTrigger;
                                refractory_period_timer.Reset();
                            }

                            break;
                    }
                }

                //Notify the UI of any changes
                _backgroundWorker.ReportProgress(0);

                //Sleep the thread for a little while so we don't consume the CPU
                Thread.Sleep(33);
            }
        }

        private void CancelBackgroundWorker(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ScopeVNSMessagingSystem.GetInstance().AddMessage("Error detected on scope " + SerialCode + "! PLEASE RESTART SCOPEVNS!");
                ErrorLoggingService.GetInstance().LogExceptionError(e.Error);
            }
        }

        private void NotifyUIOfNewData(object sender, ProgressChangedEventArgs e)
        {
            lock (propertyNamesLock)
            {
                foreach (var name in propertyNames)
                {
                    NotifyPropertyChanged(name);
                }

                propertyNames.Clear();
            }
        }

        #endregion

        #region

        private List<string> propertyNames = new List<string>();
        private object propertyNamesLock = new object();

        private void BackgroundPropertyChanged(string name)
        {
            lock (propertyNamesLock)
            {
                propertyNames.Add(name);
            }
        }

        #endregion
    }
}
