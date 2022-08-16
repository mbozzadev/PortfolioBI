using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioBI_Console
{
    internal class PBI_Driver
    {

        public struct BI_Cfg
        {
            public string CONFIG;           //Dynamic XML
            public string conn;             //Database connection
            public string Location;         //Location of local files
            public bool isTest;             //If set, we are in a TEST environment
            public bool useDB;              //If set, we pull from the database
            public bool useJSON;            //If set, we use JSON to process the CSV file(s)
            public int useThreads;          //Depending on value set, depends on how we will execute the code
            public string currentDataFile;  //Used mainly for logging our current data read

            public decimal openPrice_2015;
            public decimal openPrice_2022;
        }

        public struct CSVRow
        {
            public DateTime DateOfPrice;    //Date
            public Decimal OpenPrice;       //Open
            public Decimal HighPrice;       //High
            public Decimal LowPrice;        //Low
            public Decimal ClosePrice;      //Close
            public Decimal AdjClosePrice;   //Adj Close
            public Int64 Volume;          //Volume
        }

        #region CFG Setup

        private BI_Cfg SetUpCfg()
        {
            BI_Cfg cfg = new BI_Cfg();

            string ordinal = "CONFIG";
            cfg.CONFIG = getConfig(ordinal);

            switch (cfg.CONFIG)
            {
                case "":
                    cfg.isTest = false;
                    break;

                default:
                    cfg.isTest = true;
                    break;
            }

            ordinal = "_DB" + cfg.CONFIG;
            cfg.conn = getConfig(ordinal);

            ordinal = "_Location" + cfg.CONFIG;
            cfg.Location = getConfig(ordinal);

            ordinal = "UseDB";
            string useDBString = getConfig(ordinal);
            switch (String.IsNullOrEmpty(useDBString))
            {
                case true:
                    cfg.useDB = false;      //Set this for clarity
                    break;

                case false:
                    cfg.useDB = Convert.ToBoolean(Convert.ToInt32(getConfig(ordinal)));
                    break;
            }

            ordinal = "UseJSON";
            string useJSONString = getConfig(ordinal);
            switch (String.IsNullOrEmpty(useJSONString))
            {
                case true:
                    cfg.useJSON = false;      //Set this for clarity
                    break;

                case false:
                    cfg.useJSON = Convert.ToBoolean(Convert.ToInt32(getConfig(ordinal)));
                    break;
            }

            ordinal = "UseThreads";
            string useThreadString = getConfig(ordinal);

            switch (String.IsNullOrEmpty(useThreadString))
            {
                case true:
                    cfg.useThreads = 0;      //Set this for clarity
                    break;

                case false:
                    cfg.useThreads = Convert.ToInt32(getConfig(ordinal));
                    break;
            }

            LogCfgSettings(cfg);

            return cfg;
        }


        private void LogCfgSettings(BI_Cfg cfg)
        {

            Console.WriteLine("***** Started PBI Processing *****");

            switch (cfg.CONFIG)
            {
                case "":
                    Console.WriteLine("CONFIG: PRODUCTION");
                    break;

                default:
                    Console.WriteLine("CONFIG: " + cfg.CONFIG);
                    break;
            }

            string threadingMode = "OFF";

            switch (cfg.useThreads)
            {
                case 0:
                    threadingMode = "OFF";
                    break;

                case 1:
                    threadingMode = "TASK";
                    break;

                case 2:
                    threadingMode = "PARALLEL FOR";
                    break;

                case 3:
                    threadingMode = "PARALLEL FOREACH";
                    break;
            }
            Console.WriteLine("Use Thread: " + threadingMode);

            Console.WriteLine("Use Database: " + cfg.useDB);

            switch (cfg.useDB)
            {
                case true:
                    Console.WriteLine("Database: " + cfg.conn);
                    break;

                case false:
                    Console.WriteLine("Location: " + cfg.Location);
                    break;
            }

            Console.WriteLine(" ");
        }

        public string getConfig(string appKey)
        {
            string appKeyValue = null;
            int idx = 0;
            bool found = false;

            for (idx = 0; (idx < 3 && found == false); idx++)
            {
                switch (idx)
                {
                    case 0:

                        switch (ConfigurationManager.ConnectionStrings[appKey] == null)
                        {
                            case false:
                                appKeyValue = ConfigurationManager.ConnectionStrings[appKey].ConnectionString;
                                found = true;
                                break;
                        }

                        break;

                    case 1:

                        switch (ConfigurationManager.AppSettings[appKey] == null)
                        {
                            case false:
                                appKeyValue = ConfigurationManager.AppSettings[appKey].ToString();
                                found = true;
                                break;
                        }

                        break;

                    default:
                        string error = "[" + appKey + "] not defined in App.config";
                        Console.WriteLine(error);
                        throw new Exception(error);
                }
            }

            return (appKeyValue);
        }

        #endregion CFG Setup


        internal void BeginPBI()
        {
            BI_Cfg cfg = SetUpCfg();

            switch (cfg.useDB)
            {
                case true:
                    BeginPBIProcessingFromDatabase(cfg);
                    break;

                case false:
                    BeginPBIProcessingFromLocalFile(cfg);
                    break;
            }
        }

        internal void BeginPBIProcessingFromLocalFile(BI_Cfg cfg)
        {
            Console.WriteLine("Processing Local File");

            List<string> dataFiles = new List<string>();

            string corningDataFile = "Corning_GLW_2015Data.csv";
            dataFiles.Add(corningDataFile);

            string nvidia2015DataFile = "Nvidia_NVDA_2015Data.csv";
            dataFiles.Add(nvidia2015DataFile);

            string nvidia2015_2018DataFile = "Nvidia_NVDA_2022Data.csv";
            dataFiles.Add(nvidia2015_2018DataFile);

            switch (cfg.useThreads)
            {
                case 0:
                    ProcessFiles(cfg, dataFiles);
                    break;

                case 1:

                    break;

                case 2:

                    break;

                case 3:

                    break;
            }

        }

        private void ProcessFiles(BI_Cfg cfg, List<string> dataFiles)
        {
            //Data pulled from Yahoo Finance
            for (int fileIdx = 0; fileIdx < dataFiles.Count; fileIdx++)
            {
                string dataFile = dataFiles[fileIdx];

                string fullDataFile = Path.Combine(cfg.Location, dataFile);
                //FileInfo dataFileInfo = new FileInfo(fullDataFile);

                bool runROILogic = false; 

                switch (dataFile)
                {
                    case "Nvidia_NVDA_2015Data.csv":
                    case "Nvidia_NVDA_2022Data.csv":
                        runROILogic = true;
                        break;
                }

                switch (cfg.useJSON)
                {
                    case true:
                        ProcessJSONToDataTable(ref cfg, fullDataFile, runROILogic);
                        break;

                    case false:

                        DataTable csvDT = GetDataTableFromCsv(fullDataFile, true);
                        break;
                }


            }
        }

        private CSVRow GetCSVInfo(DataRow row)
        {
            CSVRow csvRow = new CSVRow();

            string ordinal = null;

            ordinal = "Date";
            switch (row.IsNull(ordinal))
            {
                case false:
                    csvRow.DateOfPrice = Convert.ToDateTime(row[ordinal]);
                    break;
            }

            ordinal = "Open";
            switch (row.IsNull(ordinal))
            {
                case false:
                    csvRow.OpenPrice = Convert.ToDecimal(row[ordinal]);
                    break;
            }

            ordinal = "High";
            switch (row.IsNull(ordinal))
            {
                case false:
                    csvRow.HighPrice = Convert.ToDecimal(row[ordinal]);
                    break;
            }

            ordinal = "Low";
            switch (row.IsNull(ordinal))
            {
                case false:
                    csvRow.LowPrice = Convert.ToDecimal(row[ordinal]);
                    break;
            }

            ordinal = "Close";
            switch (row.IsNull(ordinal))
            {
                case false:
                    csvRow.ClosePrice = Convert.ToDecimal(row[ordinal]);
                    break;
            }

            ordinal = "Adj Close";
            switch (row.IsNull(ordinal))
            {
                case false:
                    csvRow.AdjClosePrice = Convert.ToDecimal(row[ordinal]);
                    break;
            }

            ordinal = "Volume";
            switch (row.IsNull(ordinal))
            {
                case false:
                    csvRow.Volume = Convert.ToInt64(row[ordinal]);
                    break;
            }

            return csvRow;
        }

        #region JSON Processing

        private void ProcessJSONToDataTable(ref BI_Cfg cfg, string dataFile, bool runROI = false)
        {
            string csvJSON = ConvertCsvFileToJsonObject(dataFile);
            DataTable JSONDT = DeserializeJSONToDataTable(csvJSON);

            //Now that our data is in a datatable, we can find the min, max, and average closing price

            var minClose = JSONDT.Compute("MIN(Close)", "");
            var maxClose = JSONDT.Compute("MAX(Close)", "");
            var avgClose = ComputeAvg(JSONDT,"Close");

            //var avgClose2 = (double)JSONDT.Compute("AVG([Close])", "");
            //var avgClose3 = JSONDT.Compute("AVG([Close])", "Close is not null");

            Console.WriteLine("Min Closing Price: " + minClose.ToString());
            Console.WriteLine("Max Closing Price: " + maxClose.ToString());
            Console.WriteLine("Avg Closing Price: " + avgClose.ToString());


            switch (runROI)
            {
                //This is hacky, but is strictly for the sake of the case study
                case true:
                    //We want to calculate the ROI for 1000 shares
                    //We will use the Open value from 01/02/15 and 01/03/22, which gives us a 7 year gap to really show a true ROI over the course of time

                    string selection = "Date = '1/2/2015'";

                    DataRow[] ROI_2015Row = JSONDT.Select(selection);

                    switch (ROI_2015Row.Length > 0)
                    {
                        case true:
                            //We have a 2015 date, so let's assign to our cfg for now
                            CSVRow row_2015 = GetCSVInfo(ROI_2015Row[0]);
                            cfg.openPrice_2015 = row_2015.OpenPrice;
                            break;

                        case false:
                            //We should be in the 2022 Data file
                            break;
                    }
                    selection = "Date = '1/3/2022'";
                    DataRow[] ROI_2022Row = JSONDT.Select(selection);

                    switch (ROI_2022Row.Length > 0)
                    {
                        case true:
                            //We have a 2022 date, so let's assign to our cfg for now
                            CSVRow row_2022 = GetCSVInfo(ROI_2022Row[0]);
                            cfg.openPrice_2022 = row_2022.OpenPrice;
                            break;

                        case false:
                            //We should be in the 2015 Data file
                            break;
                    }

                    switch (cfg.openPrice_2015 == 0)
                    {
                        case false:

                            switch (cfg.openPrice_2022 == 0)
                            {
                                case false:
                                    //We have data from both 2015 and 2022, so we can compare and run our ROI logic now

                                    //ROI = (Net Income / Initial Cost of Investment) * 100

                                    //This gets us the prices paid at opening for 1000 shares in 2015
                                    decimal priceOf1000Shares_2015 = cfg.openPrice_2015 * 1000;

                                    //This gets us the prices paid at opening for 1000 shares in 2022
                                    decimal priceOf1000Shares_2022 = cfg.openPrice_2022 * 1000;

                                    decimal netIncome = priceOf1000Shares_2022 - priceOf1000Shares_2015;

                                    decimal ROI = (netIncome / priceOf1000Shares_2015) * 100;

                                    string ROIPercentage = Math.Round(ROI,2).ToString();

                                    Console.WriteLine("ROI Percentage: " + ROIPercentage + "%");
                                    break;
                            }

                            break;
                    }

                    break;
            }


        }

        private decimal ComputeAvg(DataTable jsonDT, string colName)
        {
            var sumresult = jsonDT.AsEnumerable().Sum(x => Convert.ToDecimal(x[colName]));
            int dataRowCount = jsonDT.Rows.Count;

            decimal avgClose = Decimal.Divide(sumresult, dataRowCount);

            return avgClose;
        }

        private string ConvertCsvFileToJsonObject(string path)
        {
            List<string[]> csv = new List<string[]>();
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                csv.Add(line.Split(','));
            }

            string[] properties = lines[0].Split(',');

            List<Dictionary<string,string>> listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                {
                    objResult.Add(properties[j].Trim(), csv[i][j]);
                }                    

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult);
        }

        private DataTable DeserializeJSONToDataTable(string json)
        {
            DataTable jsonDT = new DataTable();
            jsonDT = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));

            return jsonDT;
        }

        #endregion JSON Processing

        #region CSV Processing
        private void ProcessCSV(BI_Cfg cfg, string dataFile)
        {

        }

        private DataTable GetDataTableFromCsv(string path, bool isFirstRowHeader)
        {
            string header = isFirstRowHeader ? "Yes" : "No";

            string pathOnly = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            string sql = @"SELECT * FROM [" + fileName + "]";

            using (OleDbConnection connection = new OleDbConnection(
                      @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly +
                      ";Extended Properties=\"Text;HDR=" + header + "\""))
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
            {
                DataTable dataTable = new DataTable();
                dataTable.Locale = CultureInfo.CurrentCulture;
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        #endregion CSV Processing

        //This process would have a front end loader to read the data into a table/queue, which could then be pulled from this project
        internal void BeginPBIProcessingFromDatabase(BI_Cfg cfg)
        {

            //Create 2 tables: DataFileNames / DataFileContents
            //DataFileNames : CorningData / NvidiaData / CreationDate
            //DataFileContents : All data from each file, connected by a pointer/file name / CreationDate
            //   - Pointer would be better in case the same file name is submitted more than once
        }

    }
}
