using System;

namespace Netduino.Http
{
    public abstract class ResourceAction
    {
        public abstract void Execute(HttpRequestReceivedEventArgs args);
    }
}
