using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HovyMonitor.Deskbar.Win.Updater.Extensions
{
    internal static class StringEqualsIgnoreCaseExt
    {
        public static bool EqualsIgnoreCase(this string str, string comp)
        {
            return str.Equals(comp, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
