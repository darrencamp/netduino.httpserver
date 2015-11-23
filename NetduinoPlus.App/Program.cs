﻿using System;
using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Netduino.Http;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;
using System.Collections;
using Nixie;

namespace NetduinoPlus.App
{
    public class NixieAction : ResourceAction
    {
        private readonly NixieModule _nixieDevice;

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
                    if (operand == 1) { Colour = (NixieModule.Colour)data; }
                    if (operand == 2) { Separator = (NixieModule.Spacer)data; }
                    if (operand == 3) { Number = (NixieModule.Number)(2 ^ data); }
                }
            }
            
            public NixieModule.Colour Colour { get; set; }
            public NixieModule.Number Number { get; set; }
            public NixieModule.Spacer Separator { get; set; }


        }
        public NixieAction(NixieModule nixieDevice)
        {
            _nixieDevice = nixieDevice;
        }

        public override void Execute(HttpRequestReceivedEventArgs args)
        {
            //body should contain a series of commands seperate by /r/n
            var commands = new ArrayList();
            var startIndex = 0;
            var commandIndex = args.Body.IndexOf("\r\n");
            while (commandIndex > 0)
            {
                commands.Add(args.Body.Substring(startIndex, commandIndex-startIndex));
                startIndex = commandIndex+2;
                commandIndex = args.Body.IndexOf("\r\n", startIndex);
            };

            var nixieCommand = new NixieCommand(commands);
            _nixieDevice.SetBackgroundColour(nixieCommand.Colour);
            _nixieDevice.SetNumber(nixieCommand.Number);
            _nixieDevice.SetSpacer(nixieCommand.Separator);
            _nixieDevice.Display();
        }
    }

    public class Program
    {
        private static NixieModule nixie;
        public static void Main()
        {
            var dataPin = new OutputPort(Pins.GPIO_PIN_D11, false);
            var shiftPin = new OutputPort(Pins.GPIO_PIN_D12, false);
            var storagePin = new OutputPort(Pins.GPIO_PIN_D13, false);
            var brightnessPort = new PWM(PWMChannels.PWM_PIN_D10, 100.0, 0.5, false);
            nixie = new NixieModule(
                dataPin,
                shiftPin,
                storagePin,
                brightnessPort
            );

            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            Thread.Sleep(1000);
            foreach (var item in interfaces)
            {
                Debug.Print("IP:" + item.IPAddress);
            }

            var srv = new Netduino.Http.Server(80);
            srv.AddRoute("/nixie", new NixieAction(nixie));
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
