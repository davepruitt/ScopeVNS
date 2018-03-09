using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeVNS
{
    public enum PicoScopeType
    {
        [Description("PicoScope 2204A")]
        PicoScope_2204A,

        [Description("PicoScope 2206B")]
        PicoScope_2206B,

        [Description("Unknown")]
        Unknown
    }
}
