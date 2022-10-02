using SigStat.Common.Algorithms.Distances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onlab2
{
    public class LocalDistance : IDistance<double[]>
    {
        public double Calculate(double[] p1, double[] p2)
        {
            EuclideanDistance dist = new EuclideanDistance();
            return dist.Calculate(p1, p2);

        }


        public double[] LocalDifference(double[] p1, double[] p2, double[] p3)
        {
            double[] p21 = new double[p1.Length];
            for(int i = 0; i < p1.Length; i++)
            {
                p21[i] = p2[i] - p1[i];
            }

            double[] p31 = new double[p1.Length];
            for(int i = 0; i < p1.Length; i++)
            {
                p31[i] = (p3[i] - p1[i]) / 2;
            }

            double[] diff = new double[p1.Length];

            for(int i = 0; i < p1.Length; i++)
            {
                diff[i] = (p21[i] + p31[i]) / 2;
            }

            return diff;

        }

    }
}
