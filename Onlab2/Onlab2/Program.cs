using System;
using System.Collections;
using System.Collections.Generic;
using SigStat.Common;
using SigStat.Common.Loaders;

namespace Onlab2
{
    class Program
    {
        static void Main(string[] args)
        {
            Svc2004Loader svc2004Loader = new Svc2004Loader(@"C:\BME\MSc\2.felev\onlab\SVC2004.zip", false);
            IEnumerable<Signer> signers = svc2004Loader.EnumerateSigners();

            foreach (var item in signers)
            {
                Console.WriteLine(item);
            }


            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
