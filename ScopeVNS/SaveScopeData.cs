using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeVNS
{
    /// <summary>
    /// A static class that handles saving scope data
    /// </summary>
    public static class SaveScopeData
    {
        /// <summary>
        /// Saves session data for a rat
        /// </summary>
        /// <param name="base_path">The path of where to save the data on disk</param>
        /// <param name="rat_name">The rat name</param>
        /// <param name="booth_number">The booth the rat is in</param>
        /// <param name="scope_id">The serial code of the scope hooked up to the booth</param>
        /// <param name="scope_channel">The channel of the scope that is being used for the booth</param>
        /// <param name="All_Traces">All of the VNS traces that have been recorded</param>
        /// <param name="All_Timestamps">The timestamp at which each VNS trace was captured</param>
        public static void SaveData(string base_path, string rat_name, string booth_name, string scope_id, string scope_channel, List<StimulationTrain> All_Stims, 
            uint us_per_sample)
        {
            string date = DateTime.Now.ToString("yyyyMMdd_HHmm");
            string file_name = rat_name + "_" + date + ".scope";
            string primary_full_path = base_path + rat_name + @"\" + file_name;
            
            //Create directory if it doesn't exist
            new FileInfo(primary_full_path).Directory.Create();

            StreamWriter w1 = new StreamWriter(primary_full_path);
            
            double median_max = double.NaN;
            double median_min = double.NaN;
            double median_pk_to_pk = double.NaN;

            if (All_Stims.Count > 0)
            {
                //Calculate the median max voltage
                var max_voltages = All_Stims.Select(d => d.Data.Max()).ToList();
                median_max = TxBDC_Common.TxBDC_Math.Median(max_voltages);

                //Calculate the median min voltage
                var min_voltages = All_Stims.Select(d => d.Data.Min()).ToList();
                median_min = TxBDC_Common.TxBDC_Math.Median(min_voltages);

                //Calculate the median peak-to-peak voltage
                var peak_to_peak_voltages = max_voltages.Zip(min_voltages, (one, two) => one - two).ToList();
                median_pk_to_pk = TxBDC_Common.TxBDC_Math.Median(peak_to_peak_voltages);
            }
            
            int version = 4;

            w1.WriteLine("ScopeVNS Version: " + version.ToString());
            w1.WriteLine("Rat name: " + rat_name);
            w1.WriteLine("Booth number: " + booth_name);
            w1.WriteLine("Scope serial code: " + scope_id);
            w1.WriteLine("Scope channel: " + scope_channel);
            w1.WriteLine("Scope microseconds per sample: " + us_per_sample.ToString());
            w1.WriteLine("Save date: " + DateTime.Now.ToString("yyyy-MM-dd:HH:mm:ss"));
            w1.WriteLine("Max voltage (median): " + median_max.ToString());
            w1.WriteLine("Min voltage (median): " + median_min.ToString());
            w1.WriteLine("Peak-to-peak voltage (median): " + median_pk_to_pk.ToString());
            w1.WriteLine("Number of stims detected: " + All_Stims.Count.ToString());

            for (int i = 0; i < All_Stims.Count; i++)
            {
                w1.Write(All_Stims[i].StimulationTime.ToString("yyyy-MM-dd:HH:mm:ss:ffffff"));
                for (int j = 0; j < All_Stims[i].Data.Count; j++)
                {
                    w1.Write(", ");
                    w1.Write(All_Stims[i].Data[j].ToString("0.00"));
                }
                w1.Write("\r\n");
            }

            w1.Close();
        }
    }
}
