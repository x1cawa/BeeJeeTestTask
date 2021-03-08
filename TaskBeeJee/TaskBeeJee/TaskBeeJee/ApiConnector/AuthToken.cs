using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBeeJee.ApiConnector
{
    public class AuthToken
    {
        public const int TOKEN_TIMEOUT_HOURS = 24;
        public readonly string Value;
        public readonly string Login;
        public readonly string Password;
        public readonly DateTime Created;
        public AuthToken(string value, DateTime created, string login, string password)
        {
            Value = value;
            Login = login;
            Password = password;
            Created = created;
        }

        public bool IsTimeOut()
        {
            return DateTime.Now - Created >= TimeSpan.FromHours(TOKEN_TIMEOUT_HOURS);
        }
    }
}
