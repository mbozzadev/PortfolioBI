using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioBI_Console
{
    internal class PBI_Driver
    {

        //Create 2 tables: DataFileNames / DataFileContents
        //DataFileNames : CorningData / NvidiaData / CreationDate
        //DataFileContents : All data from each file, connected by a pointer/file name / CreationDate
        //   - Pointer would be better in case the same file name is submitted more than once

        internal void BeginPBIProcessingFromLocalFile()
        {
            Console.WriteLine("Processing PBI");

            string dataStagingPath = "C:\\Users\\mikeb\\Desktop\\Dev Projects\\Docs\\";
            string corningDataFile = "Corning_CLW_2015Data";
            string nvidiaDataFile = "Nvidia_NVDA_2015Data";



        }

        //This process would have a front end loader to read the data into a table/queue, which could then be pulled from this project
        internal void BeginPBIProcessingFromDatabase()
        {

        }

    }
}
