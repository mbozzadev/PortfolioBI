using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioBI_Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting PortfolioBI App");
            try
            {
                PBI_Driver pbi = new PBI_Driver();
                pbi.BeginPBIProcessing();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Could not process PortfolioBI properly");
                Console.WriteLine("ERROR: " + ex.ToString());
            }


            Console.WriteLine("Closing PortfolioBI App");
        }
    }
}
