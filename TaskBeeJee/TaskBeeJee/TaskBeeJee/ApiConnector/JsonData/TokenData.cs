using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TaskBeeJee.ApiConnector.JsonData
{
    [DataContract]
    public class TokenData
    {
        [DataMember(Name = "token")]
        public string Value { get; set; }
    }
}
