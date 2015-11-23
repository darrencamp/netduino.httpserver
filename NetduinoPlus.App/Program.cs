using System;
using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Netduino.Http;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;
using System.Collections;

namespace NetduinoPlus.App
{
    public class NixieAction : ResourceAction
    {
        private readonly object _nixieDevice;

        private class NixieCommand
        {
            
            public NixieCommand(ArrayList commands)
            {
                // high byte: command, low byte: data
                // Command: 0x01 = Colour, Data: 0..7
                // Command: 0x02 = Seperator, data: 0..3
                // Command: 0x03 = Number, data: 0..10
                // eg sequence
                // 0x0200 - separator off
                // 0x0103 - colour green
                // 0x0310 - number 0
                foreach (string item in commands)
                {
                    var command = Convert.ToUInt16(item);
                    var operand = (byte)(command >> 8);
                    var data = (byte)(command & 0xFF);
                    if (operand == 1) { Colour = data; }
                    if (operand == 2) { Separator = data; }
                    if (operand == 3) { Number = 2 ^ data; }
                }
            }
            
            public byte Colour { get; set; }
            public int Number { get; set; }
            public byte Separator { get; set; }


        }
        public NixieAction(object nixieDevice)
        {
            _nixieDevice = nixieDevice;
        }

        public override void Execute(HttpRequestReceivedEventArgs args)
        {
            //body should contain a series of commands seperate by /r/n
            var commands = new ArrayList();
            var startIndex = 0;
            var commandIndex = args.Body.IndexOf("\r\n");
            do
            {
                commands.Add(args.Body.Substring(startIndex, commandIndex));
                startIndex = commandIndex;
                commandIndex = args.Body.IndexOf("\r\n", commandIndex);
            } while (commandIndex != 0);

            var nixieCommand = new NixieCommand(commands);
            _nixieDevice.SetColour(nixieCommand.Colour);
            _nixieDevice.SetNumber(nixieCommand.Number);
            _nixieDevice.SetSeparator(nixieCommand.Separator);
            _nixieDevice.Display();
        }
    }

    public class Program
    {
        public static void Main()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            Thread.Sleep(1000);
            foreach (var item in interfaces)
            {
                Debug.Print("IP:" + item.IPAddress);
            }

            var srv = new Netduino.Http.Server(80);
            srv.AddRoute("/nixie", new NixieAction());
            srv.Start();

            using (var led = new OutputPort(Pins.ONBOARD_LED, false))
            {
                // write your code here
                while (true)
                {
                    led.Write(true);
                    Thread.Sleep(250);
                    led.Write(false);
                    Thread.Sleep(250);
                }
            };
        }

    }
}
