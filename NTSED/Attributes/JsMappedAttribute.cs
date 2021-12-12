using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NTSED.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]

    public class JsMappedAttribute : Attribute
    {
        public JsMappedAttribute() { }
        public JsMappedAttribute(string name)
        {
            Name = name;
        }
        public string? Name { get; set; }
        public bool Enumerable { get; set; } = true;
        public bool Freeze { get; set; } = true;
    }
}
