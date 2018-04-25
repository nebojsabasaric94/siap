using System.Collections.Generic;
using System.IO;
using WeedDetection;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using BackPropagationAlgorithm;
using System.Text;

namespace Test
{
    class Program
    {
        public static string TestSetPath = @"D:\PLANTS WEEDS\Test\";
        public static int ClassCount = 10;
        public static int SamplesPerClass = 50;
        private static List<ResultElement> Results = new List<ResultElement>();
        private static string SerializedNetworkPath = @"D:\WeedBPTrained.bin";

        static void Main(string[] args)
        {
            /*Console.WriteLine("Train again? y/n");
            if (Console.ReadKey().KeyChar == 'y')
                WD.TrainNetwork();
            else
                LoadFromFile();

            List<string> folderNames = GetFolderNames(TestSetPath);
            int i = 0;
            foreach (string folderName in folderNames)
            {
                i++;
                List<string> images = GetFiles(TestSetPath + folderName);
                foreach(var imagePath in images)
                {
                    Results.Add(new ResultElement() {
                        ExpectedResult = i,
                        CalculatedResult = WD.GetResult(TestSetPath + folderName + "\\" + imagePath)
                    });
                }
            }
            //ExportToCSV(Results);
           //Results = CalculateF1FromCSV.Calc();
           List<int> groupedResults = new List<int>();
           int counterr = 0;
           int trueCounter = 0;
           foreach(ResultElement r in Results)
           {
               counterr++;

               if (r.ExpectedResult == r.CalculatedResult)
                   trueCounter++;
               if (counterr == 50)
               {
                   groupedResults.Add(trueCounter);
                   counterr = 0;
                   trueCounter = 0;
               }
           }
           */
            //F1Score();
            //PngToJpeg.Convert();
            ToMnistSize.Convert();
            Console.WriteLine("...");
            Console.ReadKey();
        }


        public static List<string> GetFolderNames(string folderPath)
        {
            List<string> retval = new List<string>();
            foreach (string s in Directory.GetDirectories(folderPath).Select(Path.GetFileName))
                retval.Add(s);
            return retval;
        }

        public static List<string> GetFiles(string folderPath)
        {
            List<string> retval = new List<string>();
            foreach (string s in Directory.GetFiles(folderPath + @"\", "*.png").Select(Path.GetFileName))
                retval.Add(s);
            return WeedDetection.StringComparer.NaturalSort(retval).ToList();
        }


        public static double F1Score()
        {
            double truePositives = 0;
            double trueNegatives = 0;
            double falsePositives = 0;
            double falseNegatives = 0;
            double f1score = 0;

            for(int i=1; i<=ClassCount; i++)
            {
                foreach(var r in Results)
                {
                    if(r.ExpectedResult == i)
                    {
                        if (r.ExpectedResult == r.CalculatedResult)
                            truePositives++;
                        if (r.ExpectedResult != r.CalculatedResult)
                            falseNegatives++;
                    }
                    if(r.ExpectedResult != i)
                    {
                        if (r.CalculatedResult == i)
                            falsePositives++;
                        if (r.CalculatedResult != i)
                            trueNegatives++;
                    }
                }
            }

            double precision = truePositives / (truePositives + falsePositives);
            double recall = truePositives / (truePositives + falseNegatives);
            f1score = 2 * precision * recall / (precision + recall);
            Console.WriteLine("\n\n\t\t\t_____________F1 Score_____________ " + f1score);
            return f1score;
        }


        private static void LoadFromFile()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(SerializedNetworkPath,
                                      FileMode.Open,
                                      FileAccess.Read,
                                      FileShare.Read);
            BackPropagation obj = (BackPropagation)formatter.Deserialize(stream);
            WD.bp = obj;
            stream.Close();
        }

        private static void ExportToCSV(List<ResultElement> results)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Expected result" + ", " + "Calculated result");
            foreach (var r in results)
            {
                sb.AppendLine(ResultToString(r.ExpectedResult) + ", " + ResultToString(r.CalculatedResult));
            }
            File.WriteAllText(@"D:\results.csv", sb.ToString());
        }

        private static string ResultToString(int s)
        {
            string retval = "";
            switch (s)
            {
                case 1: retval = "Black-grass"; break;
                case 2: retval = "Charlock"; break;
                case 3: retval = "Cleavers"; break;
                case 4: retval = "Common Chickweed"; break;
                case 5: retval = "Fat Hen"; break;
                case 6: retval = "Loose Silky-bent"; break;
                case 7: retval = "Maize"; break;
                case 8: retval = "Shepherd’s Purse"; break;
                case 9: retval = "Small-flowered Cranesbill"; break;
                case 10: retval = "Sugar beet"; break;
            }

            return retval;
        }


    }
}
