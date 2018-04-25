using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeedDetection
{
    public class ToMnistSize
    {
        public static void Convert()
        {
            string dataSet = @"D:\PLANTS WEEDS\MNIST_size\Test\";
            List<string> folderNames = GetFolderNames(dataSet);
            int i = 0;
            foreach (string folderName in folderNames)
            {
                if (++i > 0)
                {
                    Directory.CreateDirectory(dataSet + "MNIST_" + folderName);
                    List<string> images = GetFiles(dataSet + folderName);
                    foreach (string image in images)
                    {
                        Image<Bgr, byte> img = new Image<Bgr, byte>(dataSet + folderName + "\\" + image);
                        Image<Bgr, byte> resized = img.Resize(28, 28, Emgu.CV.CvEnum.Inter.Linear);
                        resized.Save(dataSet + "MNIST_" + folderName + "\\" + image);
                    }
                }
            }
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
            return retval;
        }
    }
}
