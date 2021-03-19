using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Crey.Kernel.ContentConverter.Migration
{
    public class LevelMigrate_DeDupBadgeGuid : LevelConverterBase
    {
        public override (ConversionResult, JObject) Convert(JObject content)
        {
            bool changed = false;
            var badgeComponent = content["badge"];
            if (badgeComponent != null)
            {
                var version = badgeComponent["dataVersion"].Value<int>();

                bool convert = false;
                if (version == 3)
                {
                    //check for duplicate ids
                    var guid = new HashSet<string>();
                    foreach (var instance in badgeComponent["instances"].Children())
                    {
                        if (!guid.Add(instance["uid"].Value<string>()))
                        {
                            convert = true;
                            break;
                        }
                    }
                }

                if (version == 2 || convert)
                {
                    badgeComponent["dataVersion"] = 3;
                    foreach (var instance in badgeComponent["instances"].Children())
                    {
                        instance["uid"] = Guid.NewGuid().ToString().ToUpper();
                    }
                    changed = true;
                }
            }

            return (changed ? ConversionResult.Modified : ConversionResult.NoChange, content);
        }
    }
}