using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Crey.Kernel.ContentConverter.PrefabTool
{
    public class PrefabValidate : LevelConverterBase
    {
        private string _name;

        public PrefabValidate(string name)
        {
            _name = name;
        }

        public override (ConversionResult, JObject) Convert(JObject content)
        {
            var desc = content["PrefabDesc"];
            if (desc == null) throw new ConversionError("Not a prefab");
            var instances = desc["instances"];
            if (instances == null) throw new ConversionError("Not a prefab");

            var parts = new HashSet<string>();

            foreach (var inst in instances.Children())
            {
                var nm = inst["prefabName"].Value<string>();
                if (nm != _name)
                    throw new ConversionError("Required name is not matching");

                var part = inst["partName"].Value<string>();
                if (!parts.Add(part))
                {
                    throw new ConversionError($"Duplicate part name {part} in {_name}");
                }
            }

            return (ConversionResult.NoChange, content);
        }
    }
}