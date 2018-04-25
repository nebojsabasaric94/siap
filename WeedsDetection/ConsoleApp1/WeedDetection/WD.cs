using BackPropagationAlgorithm;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WeedDetection
{
    public class WD
    {
        public static string DataSetPath = @"D:\PLANTS WEEDS\Training\";
        public static int NetworkInputCount = 40;
        public static int SampleCount = 300;
        public static int ClassesCount = 10;
        public static BackPropagation bp = null;

        static void Main(string[] args)
        {
            TrainNetwork();
            Console.ReadKey();
        }
        
        public static void TrainNetwork()
        {
            List<InputOutput> ioList = new List<InputOutput>();
            List<string> folderNames = GetFolderNames(DataSetPath);
            int i = 0;
            foreach (string folderName in folderNames)
            {
                i++;
                //Directory.CreateDirectory(DataSetPath + "Processed_" + folderName);
                List<string> images = GetFiles(DataSetPath + folderName);
                int cc = 0;
                foreach (string image in images)
                {
                    if(cc++ < 30)
                    ioList.Add(new InputOutput()
                    {
                        Input = GenerateInputVector(DataSetPath + folderName + "\\" + image),
                        Output = i,
                    });
                }
            }
            ioList = Shuffle(ioList);
            double[,,] obucavajuciSkup = new double[SampleCount,2,NetworkInputCount];
            for(int j=0; j<ioList.Count; j++)
            {
                for(int k=0; k<ioList[j].Input.Count; k++)
                {
                    obucavajuciSkup[j, 0, k] = ioList[j].Input[k];
                }
                obucavajuciSkup[j, 1, 0] = 0;
                obucavajuciSkup[j, 1, 1] = 0;
                obucavajuciSkup[j, 1, 2] = 0;
                obucavajuciSkup[j, 1, 3] = 0;
                obucavajuciSkup[j, 1, 4] = 0;
                obucavajuciSkup[j, 1, 5] = 0;
                obucavajuciSkup[j, 1, 6] = 0;
                obucavajuciSkup[j, 1, 7] = 0;
                obucavajuciSkup[j, 1, 8] = 0;
                obucavajuciSkup[j, 1, 9] = 0;

                if (ioList[j].Output == 1)
                    obucavajuciSkup[j, 1, 0] = 1;
                else if (ioList[j].Output == 2)
                    obucavajuciSkup[j, 1, 1] = 1;
                else if (ioList[j].Output == 3)
                    obucavajuciSkup[j, 1, 2] = 1;
                else if (ioList[j].Output == 4)
                    obucavajuciSkup[j, 1, 3] = 1;
                else if (ioList[j].Output == 5)
                    obucavajuciSkup[j, 1, 4] = 1;
                else if (ioList[j].Output == 6)
                    obucavajuciSkup[j, 1, 5] = 1;
                else if (ioList[j].Output == 7)
                    obucavajuciSkup[j, 1, 6] = 1;
                else if (ioList[j].Output == 8)
                    obucavajuciSkup[j, 1, 7] = 1;
                else if (ioList[j].Output == 9)
                    obucavajuciSkup[j, 1, 8] = 1;
                else if (ioList[j].Output == 10)
                    obucavajuciSkup[j, 1, 9] = 1;
            }
            bp = new BackPropagation(SampleCount, obucavajuciSkup);
            bp.obuci();
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(@"D:\WeedBPTrained.bin",
                                     FileMode.Create,
                                     FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, bp);
            stream.Close();
            Console.WriteLine("BP Network trained.");
            //NetworkTest(@"D:\PLANTS WEEDS\Processed\Processed_Black-grass\44.png");
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
            return StringComparer.NaturalSort(retval).ToList();
        }

        public static List<double> GenerateInputVector(string imagePath)
        {
            List<double> retVal = new List<double>();
            Image<Gray, byte> img = new Image<Gray, byte>(imagePath);
            if (img.Width > NetworkInputCount)
            {
                int step = img.Width / NetworkInputCount;
                int val = 0;
                for (int i = 0; i < img.Width-step; i += step)
                {
                    val = 0;
                    for (int j = 0; j < img.Height; j++)
                    {
                        if (img.Data[j, i, 0] >0)
                            val++;
                    }
                    retVal.Add(((double)(val))/img.Width);
                }
                #region fixRetValCount
                int diff = retVal.Count - NetworkInputCount;
                if (diff > 0)
                {
                    Random rnd = new Random();
                    for(int i=0; i<diff; i++)
                        retVal.RemoveAt(rnd.Next(NetworkInputCount-1));
                }
                else if( diff < 0)
                {
                    double lastEl = retVal.Last();
                    for(int i=0; i<Math.Abs(diff); i++)
                        retVal.Add(lastEl);
                }
                #endregion
            }
            else
                retVal = null;
            return retVal;
        }

        public static List<InputOutput> Shuffle(List<InputOutput> list)
        {
            List<InputOutput> retVal = new List<InputOutput>();
            List<InputOutput> list1 = list.Where(x => x.Output == 1).ToList();
            List<InputOutput> list2 = list.Where(x => x.Output == 2).ToList();
            List<InputOutput> list3 = list.Where(x => x.Output == 3).ToList();
            List<InputOutput> list4 = list.Where(x => x.Output == 4).ToList();
            List<InputOutput> list5 = list.Where(x => x.Output == 5).ToList();
            List<InputOutput> list6 = list.Where(x => x.Output == 6).ToList();
            List<InputOutput> list7 = list.Where(x => x.Output == 7).ToList();
            List<InputOutput> list8 = list.Where(x => x.Output == 8).ToList();
            List<InputOutput> list9 = list.Where(x => x.Output == 9).ToList();
            List<InputOutput> list10 = list.Where(x => x.Output == 10).ToList();

            for (int i = 0; i < SampleCount/ClassesCount; i++)
            {
                retVal.Add(list1[i]);
                retVal.Add(list2[i]);
                retVal.Add(list3[i]);
                retVal.Add(list4[i]);
                retVal.Add(list5[i]);
                retVal.Add(list6[i]);
                retVal.Add(list7[i]);
                retVal.Add(list8[i]);
                retVal.Add(list9[i]);
                retVal.Add(list10[i]);
            }
            return retVal;
        }
        
        public static void NetworkTest(string imagePath)
        {
            List<double> res = bp.izracunaj(GenerateInputVector(imagePath).ToArray()).ToList();
            Console.WriteLine(res[0]);
            Console.WriteLine(res[1]);
            Console.WriteLine(res[2]);
            Console.WriteLine(res[3]);
        }

        public static int GetResult(string imagePath)
        {
            int retval = 0;
            double max = 0;
            List<double> res = bp.izracunaj(GenerateInputVector(imagePath).ToArray()).ToList();
            int i = 0;
            foreach(var r in res)
            {
                i++;
                if (r > max)
                {
                    max = r;
                    retval = i;
                }
            }

            return retval;
        }
  
    }


    public class InputOutput
    {
        public List<double> Input { get; set; }
        public int Output { get; set; }
    }
}
