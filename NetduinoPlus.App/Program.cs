using System;
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
            srv.AddRoute("/v2/nixie", new NixieFormEncodedAction(nixie));
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
