#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketCreatingLib
{
    public class TableWeightHandler: ExtensionRequest, IUsableTable
    {
        // WeightTable - хранит в себе ссылку на одну из таблиц, 
        public static List<RecordInTableWeight>? WeightTable { get; private set; }
        private List<RecordInTableWeight> HighLoadTable { get; set; }
        private List<RecordInTableWeight>? LowLoadTable { get; set; }
        public static List<RequestTime>? ArrayOfRequests { get; set; }
        private Stopwatch? Stopwatch { get; set; }

        private int positionOfListTable;

        private const int MiddleLoadWebAPI = 52;

        public TableWeightHandler()
        {
            // Инициализация таблицы высокой нагрузки
            HighLoadTable = new List<RecordInTableWeight>();
            WeightTable = HighLoadTable;
            positionOfListTable = 0;
            ArrayOfRequests = new List<RequestTime>();
        }

        private void AddConnectionInTables(string ipAdress, int port, int weight)
        {
            if (HighLoadTable.FirstOrDefault(riTw => riTw.IpAddress == ipAdress && riTw.Port == port) == null)
            {
                if (weight == 0)
                {
                    throw new Exception("Weight cant be zero!");
                }
                // Добавляем сервер в таблицу
                HighLoadTable.Add( new RecordInTableWeight
                {
                    Weight = weight,
                    IpAddress = ipAdress,
                    Port = port,
                    RemainderOfWeight = weight
                });
                // Сортируем HighLoad по убыванию веса и пересоздаём LowLoad
                ReloadTables();
            }
        }

        private void RemoveConnectionInTables(string ipAdress, int port)
        {
            var recordInTableWeight = HighLoadTable.FirstOrDefault(riTw => riTw.IpAddress == ipAdress && riTw.Port == port);
            if (recordInTableWeight != null)
            {
                // Удаляем из HighLoad
                HighLoadTable.Remove(recordInTableWeight);
                // Сортируем HighLoad по убыванию веса и осонове данных после сортировки пересоздаём LowLoad
                ReloadTables();
            }
        }
        private void ReloadTables()
        {
            HighLoadTable = HighLoadTable.OrderBy(r => r.Weight).ToList();
            LowLoadTable = RevertWeightTable();
        }

        public void UseTable(WebServerRequest webServerRequest)
        {
            switch (webServerRequest.StatusRequest)
            {
                case StatusRequest.Add: AddConnectionInTables(webServerRequest.IpAddress, webServerRequest.Port, webServerRequest.Weight); break;
                case StatusRequest.Remove: RemoveConnectionInTables(webServerRequest.IpAddress, webServerRequest.Port); break;
            }
        }
        public void ChangeTableToHighLoad()
        {
            WeightTable = HighLoadTable;
            RecordInTableWeight.ResetRemaindersInTable(WeightTable);
            Console.WriteLine("High Load Mode has started");
        }
        
        public void ChangeTableToLowLoad()
        {
            WeightTable = LowLoadTable;
            RecordInTableWeight.ResetRemaindersInTable(WeightTable);
            Console.WriteLine("Low Load Mode has started");
        }

        public void StartTimer()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        private List<RecordInTableWeight> RevertWeightTable()
        {
            int length = HighLoadTable.Count;
            if (length>1)
            {
                var lowLoadTable = new List<RecordInTableWeight>();
                int penultimate = HighLoadTable.Count - 1;
                for (int i = 0; i < penultimate; i++)
                {
                    RecordInTableWeight startRecord = HighLoadTable[i];
                    RecordInTableWeight endRecord = HighLoadTable[penultimate - i];
                    
                    if (i > penultimate - i)
                    {
                        break;
                    }

                    if (i == penultimate - i)
                    {
                        lowLoadTable.Add(startRecord);
                        break;
                    }
                    
                    // StartRecord Add
                    lowLoadTable.Add(new RecordInTableWeight
                    {
                        IpAddress = startRecord.IpAddress,
                        Port = startRecord.Port,
                        Weight = endRecord.Weight,
                        RemainderOfWeight = endRecord.RemainderOfWeight
                    });
                    // EndRecord Add
                    lowLoadTable.Add(new RecordInTableWeight
                    {
                        IpAddress = endRecord.IpAddress,
                        Port = endRecord.Port,
                        Weight = startRecord.Weight,
                        RemainderOfWeight = startRecord.RemainderOfWeight
                    });

                }
                
                return lowLoadTable.OrderBy(r=>r.Weight).ToList();
            }
            return HighLoadTable;
        }

        private void SetDurationInRecordTableWeight(WebServerRequest webRequest)
        {
            var recordInTableWeight = HighLoadTable.FirstOrDefault(
                riTw => riTw.IpAddress == webRequest.IpAddress && riTw.Port == webRequest.Port);
            if (recordInTableWeight != null)
            {
                recordInTableWeight.Duration += webRequest.Duration;
                Console.WriteLine($"Duration on port {recordInTableWeight.Port} is {recordInTableWeight.Duration}");
            }
            else
            {
                Console.WriteLine($"IP {webRequest.IpAddress} and Port {webRequest.Port} is not connected!!!");
            }
        }

        public void RemoveDurationInWeightTable()
        {
            foreach (var recordInTableWeight in WeightTable!)
            {
                recordInTableWeight.Duration = default;
            }
        }

        #region Async Methods
        
        private async Task SendRequestByTableWeight(WebServerRequest webServerRequest, List<RecordInTableWeight>? tableWeights)
        {
            try
            {
                int count = WeightTable!.Count;
                if (positionOfListTable >= count)
                {
                    positionOfListTable = 0;
                }
                if (count > 0)
                {
                    RecordInTableWeight recordInTableWeight = tableWeights![positionOfListTable];
                    if (recordInTableWeight.CalculateRemainder())
                    {
                        await SendRequestByIndexOfTable(recordInTableWeight , webServerRequest);
                    }
                    else
                    {
                        positionOfListTable++;
                        await SendRequestByTableWeight(webServerRequest, tableWeights);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        private async Task SendRequestByIndexOfTable(RecordInTableWeight recordInTableWeight, WebServerRequest webServerRequest)
        {
            try
            {
                var tcpClient = new TcpClient();
                tcpClient.Connect(recordInTableWeight.IpAddress, recordInTableWeight.Port);
                NetworkStream stream = tcpClient.GetStream();

                string responce = WebServerRequest.Serialize(webServerRequest);
                byte[] data = Encoding.UTF8.GetBytes(responce);
                stream.Write(data, 0, data.Length);
                stream.Flush();

                await Task.Run(() =>
                {
                    string requestString = stream.GetStringByStream();
                    WebServerRequest serverInClusterRequest = WebServerRequest.Deserialize(requestString);
                    // Закрываем потоки 
                    stream.Close();
                    tcpClient.Close();
                    if (serverInClusterRequest != null && serverInClusterRequest.Duration != 0)
                    {
                        SetDurationInRecordTableWeight(serverInClusterRequest);
                        var timeMilliseconds = RequestTime.GetSeconds(Stopwatch!.ElapsedMilliseconds);
                        var load = int.Parse(serverInClusterRequest.WebApiCommandTime);
                        ArrayOfRequests?.Add(new RequestTime
                        {
                            TimeMilliseconds = timeMilliseconds,
                            Load = load,
                            MiddleLoadLine = MiddleLoadWebAPI
                        });
                    } 
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        public async Task StaticSendRequestByTableWeight(WebServerRequest webServerRequest)
        {
            await SendRequestByTableWeight(webServerRequest, HighLoadTable);
        }
        public async Task DynamicSendRequestByTableWeight(WebServerRequest webServerRequest)
        {
            await SendRequestByTableWeight(webServerRequest, WeightTable);
        } 

        #endregion
    }
}