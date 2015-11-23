using System;
using System.Collections;

namespace Netduino.Http
{
    public class ResourceActionCollection : IEnumerable
    {
        private System.Collections.Hashtable _actions;

        public ResourceActionCollection()
        {
            _actions = new Hashtable();
        }

        public void Add(string route, ResourceAction controller)
        {
            _actions.Add(route, controller);
        }

        public ResourceAction Find(string route)
        {
            return _actions[route] as ResourceAction;
        }

        public IEnumerator GetEnumerator()
        {
            return _actions.GetEnumerator();
        }
    }
}
