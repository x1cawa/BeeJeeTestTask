using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBeeJee.Exceptions
{
    class StatusCodeNotOkException : Exception
    {
        public System.Net.HttpStatusCode StatusCode;
        public string Content;
        public StatusCodeNotOkException(System.Net.HttpStatusCode code, string content) : base($"Bad response: {code}, {content}") { StatusCode = code; Content = content; }
    }
}
