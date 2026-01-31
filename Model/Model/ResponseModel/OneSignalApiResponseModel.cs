using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Model.ResponseModel
{
    public enum OneSignalNotificationObjectType
    {
        None,
        RefreshToken,
        ChatSendMessage,
        ChatUpdateMessage,
        ChatDeleteMessage,
        ApprovedProcess
    }

    public enum ChatMessageActionType
    {
        None,
        UpdateMessage,
        DeleteMessage,
        AddMessage
    }

    public class OneSignalApiResponseModel
    {
        public int? errorCode { get; private set; }

        public string? errorMessage { get; private set; }

        public object? data { get; private set; }

        public OneSignalApiResponseModel()
        {
            errorCode = (int)HttpStatusCode.BadRequest;
            errorMessage = string.Empty;
        }

        public void SetMessage(string msg)
        {
            errorMessage = msg;
        }

        public void SetData(object objData)
        {
            data = objData;
        }

        public void SetCode(HttpStatusCode returnCode)
        {
            errorCode = (int)returnCode;
        }

        public void SetCode(int returnCode)
        {
            errorCode = returnCode;
        }
    }

    public class OneSignalAdditionalPayloadModel
    {
        public string? ObjectType { get; set; }

        public string? JsonContent { get; set; }
    }

    public class OneSignalResultModel
    {
        public string? id { get; set; }

        public int? recipients { get; set; }

        public string? external_id { get; set; }

        public object? errors { get; set; }
    }

    public class OneSignalResultErrorModel
    {
        public string[]? invalid_player_ids { get; set; }
    }
}