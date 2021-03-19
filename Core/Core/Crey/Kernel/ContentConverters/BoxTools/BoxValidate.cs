using System.Collections.Generic;
using Crey.Contracts.XportContracts;
using Newtonsoft.Json.Linq;

namespace Crey.Kernel.ContentConverter.BoxTools
{
    public class BoxValidate : BoxToolBase
    {
        private string _name;

        public BoxValidate(string name)
        {
            _name = name;
            Prefabs = new HashSet<string>();
        }

        public HashSet<string> Prefabs { get; private set; }

        public override (ConversionResult, JObject) Convert(JObject content)
        {
            CheckGUID(content);
            CollectPrefabs(content);

            return (ConversionResult.NoChange, content);
        }

        private void CollectPrefabs(JObject content)
        {
            foreach (var prefab in AllPrefabsByPrefabDesc(content))
            {
                Prefabs.Add(prefab);
            }
        }

        private void CheckGUID(JObject content)
        {
            var box = FindRootBox(content);
            if (box == null) throw new ConversionError("Not a box");

            if (box["GUID"].ToString() != _name)
                throw new ConversionError("Name a box");
        }
    }
}