using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeedDetection;

namespace Test
{
    public class CalculateF1FromCSV
    {
        public static List<ResultElement> Calc()
        { 
            DataTable dt = CsvToDataTable.Convert(@"C:\Users\Nebojsa\Desktop\crColor.csv");
            List<ResultElement> elems = new List<ResultElement>();
            foreach(DataRow dr in dt.Rows){
                    elems.Add(new ResultElement()
                    {
                        ExpectedResult = NameToNum(dr[0].ToString()),
                        CalculatedResult = NameToNum(dr[1].ToString()),
                    });
            }
            return elems;
        }

        public static int NameToNum(string name)
        {
            int retval = 0;
            name = name.Trim();
            switch (name)
            {
                case "black grass": retval = 1; break;
                case "charlock": retval = 2; break;
                case "cleavers": retval = 3; break;
                case "common chickweed": retval = 4; break;
                case "fat hen": retval = 5; break;
                case "loose silky bent": retval = 6; break;
                case "maize": retval = 7; break;
                case "Shepherd s Purse": retval = 8; break;
                case "Shepherd's Purse": retval = 8; break;
                case "small flowered cranesbill": retval = 9; break;
                case "sugar beet": retval = 10; break;
            }

            return retval;
        }


    }
}
