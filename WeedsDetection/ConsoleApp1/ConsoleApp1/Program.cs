using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string dataSet = @"D:\PLANTS WEEDS\Chosen\";
            List<string> folderNames =  GetFolderNames(dataSet);
            int i = 0;
            foreach(string folderName in folderNames)
            {
                if (++i > 0)
                {
                    Directory.CreateDirectory(dataSet + "Processed_" + folderName);
                    List<string> images = GetFiles(dataSet + folderName);
                    foreach (string image in images)
                    {
                        Image<Gray, byte> result = GenerateResultImage(dataSet + folderName + "\\" + image);
                        if(result !=null)
                        result.Save(dataSet + "Processed_" + folderName + "\\" + image);
                    }
                }
            }





        }




        public static Image<Gray, byte> GenerateResultImage(string filename)
        {
            Image<Bgr, byte> img = new Image<Bgr, byte>(filename);
            Image<Gray, byte> leafThreshold = LeafThreshold(img);

            Image<Gray, byte> eroded = leafThreshold.Clone();
            var element = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(3, 3), new Point(-1, -1));
            CvInvoke.Erode(leafThreshold, eroded, element, new Point(-1, -1), 5, BorderType.Reflect, default(MCvScalar));
            //Image<Gray, byte> dilated = leafThreshold.Clone();
            //CvInvoke.Dilate(eroded, dilated, element, new Point(-1, -1), 5, BorderType.Reflect, default(MCvScalar));
            
            Image<Gray, float> sobel = leafThreshold.Sobel(0, 1, 3).Add(leafThreshold.Sobel(1, 0, 3)).AbsDiff(new Gray(0.0));
            List<Point> points = GetPoints(sobel);
            if (points.Count > 40000)
            {
                Console.WriteLine("More then 40000 points: " + filename);
                return null;
            }
            List<Point> maxDistPoints = GetMaxDistancePoints(points);
            LineSegment2D line = new LineSegment2D(maxDistPoints[0], maxDistPoints[1]);
            Gray g = new Gray();
            g.Intensity = 50;
            sobel.Draw(line, g, 2);


            Image<Gray, byte> sobelByte = sobel.Convert<Gray, byte>();
            Image<Gray, byte> thresholdFilled = new Image<Gray, byte>(sobelByte.Size);
            //test, just treshold might be better choise
            // FillLargestContour(sobelByte, thresholdFilled);
            Image<Gray, byte> rotated = RotateLeaf(leafThreshold, maxDistPoints);
            // CvInvoke.Imshow("Image", sobel);
            // CvInvoke.Imshow("Rotated", rotated);
            // CvInvoke.WaitKey(0);
            rotated = FlipLeaf(rotated);
            return rotated;
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


        public static Image<Gray, byte> LeafThreshold(Image<Bgr, byte> image)
        {
            Image<Gray, byte> retval = image.Convert<Gray, byte>();
            for(int i=0; i<image.Width; i++)
            {
                for(int j=0; j<image.Height; j++)
                {
                    Bgr bgr = image[j, i];
                    if (bgr.Green > (bgr.Red) && bgr.Green > (bgr.Blue))
                        retval[j, i] = new Gray(255);
                    else
                        retval[j, i] = new Gray(0);

                }
            }
            return retval;
        }


        public static List<Point> GetPoints(Image<Gray, float> edgeDetected)
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < edgeDetected.Width; i++)
            {
                for (int j = 0; j < edgeDetected.Height; j++)
                {
                    if (edgeDetected.Data[j,i,0] > 0)
                    {
                        points.Add(new Point() { X = i, Y = j});
                    }
                }
            }
            return points;
        }



        public static List<Point> GetMaxDistancePoints(List<Point> points)
        {
            List<Point> retval = new List<Point>();
            int maxDistance = 0;
            Point maxPoint1 = new Point();
            Point maxPoint2 = new Point();

            foreach(Point p1 in points)
            {
                foreach(Point p2 in points)
                {
                    int xDist = Math.Abs(p1.X - p2.X);
                    int yDist = Math.Abs(p1.Y - p2.Y);
                    int dist = xDist + yDist;
                    if(dist > maxDistance)
                    {
                        maxDistance = dist;
                        maxPoint1 = p1;
                        maxPoint2 = p2;
                    }

                }
            }
            retval.Add(maxPoint1);
            retval.Add(maxPoint2);

            return retval;
        }


        public static float FindDegree(int x, int y)
        {
            float value = (float)((Math.Atan2(x, y) / Math.PI) * 180f);
            if (value < 0) value += 360f;

            return value;
        }
        public static Image<Gray, byte> RotateLeaf(Image<Gray, byte> img, List<Point> maxDistancePoints)
        {
            Image<Gray, byte> retVal = null;
            Point x = maxDistancePoints[0];
            Point y = maxDistancePoints[1];
            y.X = y.X - x.X;
            y.Y = y.Y - x.Y;
            double angle = -FindDegree(y.Y, y.X);
            //y.Y = x.Y + x.Y - y.Y;

            //double angle = Math.Atan2((newY.Y - newY.X), (newX.Y - newX.X));
            Image<Gray, byte> rotated = img.Rotate(angle, new Gray(0), false);

            int startBlackCount = 0;
            bool startBlackCounted = false;
            int leafWidth = 0;
            for (int i = 0; i < rotated.Width; i++)
            {
                int whiteCount = 0;
                for (int j = 0; j < rotated.Height; j++)
                {
                    if (rotated.Data[j, i, 0] > 0)
                    {
                        whiteCount++;
                       // rotated[j, i] = new Gray(170);
                    }
                }
                if (whiteCount < 1 && !startBlackCounted)
                    startBlackCount++;
                else
                    startBlackCounted = true;
                if (whiteCount > 0)
                    leafWidth++;
            }
            
            retVal = rotated;
            retVal.ROI = new Rectangle(new Point(startBlackCount, 0), new Size(leafWidth, retVal.Height));

            return retVal;
        }



        public static Image<Gray, byte> FlipLeaf(Image<Gray, byte> img)
        {
            int half = img.Width / 2;
            int leftSideCount = 0;
            int rightSideCount = 0;
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    if (img.Data[j, i, 0] > 0)
                    {
                        if (i < half)
                            leftSideCount++;
                        else
                            rightSideCount++;
                    }
                }
            }
            if(rightSideCount > leftSideCount)
                img = img.Flip(FlipType.Horizontal);
            return img;
        }



        public static VectorOfPoint FillLargestContour(IInputOutputArray cannyEdges, IInputOutputArray result)
        {
            int largest_contour_index = 0;
            double largest_area = 0;
            VectorOfPoint largestContour;
            
            CvInvoke.BitwiseNot(result, result);

            using (Mat hierachy = new Mat())
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                IOutputArray hirarchy;

                CvInvoke.FindContours(cannyEdges, contours, hierachy, RetrType.Tree, ChainApproxMethod.ChainApproxNone);

                for (int i = 0; i < contours.Size; i++)
                {
                    MCvScalar color = new MCvScalar(0, 0, 255);

                    double a = CvInvoke.ContourArea(contours[i], false);  //  Find the area of contour
                    if (a > largest_area)
                    {
                        largest_area = a;
                        largest_contour_index = i;                //Store the index of largest contour
                    }
                    // CvInvoke.DrawContours(result, contours, largest_contour_index, new MCvScalar(255, 0, 0));
                }

                CvInvoke.FillConvexPoly(result, contours[largest_contour_index], new MCvScalar(0, 0, 255), LineType.EightConnected);
                CvInvoke.DrawContours(result, contours, largest_contour_index, new MCvScalar(0, 0, 255), 3, LineType.EightConnected, hierachy);
                largestContour = new VectorOfPoint(contours[largest_contour_index].ToArray());
                CvInvoke.BitwiseNot(result, result);
            }

            return largestContour;
        }





    }
}
