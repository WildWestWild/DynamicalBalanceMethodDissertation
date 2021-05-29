using System;

namespace SocketCreatingLib
{
    public struct RequestTime
    {
        public string TimeMilliseconds;

        public int Load;

        public int MiddleLoadLine;

        public static string GetSeconds(long timeMilliseconds)
        {
            var number = Math.Round((double) timeMilliseconds / 10000, 3);
            var partsOfNumber = number.ToString().Split(',');
            return partsOfNumber[0] + "s " + partsOfNumber[1] + "m";
        }
    }
}