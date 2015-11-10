using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;
using System;
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
        private HttpWebRequestHandler[] _modules;

        public Server(int port)
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var item in interfaces)
            {
                Debug.Print("IP:" + item.IPAddress);
            }

            _port = port;
            _serverThread = new Thread(StartServer);
        }

        public int Port
        {
            get { return _port; }
        }

        public void Start()
        {
            _serverThread.Start();
        }

        private void StartServer()
        {
            using (var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                server.Bind(new IPEndPoint(IPAddress.Any, Port));
                server.Listen(1);

                while (true)
                {
                    var connection = server.Accept();

                    if (connection.Poll(-1, SelectMode.SelectRead))
                    {
                        var rawRequest = string.Empty;

                        while (connection.Available > 0)
                        {
                            var bytes = new byte[connection.Available];
                            var count = connection.Receive(bytes);
                            rawRequest = rawRequest + new string(Encoding.UTF8.GetChars(bytes));
                        }

                        var request = new HttpWebRequest(rawRequest);
                        //var modules = _modules.ModuleFor(request);
                        // queue a thread to run all the matching modules with the request and chain the responses.
                        // 
                        SendResponse("HTTP/1.1 200 OK\r\nConnection: close" + "\r\n\r\n", connection);
                        connection.Close();
                    }
                }
            }
        }

        private static void SendResponse(string response, Socket connection)
        {
            byte[] returnBytes = Encoding.UTF8.GetBytes(response);
            connection.Send(returnBytes, 0, returnBytes.Length, SocketFlags.None);
        }

    }
}
