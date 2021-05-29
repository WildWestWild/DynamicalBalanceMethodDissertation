#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SocketCreatingLib;

namespace WebApplication.ParserArea
{
    public class JSONHandler
    {
        public static void AddArrayOfJSONRequests(List<RequestTime>? arrayOfRequests)
        {
            var arrayOfRequestsInOrder = arrayOfRequests?.OrderBy(r=> r.TimeMilliseconds);

            // serialize JSON directly to a file
            using (StreamWriter file = File.CreateText(@"ParserArea/ArrayOfRequest.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, arrayOfRequestsInOrder);
            }
        }
    }
}