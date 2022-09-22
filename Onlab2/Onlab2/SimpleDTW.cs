using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onlab2
{
    public static class SimpleDTW
    {

        public static double CalculateDTW<P>(IEnumerable<P> sequence1, IEnumerable<P> sequence2, Func<P, P, double> distance)
        {
            var s1 = (new P[] { default(P) }).Concat(sequence1).ToArray();
            var s2 = (new P[] { default(P) }).Concat(sequence2).ToArray();

            var n = s1.Length - 1;
            var m = s2.Length - 1;

            var dtw = new double[n + 1, m + 1];
            for (int i = 0; i <= n; i++)
            {
                for (int j = 0; j <= m; j++)
                {
                    dtw[i, j] = Double.PositiveInfinity;
                }
            }
            dtw[0, 0] = 0;

            for (int i = 0; i <= n; i++)
            {
                for (int j = 0; j <= m; j++)
                {
                    var cost = distance(s1[i], s2[j]);
                    dtw[i, j] = cost + Min(dtw[i - 1, j], dtw[i, j - 1], dtw[i - 1, j - 1]);
                }
            }

            return dtw[n, m];

        }


        private static double Min(double d1, double d2, double d3)
        {
            double d12 = d1 > d2 ? d2 : d1;
            return d12 > d3 ? d3 : d12;
        }

    }
}
