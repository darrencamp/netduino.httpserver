using System;
using Netduino.Http;
using System.Collections;
using Nixie;

namespace NetduinoPlus.App
{
    public class NixieFormEncodedAction : ResourceAction
    {
        private readonly NixieModule _nixieDevice;

        private class NixieCommand
        {
            private Hashtable _colours = new Hashtable();
            private Hashtable _separator = new Hashtable();
            private Hashtable _numbers = new Hashtable();

            public NixieCommand()
            {
                Colour = NixieModule.Colour.White;
                Number = NixieModule.Number.Zero;
                Separator = NixieModule.Spacer.Off;

                _colours.Add("Green", NixieModule.Colour.Green);
                _colours.Add("Blue", NixieModule.Colour.Blue);
                _colours.Add("Red", NixieModule.Colour.Red);
                _colours.Add("Magenta", NixieModule.Colour.Magenta);
                _colours.Add("Yellow", NixieModule.Colour.Yellow);
                _colours.Add("Cyan", NixieModule.Colour.Cyan);
                _colours.Add("Black", NixieModule.Colour.Black);
                _colours.Add("White", NixieModule.Colour.White);

                _separator.Add("Off", NixieModule.Spacer.Off);
                _separator.Add("Dot", NixieModule.Spacer.Dot);
                _separator.Add("Colon", NixieModule.Spacer.Colon);
                _separator.Add("SingleQuote", NixieModule.Spacer.SingleQuote);

                _numbers.Add("0", NixieModule.Number.Zero);
                _numbers.Add("1", NixieModule.Number.One);
                _numbers.Add("2", NixieModule.Number.Two);
                _numbers.Add("3", NixieModule.Number.Three);
                _numbers.Add("4", NixieModule.Number.Four);
                _numbers.Add("5", NixieModule.Number.Five);
                _numbers.Add("6", NixieModule.Number.Six);
                _numbers.Add("7", NixieModule.Number.Seven);
                _numbers.Add("8", NixieModule.Number.Eight);
                _numbers.Add("9", NixieModule.Number.Nine);
                _numbers.Add("Off", NixieModule.Number.Off);
            }

            public NixieCommand(ArrayList commands)
                : this()
            {
                foreach (string item in commands)
                {
                    var command = item.Split('=');
                    var operand = command[0];
                    var data = command[1];
                    if (operand == "C" && _colours.Contains(data)) { Colour = (NixieModule.Colour)_colours[data]; }
                    if (operand == "S" && _separator.Contains(data)) { Separator = (NixieModule.Spacer)_separator[data]; }
                    if (operand == "N" && _numbers.Contains(data)) { Number = (NixieModule.Number)_numbers[data]; }
                }
            }

            public NixieModule.Colour Colour { get; set; }
            public NixieModule.Number Number { get; set; }
            public NixieModule.Spacer Separator { get; set; }


        }
        public NixieFormEncodedAction(NixieModule nixieDevice)
        {
            _nixieDevice = nixieDevice;
        }

        public override void Execute(HttpRequestReceivedEventArgs args)
        {
            //body should contain a series of commands seperate by /r/n
            var commands = new ArrayList();
            var startIndex = 0;
            var commandIndex = args.Body.IndexOf("&");
            while (commandIndex > 0)
            {
                commands.Add(args.Body.Substring(startIndex, commandIndex - startIndex));
                startIndex = commandIndex + 1;
                commandIndex = args.Body.IndexOf("&", startIndex);
            };
            commands.Add(args.Body.Substring(startIndex, args.Body.Length - startIndex));

            var nixieCommand = new NixieCommand(commands);
            _nixieDevice.SetBackgroundColour(nixieCommand.Colour);
            _nixieDevice.SetNumber(nixieCommand.Number);
            _nixieDevice.SetSpacer(nixieCommand.Separator);
            _nixieDevice.Display();
        }
    }
}
