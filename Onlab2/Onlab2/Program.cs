using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SigStat.Common;
using SigStat.Common.Loaders;

namespace Onlab2
{
    class Program
    {
        static void Main(string[] args)
        {
            Svc2004Loader svc2004Loader = new Svc2004Loader(@"C:\BME\MSc\2.felev\onlab\SVC2004.zip", true);
            IEnumerable<Signer> signers = svc2004Loader.EnumerateSigners();

            List<Signer> signersList = signers.ToList();

            Signer signer1 = signersList[0];
            List<Signature> signer1Signatures = signer1.Signatures;
            Signature signature1 = signer1Signatures[0];
            Signature signature2 = signer1Signatures[1];

            List<FeatureDescriptor> featureDescriptors = new List<FeatureDescriptor>();
            featureDescriptors.Add(Features.X);
            featureDescriptors.Add(Features.Y);

            List<double[]> sequence1 = signature1.GetAggregateFeature(featureDescriptors);
            List<double[]> sequence2 = signature2.GetAggregateFeature(featureDescriptors);


            Console.WriteLine(signature1.GetFeature(Svc2004.Altitude)[4]);
            Console.WriteLine();
            Console.WriteLine();

            foreach (var item in sequence1[0])
            {
                Console.WriteLine(item);
            }

            

            
        }
    }
}
