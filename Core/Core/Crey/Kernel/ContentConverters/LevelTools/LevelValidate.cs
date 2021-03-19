using Crey.Contracts;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Crey.Kernel.ContentConverter.LevelTools
{
    public class LevelValidate : LevelConverterBase
    {
        public List<BadgeInfo> Badges { get; private set; } = new List<BadgeInfo>();
        public HashSet<string> Prefabs { get; private set; } = new HashSet<string>();

        private void CollectBadges(JObject content)
        {
            var badges = content["badge"];
            if (badges != null)
            {
                foreach (var instance in badges["instances"].Children())
                {
                    Badges.Add(new BadgeInfo
                    {
                        BadgeGuid = instance["uid"].Value<string>(),
                        Icon = instance["badge"].Value<string>(),
                        Name = instance["name"].Value<string>(),
                        Description = instance["description"].Value<string>(),
                        CountRequired = instance["maxCount"].Value<int>(),
                        Persistent = instance["persistent"].Value<bool>() ? BadgePersistencyType.Persistent : BadgePersistencyType.NotPersistent,
                    });
                }
            }
        }

        private void CollectPrefabs(JObject content)
        {
            foreach (var prefab in AllPrefabsByPrefabDesc(content))
            {
                Prefabs.Add(prefab);
            }
        }

        public override (ConversionResult, JObject) Convert(JObject content)
        {
            CollectBadges(content);
            CollectPrefabs(content);

            return (ConversionResult.NoChange, content);
        }

        public void Clear()
        {
            Badges.Clear();
            Prefabs.Clear();
        }
    }
}