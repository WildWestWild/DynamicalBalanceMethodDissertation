using System.Threading.Tasks;

namespace SocketCreatingLib
{
    public interface IUsableTable
    {
        void UseTable(WebServerRequest webServerRequest);

        Task DynamicSendRequestByTableWeight(WebServerRequest webServerRequest);

        Task StaticSendRequestByTableWeight(WebServerRequest webServerRequest);

        void ChangeTableToLowLoad();

        void ChangeTableToHighLoad();

        void StartTimer();

        void RemoveDurationInWeightTable();
    }
}