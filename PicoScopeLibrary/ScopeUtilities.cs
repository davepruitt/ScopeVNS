using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoScopeLibrary
{
    /// <summary>
    /// Useful utility functions when working with the pico scopes
    /// </summary>
    public static class ScopeUtilities
    {
        /// <summary>
        /// This function returns the scope's serial code
        /// </summary>
        /// <param name="scopeHandle">The handle of the scope</param>
        /// <returns>The serial code of the scope</returns>
        public static string GetScopeID(short scopeHandle)
        {
            StringBuilder returned_info = new StringBuilder(80);
            Imports.GetUnitInfo(scopeHandle, returned_info, 80, 4);
            return returned_info.ToString();
        }
    }
}
