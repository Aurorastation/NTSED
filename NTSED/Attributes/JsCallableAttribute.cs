using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NTSED.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class JsCallableAttribute : Attribute
    {
        public JsCallableAttribute() { }
        public JsCallableAttribute(string name)
        {
            Name = name;
        }

        public string? Name { get; set; }
        public bool Enumerable { get; set; } = true;
        public bool Freeze { get; set; } = true;
    }
}
