using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeVNS
{
    /// <summary>
    /// A class that manages saving to general booth logs
    /// </summary>
    public class BoothSaveManager
    {
        #region Singleton

        private static BoothSaveManager _instance = null;
        private static Object syncRoot = new Object();

        /// <summary>
        /// Singleton constructor
        /// </summary>
        private BoothSaveManager()
        {
            //empty
        }

        /// <summary>
        /// Returns the singleton instance of the booth save manager
        /// </summary>
        /// <returns>BoothSaveManager object</returns>
        public static BoothSaveManager GetInstance()
        {
            if (_instance == null)
            {
                lock (syncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new BoothSaveManager();
                    }
                }
            }

            return _instance;
        }

        #endregion

        #region Properties

        Dictionary<int, List<StreamWriter>> BoothStreamAssociations = new Dictionary<int, List<StreamWriter>>();
        Dictionary<int, DateTime> LastSaveForEachBooth = new Dictionary<int, DateTime>();
        
        #endregion

        #region Methods

        /// <summary>
        /// Gets the current StreamWriter object for the designated booth.  If one doesn't exist, it creates one.
        /// If the writer is too old, it creates a new one.
        /// </summary>
        /// <param name="booth_number">The booth number</param>
        /// <returns>The current stream writer object</returns>
        private List<StreamWriter> GetStreamWriter (int booth_number)
        {
            List<StreamWriter> writer = null;
            var success = BoothStreamAssociations.TryGetValue(booth_number, out writer);
            if (!success)
            {
                //If no stream writer objects exist for this booth
                //Create them
                writer = CreateStreamWriter(booth_number);
                BoothStreamAssociations[booth_number] = writer;
            }
            else
            {
                //If stream writer objects do exist for this booth
                //Make sure the stream writer is writing to a file with the correct date

                DateTime last_save = DateTime.MinValue;
                success = LastSaveForEachBooth.TryGetValue(booth_number, out last_save);
                if (success)
                {
                    //If it was successful, check the date to see if it matches the current date
                    if (last_save.Date == DateTime.Today)
                    {
                        //do nothing in this case.
                        //the "writer" variable should already have current StreamWriter objects.
                    }
                    else
                    {
                        //Close all the previous StreamWriter objects
                        foreach (var w in writer)
                        {
                            if (w != null)
                            {
                                w.Close();
                            }
                        }

                        //Otherwise, create new stream writers for the new day
                        try
                        {
                            writer = CreateStreamWriter(booth_number);
                        }
                        catch
                        {
                            try
                            {
                                StreamWriter d = new StreamWriter("debug.txt", true);
                                d.WriteLine("Unable to create new file stream for booth" + booth_number.ToString() + " at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                d.Close();
                            }
                            catch
                            {
                                //empty
                            }
                        }

                        BoothStreamAssociations[booth_number] = writer;
                    }
                }
            }

            return writer;
        }

        public void CloseAllStreams()
        {
            foreach (var g in BoothStreamAssociations)
            {
                var v = g.Value;
                if (v != null)
                {
                    foreach (var w in v)
                    {
                        if (w != null)
                        {
                            w.Close();
                        }
                    }
                }
            }
        }

        public List<StreamWriter> CreateStreamWriter (int booth_number)
        {
            //Set the "last save" timestamp for this booth number, so that we don't accidentally try to create
            //a stream for the same file again
            LastSaveForEachBooth[booth_number] = DateTime.Now;

            //Get the parts of the file name
            var scopeManager = ScopeManager.GetInstance();
            string primary_base_path = scopeManager.PrimaryDataPath;
            string secondary_base_path = scopeManager.SecondaryDataPath;
            string group_id = scopeManager.GroupID;

            DateTime now = DateTime.Now;
            string date_today = now.ToString("_yyyy_MM_dd");

            string booth_file_name = "Booth" + booth_number.ToString() + date_today;

            //Make the full file path for each save location
            string full_primary_path = primary_base_path;
            string full_secondary_path = secondary_base_path;
            if (!string.IsNullOrEmpty(group_id))
            {
                full_primary_path += group_id + @"\";
                full_secondary_path += group_id + @"\";
            }

            full_primary_path += booth_number.ToString() + @"\" + booth_file_name;
            full_secondary_path += booth_number.ToString() + @"\" + booth_file_name;

            List<StreamWriter> result = new List<StreamWriter>();
            if (!string.IsNullOrEmpty(primary_base_path))
            {
                //Create directory if it doesn't exist
                new FileInfo(full_primary_path).Directory.Create();

                try
                {
                    StreamWriter p = new StreamWriter(full_primary_path, true);
                    result.Add(p);
                }
                catch
                {
                    //empty
                }
            }
            if (!string.IsNullOrEmpty(secondary_base_path))
            {
                //Create directory if it doesn't exist
                new FileInfo(full_secondary_path).Directory.Create();
                
                try
                {
                    StreamWriter s = new StreamWriter(full_secondary_path, true);
                    result.Add(s);
                }
                catch
                {
                    //empty
                }
            }

            return result;
        }

        public void SavePassiveStim(int booth_number, StimulationTrain stim, uint microseconds_per_sample)
        {
            //Save the data to a file (or multiple files)
            List<StreamWriter> writers = this.GetStreamWriter(booth_number);

            //Log the current save time AFTER we get the stream (it is important that this happens AFTER and not before, because of
            //how we check for refreshing streams based on the date of the last stimulation that occurred).
            LastSaveForEachBooth[booth_number] = DateTime.Now;

            foreach (var w in writers)
            {
                if (w != null)
                {
                    w.Write(stim.StimulationTime.ToString("yyyy-MM-dd:HH:mm:ss:ffffff"));
                    w.Write(", " + microseconds_per_sample.ToString());
                    for (int j = 0; j < stim.Data.Count; j++)
                    {
                        w.Write(", ");
                        w.Write(stim.Data[j].ToString("0.00"));
                    }
                    w.Write("\r\n");

                    //Flush the stream so that the data gets written to the file for sure.
                    //If we don't flush the stream here, then the computer will decide when to flush it, which could be hours from now
                    //And what if a power outage happened?!?!?  The data would be lost.  Flush the stream.
                    w.Flush();
                }
            }
        }

        /// <summary>
        /// This function will refresh all file streams for the designated booth.  If no file stream is open, it will
        /// open one.  If a file stream is from the previous day, it will create a new one for the current day.
        /// </summary>
        /// <param name="booth_number">The booth on which to refresh streams.</param>
        public void RefreshStreams (int booth_number)
        {
            //This will actually cause it to refresh the file streams, so if the day has changed, it will immediately
            //close out the old file for the old day and create a new file for the new day.
            this.GetStreamWriter(booth_number);
        }

        #endregion
    }
}
