using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (TextFieldParser parser = new TextFieldParser(@"C:\Users\Reza\Desktop\Flow Optimization Project_BUMC data.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                Console.WriteLine("Enter start date and time: (yyyy-MM-dd HH:mm:ss)");
                String queryStart = Console.ReadLine();
                Console.WriteLine("Enter end date and time: (yyyy-MM-dd HH:mm:ss)");
                String queryEnd = Console.ReadLine();
                DateTime arr;
                DateTime registeration;
                DateTime triage;
                DateTime dispo;
                DateTime dep;
                DateTime queS;
                DateTime queE;
                int arrCounter = 0;
                int registerationCounter = 0;
                int triageCounter = 0;
                int dispoCounter = 0;
                int depCounter = 0;
                string pattern = "yyyy-MM-dd HH:mm:ss";
                DateTime.TryParseExact(queryStart, pattern, null, DateTimeStyles.None, out queS);
                DateTime.TryParseExact(queryEnd, pattern, null, DateTimeStyles.None, out queE);                
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    //...
                    DateTime.TryParseExact(fields[0], pattern, null, DateTimeStyles.None, out arr);
                    if (DateTime.Compare(queS, arr)==-1 && DateTime.Compare(queE, arr)==1)
                    {
                        arrCounter++;
                    }
                    DateTime.TryParseExact(fields[3], pattern, null, DateTimeStyles.None, out registeration);
                    if (DateTime.Compare(queS, registeration) == -1 && DateTime.Compare(queE, registeration) == 1)
                    {
                        registerationCounter++;
                    }
                    DateTime.TryParseExact(fields[5], pattern, null, DateTimeStyles.None, out triage);
                    if (DateTime.Compare(queS, triage) == -1 && DateTime.Compare(queE, triage) == 1)
                    {
                        triageCounter++;
                    }
                    DateTime.TryParseExact(fields[9], pattern, null, DateTimeStyles.None, out dispo);
                    if (DateTime.Compare(queS, dispo) == -1 && DateTime.Compare(queE, dispo) == 1)
                    {
                        dispoCounter++;
                    }
                    DateTime.TryParseExact(fields[10], pattern, null, DateTimeStyles.None, out dep);
                    if (DateTime.Compare(queS, dep) == -1 && DateTime.Compare(queE, dep) == 1)
                    {
                       depCounter++;
                    }
                }
                Console.WriteLine("Arr: "+ arrCounter);
                Console.WriteLine("Registeration: " + registerationCounter);
                Console.WriteLine("Triage: " + triageCounter);
                Console.WriteLine("Dispo: " + dispoCounter);
                Console.WriteLine("dep: " + depCounter);
                Console.ReadLine();
            }
        }
    }
}
