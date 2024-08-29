using System;
using System.Configuration;

namespace InvestCloud
{
    
    public class Program
    {       
        public static void Main(string[] args)
        {
            InvestCloud investCloudChallenge = new InvestCloud(int.Parse(ConfigurationManager.AppSettings["size"])); //size of the array, set in App.config
            investCloudChallenge.MultiplyMatrix().Wait();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }       
       
    }
}
