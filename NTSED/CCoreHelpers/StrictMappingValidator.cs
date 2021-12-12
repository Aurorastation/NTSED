using CCore.Net.Managed.Mapping;
using NTSED.Attributes;
using System.Reflection;

namespace NTSED.CCoreHelpers
{
    public class StrictMappingValidator : MappingValidator
    {
        public static StrictMappingValidator Instance = new StrictMappingValidator();

        public override MappingInfo Map(Type type)
        {
            var jsObjectAttribute = type.GetCustomAttribute<JsObjectAttribute>();
            if(jsObjectAttribute == null)
                return new MappingInfo { Mapped = false };
            return new MappingInfo { 
                Mapped = true, 
                Freeze = jsObjectAttribute.Freeze,
            };
        }

        public override MappingInfo Map(Type type, FieldInfo field)
        {
            var jsMappedAttribute = field.GetCustomAttribute<JsMappedAttribute>();
            if (jsMappedAttribute == null)
                return new MappingInfo { Mapped = false };
            return new MappingInfo { 
                Mapped = true, 
                Name = jsMappedAttribute.Name ?? field.Name, 
                Enumerable = jsMappedAttribute.Enumerable, 
                Freeze = jsMappedAttribute.Freeze,
            };
        }

        public override MappingInfo Map(Type type, PropertyInfo property)
        {
            var jsMappedAttribute = property.GetCustomAttribute<JsMappedAttribute>();
            if (jsMappedAttribute == null)
                return new MappingInfo { Mapped = false };
            return new MappingInfo
            {
                Mapped = true,
                Name = jsMappedAttribute.Name ?? property.Name,
                Enumerable = jsMappedAttribute.Enumerable,
                Freeze = jsMappedAttribute.Freeze,
            };
        }

        public override MappingInfo Map(Type type, MethodInfo method)
        {
            var jsCallableAttribute = method.GetCustomAttribute<JsCallableAttribute>();
            if (jsCallableAttribute == null)
                return new MappingInfo { Mapped = false };
            return new MappingInfo {
                Mapped = IsFullyFledgedMethod(method),
                Name = jsCallableAttribute.Name ?? method.Name,
                Enumerable = jsCallableAttribute.Enumerable,
                Freeze = jsCallableAttribute.Freeze,
            };
        }
    }
}
