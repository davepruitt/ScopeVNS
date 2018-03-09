using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeVNS
{
    public class StimulationTrain
    {
        public StimulationTrain()
        {
            //empty
        }

        public DateTime StimulationTime = DateTime.MinValue;
        public List<double> Data = new List<double>();
    }
}
