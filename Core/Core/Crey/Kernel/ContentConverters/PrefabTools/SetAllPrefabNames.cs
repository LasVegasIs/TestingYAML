using Newtonsoft.Json.Linq;

namespace Crey.Kernel.ContentConverter.PrefabTool
{
    public class SetAllPrefabNames : LevelConverterBase
    {
        private string _name;

        public SetAllPrefabNames(string name)
        {
            _name = name;
        }

        public override (ConversionResult, JObject) Convert(JObject content)
        {
            var desc = content["PrefabDesc"];
            if (desc == null) throw new ConversionError("Not a prefab");
            var instances = desc["instances"];
            if (instances == null) throw new ConversionError("Not a prefab");

            bool changed = false;
            foreach (var inst in instances.Children())
            {
                inst["prefabName"] = _name;
                changed = true;
            }

            return (changed ? ConversionResult.Modified : ConversionResult.NoChange, content);
        }
    }
}