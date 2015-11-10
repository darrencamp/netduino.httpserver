using Microsoft.SPOT;
using System;

namespace Netduino.Http
{
    public class HttpWebRequest
    {
        private readonly string _RequestString;
        public HttpWebRequest(string requestString)
        {
            _RequestString = requestString;
            Debug.Print("Request: " + requestString);
        }
    }
}
