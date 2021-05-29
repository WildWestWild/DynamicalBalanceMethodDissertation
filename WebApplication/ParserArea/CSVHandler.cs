using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CsvHelper;
using SocketCreatingLib;

namespace WebApplication.ParserArea
{
    //Запускается с запроса пользователя
    public static class CSVHandler
    {
        private static long countRequest;
        
        private static int maxRequestCountRange = 8;

        private static int minRequestCountRange = 3;

        private static ConcurrentDictionary<long, bool> dictionaryChangeLoad;

        public static void UseCSVHandler(IUsableTable tableHandler)
        {
            try
            {
                countRequest++;

                if (dictionaryChangeLoad.ContainsKey(countRequest))
                {
                    if (dictionaryChangeLoad[countRequest])
                    {
                        tableHandler.ChangeTableToLowLoad();
                    }
                    else
                    {
                        tableHandler.ChangeTableToHighLoad();
                    }
                }
            }
            catch
            {
                Console.WriteLine($"Key №{countRequest}");
            }
        }
        
        private static void ResetCounters(out long highLoadCounter, out long lowLoadCounter)
        {
            highLoadCounter = 0;
            lowLoadCounter = 0;
        }

        public static void ResetRequestCounter()
        {
            countRequest = 0;
        }

        public static void StartReadCSVAsync()
        {
            Task.Factory.StartNew(StartReadCSV, TaskCreationOptions.LongRunning);
        }


        private static void StartReadCSV()
        {
            dictionaryChangeLoad = new ConcurrentDictionary<long, bool>();
            ResetCounters(out var highLoadCounter, out var lowLoadCounter);
            string dirPath = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            string filePath = dirPath + @"/MLDataRequest.csv";
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Read();
                        csv.ReadHeader();
                        while (csv.Read())
                        {
                            var record = csv.GetRecord<RecordInFile>();
                            switch (record.Load)
                            {
                                case "low": lowLoadCounter++; 
                                    if (lowLoadCounter == maxRequestCountRange 
                                        && highLoadCounter < minRequestCountRange)
                                    {
                                        dictionaryChangeLoad.TryAdd(record.IdRequest - lowLoadCounter - highLoadCounter, true);
                                        dictionaryChangeLoad.TryAdd(record.IdRequest, false);
                                        ResetCounters(out highLoadCounter, out lowLoadCounter);
                                    }
                                    break;
                                default:
                                    highLoadCounter++;
                                    if (highLoadCounter == minRequestCountRange)
                                    {
                                        ResetCounters(out highLoadCounter, out lowLoadCounter);
                                    }
                                    break;
                            }
                        }
                    } 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}