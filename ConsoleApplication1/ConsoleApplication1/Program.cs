using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Data;
using Accord.Neuro.Learning;
using AForge.Neuro;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
           //dataCleaner();
           neuralNetworkTrainer();
            //nnSample();
        }

        static void nnSample()
        {
            double[][] input =
            {
                new double[] {0, 0}, new double[] {0, 1},
                new double[] {1, 0}, new double[] {1, 1}
            };

                        double[][] output = 
            {
                new double[] {0}, new double[] {1},
                new double[] {1}, new double[] {0}
            };

                        // create neural network
            ActivationNetwork network = new ActivationNetwork(new SigmoidFunction(2),
                            2, // two inputs in the network
                            2, // two neurons in the first layer
                            1); // one neuron in the second layer

                        // create teacher
            LevenbergMarquardtLearning teacher = new LevenbergMarquardtLearning(network);

                        // loop
            double error=4;
            while (error>.01)
            {
                // run epoch of learning procedure
                error = teacher.RunEpoch(input, output);
               
                // check error value to see if we need to stop
                // ...
            }
            Console.Write(network.Compute(new double[] {1, 1}).ToArray()[0] + " * ");
            Console.ReadLine();
             
        }

        static void neuralNetworkTrainer()
        {
            double testPercent=0.15;

            DataTable data = new DataTable();
            data = MySqlDB.Query("SELECT * FROM input", "input");
            int testSampleNo = (int)(data.Rows.Count * testPercent);
            double[][] input = new double[data.Rows.Count - testSampleNo][];
            double[][] testInput = new double[testSampleNo][];
            double[][] output = new double[data.Rows.Count - testSampleNo][];
            double[][] testOutput = new double[testSampleNo][];

            for(int i=0; i<data.Rows.Count-testSampleNo; i++)
                input[i] = (data.Rows[i].ItemArray.Select(x => double.Parse(x.ToString())).ToArray());
            for(int i=data.Rows.Count-testSampleNo; i<data.Rows.Count; i++)
                testInput[i - data.Rows.Count + testSampleNo] = (data.Rows[i].ItemArray.Select(x => double.Parse(x.ToString())).ToArray());

            int noOfNetworkInputs = data.Columns.Count;

            data = MySqlDB.Query("SELECT * FROM outputArr", "outputArr");
            for (int i = 0; i < data.Rows.Count-testSampleNo; i++)
                output[i] = (data.Rows[i].ItemArray.Select(x => ((double.Parse(x.ToString()))/40)).ToArray());
            for(int i=data.Rows.Count-testSampleNo; i<data.Rows.Count; i++)
                testOutput[i - data.Rows.Count + testSampleNo] = (data.Rows[i].ItemArray.Select(x => ((double.Parse(x.ToString())) / 40)).ToArray());
            for (int hiddenNeuronsNo = 5; hiddenNeuronsNo <= 60; hiddenNeuronsNo++)
            {
                for (int errorThreshold = 5; errorThreshold <= 20; errorThreshold++)
                {

                    ActivationNetwork network = new ActivationNetwork((IActivationFunction)new SigmoidFunction(.1), noOfNetworkInputs, hiddenNeuronsNo, 1);
                    LevenbergMarquardtLearning teacher = new LevenbergMarquardtLearning(network);
                    teacher.LearningRate = .5;
                    double error = 100;
                    int epochCounter = 0;
                    Boolean convergance = true;
                    while (error > errorThreshold)
                    {
                        // run epoch of learning procedure
                        error = teacher.RunEpoch(input, output);
                        epochCounter++;
                        //Console.WriteLine(error);
                        // check error value to see if we need to stop
                        // ...
                        if (epochCounter == 100)
                        {
                            convergance = false;
                            break;
                        }
                    }
                    if (!convergance)
                        Console.WriteLine("" + hiddenNeuronsNo + "\t" + errorThreshold + "\t" + epochCounter + "\tNC");
                    else
                    {
                        double sumSquaredError = 0;
                        for (int i = 0; i < testSampleNo; i++)
                            sumSquaredError += Math.Pow(((network.Compute(testInput[i]).ToArray())[0] * 40) - (testOutput[i][0] * 40), 2);
                        Console.WriteLine("" + hiddenNeuronsNo + "\t" + errorThreshold + "\t" + epochCounter + "\t" + Math.Sqrt(sumSquaredError / testSampleNo));
                    }
                }
            }
            Console.ReadLine();
        }

        static void dataCleaner(){       
            int arrCounter;
            int registerationCounter;
            int triageCounter;
            int dispoCounter;
            int depCounter;
            MySqlDB.NonQuery("TRUNCATE TABLE modified");
            MySqlDB.NonQuery("TRUNCATE TABLE input");
            MySqlDB.NonQuery("TRUNCATE TABLE output");
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
                int isHolidayNum = 0;
                int isBeforeHolidayNum = 0;
                if (isHoliday(startDateTime))
                    isHolidayNum = 1;
                if (isHoliday(startDateTime.AddDays(1)))
                    isBeforeHolidayNum = 1;
                MySqlDB.NonQuery("insert into modified (`Interval`, `Arr`, `Registeration`, `Triage`, `Dispo`, `Dep`, `" + startDateTime.DayOfWeek.ToString() + "`, `Month" + startDateTime.Month.ToString() + "`, `Day" + startDateTime.Day.ToString() + "`, `Hour" + startDateTime.Hour.ToString() + "`, `isHoliday`, `isBeforeHoliday`) values ('" + startDateTime.GetDateTimeFormats()[93] + "', '" + arrCounter + "', '" + registerationCounter + "', '" + triageCounter + "', '" + dispoCounter + "', '" + depCounter + "', '1', '1', '1', '1', '" + isHolidayNum + "', '" + isBeforeHolidayNum + "')");
                MySqlDB.NonQuery("insert into input (`" + startDateTime.DayOfWeek.ToString() + "`, `Month" + startDateTime.Month.ToString() + "`, `Day" + startDateTime.Day.ToString() + "`, `Hour" + startDateTime.Hour.ToString() + "`, `isHoliday`, `isBeforeHoliday`) values ('1', '1', '1', '1', '" + isHolidayNum + "', '" + isBeforeHolidayNum + "')");
                MySqlDB.NonQuery("insert into output (`Arr`, `Registeration`, `Triage`, `Dispo`, `Dep`) values ('" + arrCounter + "', '" + registerationCounter + "', '" + triageCounter + "', '" + dispoCounter + "', '" + depCounter + "')");
                startDateTime = startDateTime.AddHours(1);
            }         
        }

        static bool isHoliday(DateTime Date){
            if (Date.DayOfWeek == System.DayOfWeek.Saturday || Date.DayOfWeek == System.DayOfWeek.Sunday)
                return true;
            int iDay=Date.Day;
            switch (Date.Month){
                case 1:
                    if (iDay == 1 || iDay == 2)//New year
                        return true;
                    else if (Date.DayOfWeek == System.DayOfWeek.Monday && iDay > 14 && iDay < 22)//Martin Luther King B-Day 3rd Monday of January
                        return true;
                    break;
                case 2:
                    if (Date.DayOfWeek == System.DayOfWeek.Monday && iDay > 14 && iDay < 22)//Presidents Day 3rd Monday
                        return true;
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    if (Date.DayOfWeek == System.DayOfWeek.Monday && iDay > 24 && iDay < 32)
                        return true;
                    break;
                case 6:
                    break;
                case 7:
                    switch (iDay)//Independence Day
                    {
                        case 3:
                            if (Date.DayOfWeek == System.DayOfWeek.Friday)
                                return true;
                            break;
                        case 4:
                            return true;
                        case 5:
                            if (Date.DayOfWeek == System.DayOfWeek.Monday)
                                return true;
                            break;
                    }
                    break;
                case 8:
                    break;
                case 9:
                    if (Date.DayOfWeek == System.DayOfWeek.Monday && iDay < 7)//Labor day
                        return true;
                    break;
                case 10:
                     if (Date.DayOfWeek == System.DayOfWeek.Monday && iDay > 7 && iDay < 15)//Columbus day
                        return true;
                    break;
                case 11:
                    switch (iDay)//Veterans day
                    {
                        case 10:
                            if (Date.DayOfWeek == System.DayOfWeek.Friday)
                                return true;
                            break;
                        case 11:
                            if (Date.DayOfWeek != System.DayOfWeek.Saturday && Date.DayOfWeek != System.DayOfWeek.Sunday)
                                return true;
                            break;
                        case 12:
                            if (Date.DayOfWeek == System.DayOfWeek.Monday)
                                return true;
                            break;
                    }
                    if (Date.DayOfWeek == System.DayOfWeek.Thursday && iDay > 20 && iDay < 28)//thanksgiving
                        return true;
                    break;
                case 12:
                    switch (iDay)//Christmas
                    {
                        case 24:
                            if (Date.DayOfWeek == System.DayOfWeek.Friday)
                                return true;
                            break;
                        case 25:
                            if (Date.DayOfWeek != System.DayOfWeek.Saturday && Date.DayOfWeek != System.DayOfWeek.Sunday)
                                return true;
                            break;
                        case 26:
                            if (Date.DayOfWeek == System.DayOfWeek.Monday)
                                return true;
                            break;
                    }
                    if (iDay == 31 && Date.DayOfWeek == System.DayOfWeek.Friday)
                        return true;
                    break;
            }
            return false;
        }
    }
}
