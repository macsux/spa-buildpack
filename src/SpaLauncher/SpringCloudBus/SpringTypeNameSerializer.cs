using System;
using System.Linq;
using EasyNetQ;

namespace SpaLauncher.SpringCloudBus
{
    public class SpringTypeNameSerializer : ITypeNameSerializer
    {
        public string Serialize(Type type) => type.Name;

        public Type DeSerialize(string typeName)
        {
            var type = GetType().Assembly
                .GetExportedTypes()
                .FirstOrDefault(x => x.Name == typeName);
            if(type == null)
                throw new Newtonsoft.Json.JsonException($"No CLR type matching {typeName} is found");
            return type;
        }
    }
}