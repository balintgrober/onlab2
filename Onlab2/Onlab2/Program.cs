using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SigStat.Common;
using SigStat.Common.Algorithms.Distances;
using SigStat.Common.Framework.Samplers;
using SigStat.Common.Loaders;
using SigStat.Common.Logging;
using SigStat.Common.Model;
using SigStat.Common.Pipeline;
using SigStat.Common.PipelineItems.Classifiers;
using SigStat.Common.PipelineItems.Transforms.Preprocessing;
using SigStat.Common.Transforms;

namespace Onlab2
{
    class Program
    {
        static void Main(string[] args)
        {
            Svc2004Loader svc2004Loader = new Svc2004Loader(@"C:\BME\MSc\2.felev\onlab\SVC2004.zip", true);
            List<Signer> signers = svc2004Loader.EnumerateSigners().ToList();

            Signer signer1 = signers[0];
            List<Signature> signer1Signatures = signer1.Signatures;

            LocalDistance distance = new LocalDistance();

            var benchmark = new VerifierBenchmark()
            {
                Loader = svc2004Loader,
                Verifier = new Verifier(new SimpleConsoleLogger())
                {
                    Pipeline = new SequentialTransformPipeline()
                    {
                        new ZNormalization() { InputFeature = Features.X, OutputFeature = Features.X},
                        new ZNormalization() { InputFeature = Features.Y, OutputFeature = Features.Y},
                        new ZNormalization() { InputFeature = Features.Pressure, OutputFeature = Features.Pressure}
                    },
                    Classifier = new Classifier(distance.Calculate)
                    {
                        Features = new List<FeatureDescriptor>() { Features.X, Features.Y }
                    }
                },
                Sampler = new FirstNSampler()
            };

            var benchmarkFramework = new VerifierBenchmark()
            {
                Loader = svc2004Loader,
                Verifier = new Verifier(new SimpleConsoleLogger())
                {
                    Pipeline = new SequentialTransformPipeline()
                    {
                        new ZNormalization() { InputFeature = Features.X, OutputFeature = Features.X},
                        new ZNormalization() { InputFeature = Features.Y, OutputFeature = Features.Y},
                        new ZNormalization() { InputFeature = Features.Pressure, OutputFeature = Features.Pressure}
                    },
                    Classifier = new DtwClassifier()
                    {
                        Features = new List<FeatureDescriptor>() { Features.X, Features.Y }
                    }
                },
                Sampler = new FirstNSampler()
            };

            var result = benchmark.Execute(true);
            Console.WriteLine($"AER: {result.FinalResult.Aer}");
            Console.WriteLine($"FRR: {result.FinalResult.Frr}");
            Console.WriteLine($"FAR: {result.FinalResult.Far}");

            var resultFramework = benchmarkFramework.Execute(true);
            Console.WriteLine($"AER (framework): {resultFramework.FinalResult.Aer}");
            Console.WriteLine($"FRR (framework): {resultFramework.FinalResult.Frr}");
            Console.WriteLine($"FAR (framework): {resultFramework.FinalResult.Far}");

            /*var verifier = new Verifier(new SimpleConsoleLogger())
            {
                Pipeline = new SequentialTransformPipeline()
                {
                    new ParallelTransformPipeline()
                    {
                        new ZNormalization() { InputFeature = Features.X },
                        new ZNormalization() { InputFeature = Features.Y }
                    },
                },
                Classifier = new Classifier(distance.Calculate)
                {
                    Features = new List<FeatureDescriptor>() { Features.X, Features.Y } 
                }
            };

            verifier.Train(signer1Signatures);
            Console.WriteLine(verifier.Test(signer1Signatures[1]));
            Console.WriteLine(verifier.Test(signer1Signatures[33]));*/


            /*Signature signature1 = signer1Signatures[0];
            Signature signature2 = signer1Signatures[1];

            List<FeatureDescriptor> featureDescriptors = new List<FeatureDescriptor>();
            featureDescriptors.Add(Features.X);
            featureDescriptors.Add(Features.Y);

            List<double[]> sequence1 = signature1.GetAggregateFeature(featureDescriptors);
            List<double[]> sequence2 = signature2.GetAggregateFeature(featureDescriptors);


            TestDTW(sequence1, sequence2);
            TestClassifier(signer1);*/
            

            
        }

        private static void TestDTW(List<double[]> sequence1, List<double[]> sequence2)
        {
            LocalDistance distance = new LocalDistance();

            double simple_result = SimpleDTW.CalculateDTW(sequence1, sequence2, distance.Calculate);
            double improved_result = ImprovedDTW.CalculateDTW(sequence1, sequence2, distance.Calculate, distance.LocalDifference);
            

            Console.WriteLine(simple_result);
            Console.WriteLine(improved_result);
        }


        private static void TestClassifier(Signer signer)
        {
            LocalDistance distance = new LocalDistance();
            Classifier classifier = new Classifier(distance.Calculate);
            classifier.Features.Add(Features.X);
            classifier.Features.Add(Features.Y);
            EuclideanDistance euclideanDistance = new EuclideanDistance();
            DtwClassifier dtwClassifier = new DtwClassifier(euclideanDistance.Calculate);
            dtwClassifier.Features.Add(Features.X);
            dtwClassifier.Features.Add(Features.Y);

            ISignerModel model = classifier.Train(signer.Signatures);
            ISignerModel model2 = dtwClassifier.Train(signer.Signatures);

            double testResult1 = classifier.Test(model, signer.Signatures[0]);
            double testResult2 = classifier.Test(model, signer.Signatures[39]);
            double dtwClassifierTest1 = dtwClassifier.Test(model2, signer.Signatures[0]);
            double dtwClassifierTest2 = dtwClassifier.Test(model2, signer.Signatures[39]);


            Console.WriteLine(testResult1);
            Console.WriteLine(testResult2);

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine(dtwClassifierTest1);
            Console.WriteLine(dtwClassifierTest2);

        }

    }
}
