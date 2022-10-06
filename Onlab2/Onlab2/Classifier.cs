using SigStat.Common;
using SigStat.Common.Logging;
using SigStat.Common.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onlab2
{

    public class SignerModel : ISignerModel
    {
        public string SignerID { get; set; }

        public List<KeyValuePair<string, double[][]>> GenuineSignatures { get; set; }

        public double Threshold;

        public DistanceMatrix<string, string, double> DistanceMatrix;

    }


    public class Classifier : PipelineBase, IDistanceClassifier
    {
        public Func<double[], double[], double> DistanceFunction { get; set; }

        public List<FeatureDescriptor> Features { get; set; }

        public double MultiolicationFactor { get; set; } = 1;

        public Classifier(Func<double[], double[], double> distanceMethod)
        {
            this.DistanceFunction = distanceMethod;
            Features = new List<FeatureDescriptor>();
        }

        public double Test(ISignerModel model, Signature signature)
        {
            throw new NotImplementedException();
        }

        public ISignerModel Train(List<Signature> signatures)
        {
            if (signatures == null || signatures.Count == 0)
                throw new ArgumentException("Argument 'signatures' can not be null or an empty list", nameof(signatures));
            var signerID = signatures[0].Signer?.ID;
            var genuines = signatures.Where(s => s.Origin == Origin.Genuine)
                .Select(s => new { s.ID, Features = s.GetAggregateFeature(Features).ToArray() }).ToList();
            var distanceMatrix = new DistanceMatrix<string, string, double>();
            foreach (var i in genuines)
            {
                foreach (var j in genuines)
                {
                    if (distanceMatrix.ContainsKey(j.ID, i.ID))
                    {
                        distanceMatrix[i.ID, j.ID] = distanceMatrix[j.ID, i.ID];
                    }
                    else if (i == j)
                    {
                        distanceMatrix[i.ID, j.ID] = 0;
                    }
                    else
                    {
                        LocalDistance diff = new LocalDistance();
                        var distance = ImprovedDTW.CalculateDTW(i.Features, j.Features, DistanceFunction, diff.LocalDifference);
                        distanceMatrix[i.ID, j.ID] = distance;
                        this.LogTrace(new ClassifierDistanceLogState(signerID, signerID, i.ID, j.ID, distance));

                    }

                }
            }




        }
    }
}
