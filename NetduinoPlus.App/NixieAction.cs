using System;
using Netduino.Http;
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
                    if (operand == 3)
                    {
                        switch (data)
                        {
                            case 0:
                                Number = NixieModule.Number.Zero;
                                break;
                            case 1:
                                Number = NixieModule.Number.One;
                                break;
                            case 2:
                                Number = NixieModule.Number.Two;
                                break;
                            case 3:
                                Number = NixieModule.Number.Three;
                                break;
                            case 4:
                                Number = NixieModule.Number.Four;
                                break;
                            case 5:
                                Number = NixieModule.Number.Five;
                                break;
                            case 6:
                                Number = NixieModule.Number.Six;
                                break;
                            case 7:
                                Number = NixieModule.Number.Seven;
                                break;
                            case 8:
                                Number = NixieModule.Number.Eight;
                                break;
                            case 9:
                                Number = NixieModule.Number.Nine;
                                break;
                            default:
                                Number = NixieModule.Number.Off;
                                break;
                        }
                    }
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
            var commandIndex = args.Body.IndexOf("\n");
            while (commandIndex > 0)
            {
                commands.Add(args.Body.Substring(startIndex, commandIndex - startIndex));
                startIndex = commandIndex + 1;
                commandIndex = args.Body.IndexOf("\n", startIndex);
            };

            var nixieCommand = new NixieCommand(commands);
            _nixieDevice.SetBackgroundColour(nixieCommand.Colour);
            _nixieDevice.SetNumber(nixieCommand.Number);
            _nixieDevice.SetSpacer(nixieCommand.Separator);
            _nixieDevice.Display();
        }
    }
}
