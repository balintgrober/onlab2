using SigStat.Common;
using SigStat.Common.Algorithms.Distances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onlab2
{
    public static class ImprovedDTW
    {

        public static double CalculateDTW<P>(IEnumerable<P> sequence1, IEnumerable<P> sequence2, Func<P, P, double> distance, Func<P,P,P, double[]> difference)
        {
            var s1 = (new P[] { default(P) }).Concat(sequence1).ToArray();
            var s2 = (new P[] { default(P) }).Concat(sequence2).ToArray();

            var n = s1.Length - 1;
            var m = s2.Length - 1;

            var dtw = new double[n + 1, m + 1];
            dtw.SetValues(Double.PositiveInfinity);
            dtw[0, 0] = 0;

            EuclideanDistance dist = new EuclideanDistance();

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    var cost = 0.0;
                    if(i + 1 <= n && j + 1 <= m && i > 1 && j > 1)
                    {
                        cost = distance(s1[i], s2[j]) * dist.Calculate(difference(s1[i - 1], s1[i], s1[i + 1]), difference(s2[j - 1], s2[j], s2[j + 1]));
                    }
                    else
                    {
                        cost = distance(s1[i], s2[j]) + Min(dtw[i - 1, j], dtw[i, j - 1], dtw[i - 1, j - 1]); ;
                    }
                    
                    dtw[i, j] = cost;
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
