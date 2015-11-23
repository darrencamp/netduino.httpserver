using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Netduino.Http
{
    public class Server
    {
        private readonly Thread _serverThread;
        private readonly int _port;
        private ResourceActionCollection _modules;

        public Server(int port)
        {
            _modules = new ResourceActionCollection();

            _port = port;
            _serverThread = new Thread(StartServer);
        }

        public int Port
        {
            get { return _port; }
        }

        public void AddRoute(string route, ResourceAction action)
        {
            _modules.Add(route, action);
        }

        public void Start()
        {
            _serverThread.Start();
        }

        private void StartServer()
        {
            var listener = new HttpListener("http", _port);
            listener.Start();

            while (listener.IsListening)
            {
                var context = listener.GetContext();
                new Thread(() =>
                {
                    var args = new HttpRequestReceivedEventArgs(context);
                    var controller = _modules.Find(context.Request.RawUrl);
                    controller.Execute(args);

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.KeepAlive = false;
                    context.Response.StatusDescription = "OK";
                    context.Response.Close();
                }).Start();
            }
        }
    }
}
