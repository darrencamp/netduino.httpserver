using Microsoft.SPOT;
using System;
using System.Net;
using System.Text;

namespace Netduino.Http
{
    public class HttpRequestReceivedEventArgs
    {
        public HttpRequestReceivedEventArgs(HttpListenerContext context)
        {
            Request = context.Request;
            HasContent = context.Request.ContentLength64 > 0;

            if (HasContent)
            {
                int length = (int)context.Request.ContentLength64;
                var bytes = new byte[length];
                context.Request.InputStream.Read(bytes, 0, length);
                Body = new string(Encoding.UTF8.GetChars(bytes));
                Debug.Print(Body);
            }
        }

        public string Body { get; private set; }
        public HttpListenerRequest Request { get; private set; }
        public bool HasContent { get; private set; }
    }
}
