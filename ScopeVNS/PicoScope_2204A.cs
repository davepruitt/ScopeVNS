using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeVNS
{
    public class PicoScope_2204A : PicoScope
    {
        #region Constructor

        public PicoScope_2204A(short scope_handle) : base(scope_handle)
        {
            //empty
        }

        #endregion

        #region Properties

        public override string ImagePath
        {
            get
            {
                return "./Images/2204A.png";
            }
        }

        public override PicoScopeType ScopeType
        {
            get
            {
                return PicoScopeType.PicoScope_2204A;
            }
        }

        public override int ScopeMaxValue
        {
            get
            {
                return PicoScopeLibrary.Imports.MaxValue;
            }
        }
        
        #endregion

        #region Methods

        public override string FetchSerialCode()
        {
            SerialCode = PicoScopeLibrary.ScopeUtilities.GetScopeID(ScopeHandle);
            return SerialCode;
        }

        protected override short GetTimebase ( Int64 nanoseconds_per_sample )
        {
            double ns = Convert.ToDouble(nanoseconds_per_sample);
            double t = Math.Log(ns / 10.0d, 2);
            return Convert.ToInt16(t);
        }

        protected override List<Int64> GetAllPossibleNanosecondsPerSample ()
        {
            List<Int64> v = new List<Int64>();
            for (int i = 0; i < 25; i++)
            {
                v.Add(Convert.ToInt64(10 * Math.Pow(2, i)));
            }

            return v;
        }

        protected override Int64 GetNearestNanosecondsPerSample ( Int64 ns )
        {
            var v = this.GetAllPossibleNanosecondsPerSample();
            var nearest = v.OrderBy(x => Math.Abs(x - ns)).First();
            return nearest;
        }
        
        public override void ShutdownOscilloscope()
        {
            //Close down the pico scope
            PicoScopeLibrary.Imports.Stop(ScopeHandle);
            PicoScopeLibrary.Imports.CloseUnit(ScopeHandle);

            //Shutdown the background thread
            StopBackgroundWorker();
        }

        protected override void SetOscilloscopeChannel()
        {
            short success = PicoScopeLibrary.Imports.SetChannel(ScopeHandle, PicoScopeLibrary.Imports.Channel.ChannelA,
                1, 1, PicoScopeLibrary.Imports.Range.Range_20V);
        }

        protected override void SetupTrigger()
        {
            var pre_samples_to_record = (this.TriggerDelay * 1000) / this.NanosecondsPerSample;
            var post_samples_to_record = (this.RecordingDuration * 1000) / this.NanosecondsPerSample;
            var total_samples_to_record = pre_samples_to_record + post_samples_to_record;

            //Calculate the trigger delay
            //Delay is in units of "percent of total recorded samples". In other words, a 10% negative delay would have 10% of the signal on
            //the left of the trigger, and 90% on the right of the trigger.
            var delay = Convert.ToInt16(Math.Round((Convert.ToDouble(pre_samples_to_record) / Convert.ToDouble(total_samples_to_record)) * -100));
            
            //Calculate the trigger threshold.  It is supposed to be a value in the range of [0, Int16.MaxValue], where Int16.MaxValue
            //is equal to the maximum voltage we have set the scope to (which should be 20 V).
            short threshold = Convert.ToInt16(Math.Round(Convert.ToDouble(PS2000ACSConsole.Imports.MaxValue) * (TriggerVoltage / MaxVoltage)));
            
            //Set the trigger direction
            PicoScopeLibrary.Imports.ThresholdDirection d = PicoScopeLibrary.Imports.ThresholdDirection.Above;
            switch (this.ScopeTriggerType)
            {
                case TriggerType.RisingEdge:
                    d = PicoScopeLibrary.Imports.ThresholdDirection.Rising;
                    break;
                case TriggerType.FallingEdge:
                    d = PicoScopeLibrary.Imports.ThresholdDirection.Falling;
                    break;
                case TriggerType.Above:
                    d = PicoScopeLibrary.Imports.ThresholdDirection.Above;
                    break;
                case TriggerType.Below:
                    d = PicoScopeLibrary.Imports.ThresholdDirection.Below;
                    break;
            }
            
            //Set the trigger
            short success = PicoScopeLibrary.Imports.SetTrigger(
                ScopeHandle,
                (short)PicoScopeLibrary.Imports.Channel.ChannelA,
                threshold,
                (short)d,
                delay,
                0
                );
        }

        protected override void RunBlock()
        {
            var pre_samples_to_record = (this.TriggerDelay * 1000) / this.NanosecondsPerSample;
            var post_samples_to_record = (this.RecordingDuration * 1000) / this.NanosecondsPerSample;
            var total_samples_to_record = Convert.ToInt16(pre_samples_to_record + post_samples_to_record);

            short successful = PicoScopeLibrary.Imports.RunBlock(ScopeHandle, total_samples_to_record, Timebase, 1, out int timeIndisposedMs);
        }

        protected override bool CheckIfReady()
        {
            return (PicoScopeLibrary.Imports.Isready(ScopeHandle) == 1);
        }

        protected override short [] RetrieveOscilloscopeData()
        {
            var pre_samples_to_record = (this.TriggerDelay * 1000) / this.NanosecondsPerSample;
            var post_samples_to_record = (this.RecordingDuration * 1000) / this.NanosecondsPerSample;
            var total_samples_to_record = Convert.ToInt16(pre_samples_to_record + post_samples_to_record);

            //Grab the data returned from the oscilloscope
            short[] values = new short[total_samples_to_record];
            long num_values_returned = PicoScopeLibrary.Imports.GetValues(ScopeHandle, values, null, null, null, out short overflow, total_samples_to_record);

            return values;
        }

        #endregion
    }
}
