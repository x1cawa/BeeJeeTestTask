using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace TaskBeeJee.Exceptions
{
    internal class NoInternetConnectionException : Exception
    {
        public NetworkAccess networkAccess;

        public NoInternetConnectionException(NetworkAccess network) : base("No internet connection")
        {
            networkAccess = network;
        }
    }
}
