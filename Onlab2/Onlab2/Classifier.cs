using SigStat.Common;
using SigStat.Common.Loaders;
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

        public double MultiplicationFactor { get; set; } = 1;

        public double GenuineAverageTime { get; set; }

        public Classifier(Func<double[], double[], double> distanceMethod)
        {
            this.DistanceFunction = distanceMethod;
            Features = new List<FeatureDescriptor>();
        }

        public ISignerModel Train(List<Signature> signatures)
        {
            if (signatures == null || signatures.Count == 0)
                throw new ArgumentException("Argument 'signatures' can not be null or an empty list", nameof(signatures));
            var signerID = signatures[0].Signer?.ID;
            GenuninesAverageTimeCost(signatures);
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

            var distances = distanceMatrix.GetValues().Where(v => v != 0).ToArray();
            var mean = distances.Min();
            var stdev = Math.Sqrt(distances.Select(d => (d - mean) * (d - mean)).Sum() / (distances.Count() - 1));

            double med;
            var orderedDistances = new List<double>(distances).OrderBy(d => d);
            if (orderedDistances.Count() % 2 == 0)
            {
                int i = orderedDistances.Count() / 2;
                med = (orderedDistances.ElementAt(i - 1) + orderedDistances.ElementAt(i)) / 2.0;
            }
            else
            {
                int i = orderedDistances.Count() / 2;
                med = orderedDistances.ElementAt(i);
            }

            Console.WriteLine($"Threshold: {mean + MultiplicationFactor * stdev}");

            return new SignerModel
            {
                SignerID = signerID,
                GenuineSignatures = genuines.Select(g => new KeyValuePair<string, double[][]>(g.ID, g.Features)).ToList(),
                DistanceMatrix = distanceMatrix,
                Threshold = mean + MultiplicationFactor * stdev
                
            };


        }

        public double Test(ISignerModel model, Signature signature)
        {
            var dtwModel = (SignerModel)model;
            var testSignature = signature.GetAggregateFeature(Features).ToArray();
            var distances = new double[dtwModel.GenuineSignatures.Count];
            LocalDistance diff = new LocalDistance();

            for (int i = 0; i < dtwModel.GenuineSignatures.Count; i++)
            {
                distances[i] = ImprovedDTW.CalculateDTW(dtwModel.GenuineSignatures[i].Value, testSignature, DistanceFunction, diff.LocalDifference);
                dtwModel.DistanceMatrix[signature.ID, dtwModel.GenuineSignatures[i].Key] = distances[i] * TimeCost(signature);
                this.LogTrace(new ClassifierDistanceLogState(model.SignerID, signature?.Signer.ID, dtwModel.GenuineSignatures[i].Key, signature.ID, distances[i]));
            }

            
            return Math.Max(1 - (distances.Average() / dtwModel.Threshold) / 2, 0);

        }

        private void GenuninesAverageTimeCost(List<Signature> signatures)
        {
            var genuinesTime = signatures.Where(s => s.Origin == Origin.Genuine).Select(s => new { s.ID, Features = s.GetFeature(Svc2004.T).ToArray() }).ToList();
            double time = 0;
            foreach (var signature in genuinesTime)
            {
                DateTime first = DateTimeOffset.FromUnixTimeSeconds(signature.Features[0]).DateTime;
                DateTime last = DateTimeOffset.FromUnixTimeSeconds(signature.Features.Last()).DateTime;

                time += (last - first).TotalSeconds;
            }
            GenuineAverageTime = time / genuinesTime.Count;
        }

        private double TimeCost(Signature candidate)
        {
            double time = 0;
            int firstTimestamp = candidate.GetFeature(Svc2004.T)[0];
            int lastTimestamp = candidate.GetFeature(Svc2004.T).Last();
            DateTime first = DateTimeOffset.FromUnixTimeMilliseconds(firstTimestamp).DateTime;
            DateTime last = DateTimeOffset.FromUnixTimeMilliseconds(lastTimestamp).DateTime;
            time = (last - first).TotalSeconds;

            return time / GenuineAverageTime;
        }


    }
}
