using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onlab2
{
    public static class ImprovedDTW
    {

        public static double CalculateDTW<P>(IEnumerable<P> sequence1, IEnumerable<P> sequence2)
        {
            var s1 = (new P[] { default(P) }).Concat(sequence1).ToArray();
            var s2 = (new P[] { default(P) }).Concat(sequence2).ToArray();
        }

        private static double Distance(double[] x, double[] y)
        {

        }

        private static double[] LocalDifference(double[] r)
        {
            double[] result = new double[r.Length + 1];
            result[0] = 0;
            for(int i = 1; i < r.Length; i++)
            {
                result[i] = ((r[i] - r[i - 1]) + ((r[i + 1] - r[i - 1]) / 2)) / 2;
            }

            return result;
        }



    }
}
