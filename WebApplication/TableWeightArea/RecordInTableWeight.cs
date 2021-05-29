using System;
using System.Collections.Generic;

namespace SocketCreatingLib
{
    public class RecordInTableWeight
    {
        public int Weight { get; set; }
        
        public int RemainderOfWeight { get; set; }

        public string IpAddress { get; set; }
        
        public int Port { get; set; }
        
        public double Duration { get; set; }
        

        public static void ResetRemaindersInTable(List<RecordInTableWeight> tableWeights)
        {
            foreach (var recordInTableWeight in tableWeights)
            {
                recordInTableWeight.ResetRemainder();
            }
        }

        private void ResetRemainder()
        {
            RemainderOfWeight = Weight;
        }
        
        public bool CalculateRemainder()
        {
            RemainderOfWeight--;

            if (RemainderOfWeight > -1)
            {
                return true;
            }

            ResetRemainder();
            return false;
        }
    }
}