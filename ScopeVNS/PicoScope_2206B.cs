using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeVNS
{
    public class PicoScope_2206B : PicoScope
    {
        #region Constructor

        public PicoScope_2206B(short scope_handle) : base(scope_handle)
        {
            //empty
        }

        #endregion

        #region Properties

        public override string ImagePath
        {
            get
            {
                return "./Images/2206B.png";
            }
        }

        public override PicoScopeType ScopeType
        {
            get
            {
                return PicoScopeType.PicoScope_2206B;
            }
        }

        public override int ScopeMaxValue
        {
            get
            {
                return PS2000ACSConsole.Imports.MaxValue;
            }
        }

        #endregion

        #region Methods

        public override string FetchSerialCode()
        {
            StringBuilder returned_info = new StringBuilder(80);
            short requiredSize = 80;

            PS2000ACSConsole.Imports.GetUnitInfo(ScopeHandle, returned_info, 80, out requiredSize, 4);

            SerialCode = returned_info.ToString().Trim();
            return SerialCode;
        }

        protected override short GetTimebase(Int64 nanoseconds_per_sample)
        {
            if (nanoseconds_per_sample == 2)
            {
                return 0;
            }
            else if (nanoseconds_per_sample == 4)
            {
                return 1;
            }
            else if (nanoseconds_per_sample == 8)
            {
                return 2;
            }
            else
            {
                double t = nanoseconds_per_sample / (1000d * 1000d * 1000d);
                t *= 62_500_000d;
                t += 2d;
                return Convert.ToInt16(t);
            }
        }

        protected override List<Int64> GetAllPossibleNanosecondsPerSample()
        {
            List<Int64> v = new List<Int64>();
            for (int i = 0; i < 32767; i++)
            {
                double n = 0;
                if (i < 3)
                {
                    n = Math.Pow(2, i) / 500_000_000d;
                }
                else
                {
                    n = Convert.ToDouble(i - 2) / 62_500_000d;
                }

                n = n * 1000d * 1000d * 1000d;

                v.Add(Convert.ToInt64(n));
            }

            return v;
        }

        protected override Int64 GetNearestNanosecondsPerSample(Int64 ns)
        {
            var v = this.GetAllPossibleNanosecondsPerSample();
            var nearest = v.OrderBy(x => Math.Abs(x - ns)).First();
            return nearest;
        }

        public override void ShutdownOscilloscope()
        {
            //Close down the pico scope
            PS2000ACSConsole.Imports.Stop(ScopeHandle);
            PS2000ACSConsole.Imports.CloseUnit(ScopeHandle);
            
            //Shutdown the background thread
            StopBackgroundWorker();
        }

        protected override void SetOscilloscopeChannel()
        {
            short success = PS2000ACSConsole.Imports.SetChannel(ScopeHandle, PS2000ACSConsole.Imports.Channel.ChannelA,
                1, 1, PS2000ACSConsole.Imports.Range.Range_20V, 0);
        }

        protected override void SetupTrigger()
        {
            var pre_samples_to_record = (this.TriggerDelay * 1000) / this.NanosecondsPerSample;
            var post_samples_to_record = (this.RecordingDuration * 1000) / this.NanosecondsPerSample;
            var total_samples_to_record = pre_samples_to_record + post_samples_to_record;
            
            //Calculate the trigger threshold.  It is supposed to be a value in the range of [0, Int16.MaxValue], where Int16.MaxValue
            //is equal to the maximum voltage we have set the scope to (which should be 20 V).
            short threshold = Convert.ToInt16(Math.Round(Convert.ToDouble(PicoScopeLibrary.Imports.MaxValue) * (TriggerVoltage / MaxVoltage)));

            //Set the trigger direction
            PS2000ACSConsole.Imports.ThresholdDirection d = PS2000ACSConsole.Imports.ThresholdDirection.Above;
            switch (this.ScopeTriggerType)
            {
                case TriggerType.RisingEdge:
                    d = PS2000ACSConsole.Imports.ThresholdDirection.Rising;
                    break;
                case TriggerType.FallingEdge:
                    d = PS2000ACSConsole.Imports.ThresholdDirection.Falling;
                    break;
                case TriggerType.Above:
                    d = PS2000ACSConsole.Imports.ThresholdDirection.Above;
                    break;
                case TriggerType.Below:
                    d = PS2000ACSConsole.Imports.ThresholdDirection.Below;
                    break;
            }

            //Set the trigger
            short success = PS2000ACSConsole.Imports.SetSimpleTrigger(ScopeHandle, 1, PS2000ACSConsole.Imports.Channel.ChannelA,
                threshold, d, 0, 0);
        }

        protected override void RunBlock()
        {
            var pre_samples_to_record = Convert.ToInt32((this.TriggerDelay * 1000) / this.NanosecondsPerSample);
            var post_samples_to_record = Convert.ToInt32((this.RecordingDuration * 1000) / this.NanosecondsPerSample);
            var total_samples_to_record = Convert.ToInt32(pre_samples_to_record + post_samples_to_record);

            IntPtr null_pointer = IntPtr.Zero;
            short successful = PS2000ACSConsole.Imports.RunBlock(ScopeHandle, pre_samples_to_record, post_samples_to_record, Convert.ToUInt32(Timebase),
                0, out int timeIndisposedMs, 0, null, null_pointer);
        }

        protected override bool CheckIfReady()
        {
            PS2000ACSConsole.Imports.IsReady(ScopeHandle, out Int16 ready);
            return (ready != 0);
        }

        protected override short[] RetrieveOscilloscopeData()
        {
            var pre_samples_to_record = Convert.ToInt32((this.TriggerDelay * 1000) / this.NanosecondsPerSample);
            var post_samples_to_record = Convert.ToInt32((this.RecordingDuration * 1000) / this.NanosecondsPerSample);
            var total_samples_to_record = Convert.ToInt32(pre_samples_to_record + post_samples_to_record);

            //Grab the data returned from the oscilloscope
            short[] data_buffer = new short[total_samples_to_record];

            PS2000ACSConsole.Imports.SetDataBuffer(ScopeHandle, PS2000ACSConsole.Imports.Channel.ChannelA, data_buffer,
                total_samples_to_record, 0, PS2000ACSConsole.Imports.RatioMode.None);

            Int16 overflow = 0;
            uint total_samples_uint = Convert.ToUInt32(total_samples_to_record);
            PS2000ACSConsole.Imports.GetValues(ScopeHandle, 0, ref total_samples_uint, 0, PS2000ACSConsole.Imports.DownSamplingMode.None, 0, out overflow);
            
            return data_buffer;
        }

        #endregion
    }
}
