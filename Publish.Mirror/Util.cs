using System.Collections.Generic;
using System.Linq;

namespace Tridion.Events
{
    static class Util
    {
        /// <summary>
        /// Print a list of strings into a comma separated list
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        public static string PrintList(this IEnumerable<string> strings)
        {
            if(strings == null || strings.Count() == 0)
            {
                return null;
            }

            return string.Join(", ", strings);
        }

    }
}
