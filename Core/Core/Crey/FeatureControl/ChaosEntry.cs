using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Text.RegularExpressions;

namespace Crey.FeatureControl
{
    [Obsolete]
    public class ChaosEntry : TableEntity
    {
        public static int MAX_PROPABILITY = 10000;

        public int Order { get; set; }

        public string RouteMatcher { get; set; }

        public int Propability { get; set; }

        public bool IsChaos(string route, Random rnd, IServiceProvider services)
        {
            if (Propability <= 0)
                return false;

            var pattern = RouteMatcher;
            if (Regex.IsMatch(route, pattern))
            {
                var p = rnd.Next(0, MAX_PROPABILITY);
                return Propability > p;
            }

            return false;
        }
    }
}
