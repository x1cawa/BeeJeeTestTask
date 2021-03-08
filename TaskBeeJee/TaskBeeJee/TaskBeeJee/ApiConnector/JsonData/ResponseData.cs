using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TaskBeeJee.ApiConnector.JsonData
{
    [DataContract]
    public class ResponseData<T>
    {
        [DataMember(Name = "status")]
        public string Status { get; set; }
        [DataMember(Name = "message")]
        public T Data { get; set; }

        public bool IsStatusOk() { return Status.Equals("ok"); }
        public string GetErrorMessage()
        {
            string res = Data as string;
            if (res == null)
                return string.Empty;
            else
                return res;
        }
    }
}
