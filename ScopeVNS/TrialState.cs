using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeVNS
{
    public enum TrialState
    {
        Idle,
        SetupForNextTrigger,
        WaitForTrigger,
        TriggeredAndCollecting,
        FinalizingCollectionAfterTrigger,
        WaitingOnRefractoryPeriod
    }
}
