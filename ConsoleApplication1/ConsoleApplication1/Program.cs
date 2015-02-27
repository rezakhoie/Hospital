using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Data;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (TextFieldParser parser = new TextFieldParser("Flow Optimization Project_BUMC data.csv"))
            {
                int arrCounter;
                int registerationCounter;
                int triageCounter;
                int dispoCounter;
                int depCounter;
                MySqlDB.NonQuery("TRUNCATE TABLE counts");
                DateTime startDateTime = new DateTime(2013, 07, 01, 00, 00, 00);
                while (true)
                {
                    String begin = startDateTime.GetDateTimeFormats()[93];
                    String end = (startDateTime + new TimeSpan(1, 0, 0)).GetDateTimeFormats()[93];
                    arrCounter = int.Parse(MySqlDB.ScalarQuery("select count(id) from raw WHERE Arr >= '" + begin.Replace(" ", ":") + "' AND Arr < '" + end.Replace(" ", ":") + "'").ToString());
                    registerationCounter = int.Parse(MySqlDB.ScalarQuery("select count(id) from raw WHERE Registeration >= '" + begin + "' AND Registeration < '" + end + "'").ToString());
                    triageCounter = int.Parse(MySqlDB.ScalarQuery("select count(id) from raw WHERE Triage >= '" + begin + "' AND Triage < '" + end + "'").ToString());
                    dispoCounter = int.Parse(MySqlDB.ScalarQuery("select count(id) from raw WHERE Dispo >= '" + begin + "' AND Dispo < '" + end + "'").ToString());
                    depCounter = int.Parse(MySqlDB.ScalarQuery("select count(id) from raw WHERE Dep >= '" + begin + "' AND Dep < '" + end + "'").ToString());
                    if (arrCounter + registerationCounter + triageCounter + dispoCounter + depCounter == 0)
                        break;
                    MySqlDB.NonQuery("insert into counts (`Interval`, `Arr`, `Registeration`, `Triage`, `Dispo`, `Dep`, `" + startDateTime.DayOfWeek.ToString() + "`, `Month" + startDateTime.Month.ToString() + "`, `Day" + startDateTime.Day.ToString() + "`, `Hour" + startDateTime.Hour.ToString() + "`) values ('" + startDateTime.GetDateTimeFormats()[93] + "', '" + arrCounter + "', '" + registerationCounter + "', '" + triageCounter + "', '" + dispoCounter + "', '" + depCounter + "', '1', '1', '1', '1')");
                    startDateTime=startDateTime.AddHours(1);
                }
            }
        }
    }
}
