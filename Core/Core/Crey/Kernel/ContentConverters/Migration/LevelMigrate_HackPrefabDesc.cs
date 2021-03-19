using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Crey.Kernel.ContentConverter.Migration
{
    public class LevelMigrate_HackPrefabDesc : LevelConverterBase
    {
        public override (ConversionResult, JObject) Convert(JObject content)
        {
            bool changed = false;
            var go = content["_go"];
            if (go == null)
                throw new ConversionError("Missing game objects");

            var prefab = content["PrefabDesc"];
            if (prefab == null)
            {
                // create initial prefab desc
                prefab = content["PrefabDesc"] = JObject.Parse(@"{""dataVersion"":1,""instances"":[]}");
                changed = true;
            }

            JArray prefabDescs = (JArray)prefab["instances"];

            //load initial prefab descs
            var idToPrefab = new Dictionary<int, string>();
            foreach (var instance in prefabDescs)
            {
                var name = instance["prefabName"].Value<string>();
                var id = instance["id"].Value<int>();
                if (name != null && name != "")
                    idToPrefab.Add(id, name);
                //else
                //    Console.WriteLine($"Invalid prefab name for {id}");
            }

            foreach (var goInst in go["instances"].Children())
            {
                var id = goInst["id"].Value<int>();
                if (idToPrefab.ContainsKey(id))
                    continue;

                changed = true;
                try
                {
                    var mr = FindComponentById(content, "ModelRender", id);
                    if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/gameflow/startpoint.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/gameflow/startpoint.prefab\",\"partName\":\"fixed\",\"userName\":\"\",\"ui_filter\":\"power, name, other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/objects/flag/flag_checker.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/gameflow/levelfinish.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/characters/player/mm_adult/mm_adult.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/gameflow/startpoint.prefab\",\"partName\":\"character\",\"userName\":\"\",\"ui_filter\":\"other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/characters/player/mm_basechar/mm_basechar.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/characters/player/mm_basechar/mm_basechar.prefab\",\"partName\":\"character\",\"userName\":\"\",\"ui_filter\":\"other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/sensor.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/sensor.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/checkpoint.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/checkpoint.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/characters/player/test/barrel/barrel.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/characters/player/test/barrel/barrel.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, life, name, other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/badge.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/badge.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/textoverlay.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/textoverlay.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/input_trigger.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/input_trigger.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/static_camera.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/static_camera.prefab\",\"partName\":\"camera\",\"userName\":\"\",\"ui_filter\":\"power, name, other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/grounds/ground_flat.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/grounds/ground_flat.prefab\",\"partName\":\"camera\",\"userName\":\"\",\"ui_filter\":\"power, life, name, other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/seeker.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/seeker.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/box.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/box.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/nature/plants/ash/ash_medium_red.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/nature/plants/ash/ash_medium_red.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/lift_fixed.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/lift.prefab\",\"partName\":\"fixed\",\"userName\":\"\",\"ui_filter\":\"power, name, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/lift_moving.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/lift.prefab\",\"partName\":\"moving\",\"userName\":\"\",\"ui_filter\":\"other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/rotator_fixed.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/rotator.prefab\",\"partName\":\"fixed\",\"userName\":\"\",\"ui_filter\":\"power, name, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/rotator_moving.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/rotator.prefab\",\"partName\":\"moving\",\"userName\":\"\",\"ui_filter\":\"other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/joint_fixed.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/joint.prefab\",\"partName\":\"fixed\",\"userName\":\"\",\"ui_filter\":\"power, name, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/joint_moving.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/joint.prefab\",\"partName\":\"moving\",\"userName\":\"\",\"ui_filter\":\"other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/directional_mover_fixed.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/directional_mover.prefab\",\"partName\":\"fixed\",\"userName\":\"\",\"ui_filter\":\"power, name, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/directional_mover_moving.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/directional_mover.prefab\",\"partName\":\"moving\",\"userName\":\"\",\"ui_filter\":\"other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/animator_fixed.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/animator.prefab\",\"partName\":\"fixed\",\"userName\":\"\",\"ui_filter\":\"power, name, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/animator_moving.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/animator.prefab\",\"partName\":\"moving\",\"userName\":\"\",\"ui_filter\":\"other, \"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/boxes/airship/airship.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/boxes/airship/airship.prefab\",\"partName\":\"part_0\",\"userName\":\"\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/generator.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/generator.prefab\",\"partName\":\"part_0\",\"userName\":\"\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/connector.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/connector.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/nature/plants/oak/tree_oak_red.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/nature/plants/oak/tree_oak_red.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/pickups/pickup_01.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/pickups/pickup_01.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/pickups/pickup_02.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/pickups/pickup_02.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/timer.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/timer.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/damage.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/damage.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/audio.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/audio.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/nature/rocks//block_rocks/block_rock_sandy_2.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/nature/rocks/block_rocks/block_rock_sandy_2.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, life, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/counter.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/counter.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/items/weapons/rifle01.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/items/weapons/rifle.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/tutorial.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/tutorial.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>() == "/assets/models/environment/props/gameplay/leaderboard.gr2")
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/gameplay/leaderboard.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>().StartsWith("/assets/models/environment/props/landscapes/lowpoly/"))
                    {
                        var name = mr["model"]["resource_id"].Value<string>().Replace(".gr2", ".prefab");
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"{name}\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, life, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>().StartsWith("/assets/models/environment/props/buildings/"))
                    {
                        var name = mr["model"]["resource_id"].Value<string>().Replace(".gr2", ".prefab");
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"{name}\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, life, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>().StartsWith("/assets/models/environment/props/nature/"))
                    {
                        var name = mr["model"]["resource_id"].Value<string>().Replace(".gr2", ".prefab");
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"{name}\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, life, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>().StartsWith("/assets/models/environment/props/fx/"))
                    {
                        var name = mr["model"]["resource_id"].Value<string>().Replace(".gr2", ".prefab");
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"{name}\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, life, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>().StartsWith("/assets/models/environment/props/test/render/"))
                    {
                        var name = mr["model"]["resource_id"].Value<string>().Replace(".gr2", ".prefab");
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"{name}\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, life, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>().StartsWith("/assets/models/characters/npc/"))
                    {
                        var name = mr["model"]["resource_id"].Value<string>().Replace(".gr2", ".prefab");
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"{name}\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, life, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>().StartsWith("/assets/models/characters/player/test"))
                    {
                        var name = mr["model"]["resource_id"].Value<string>().Replace(".gr2", ".prefab");
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"{name}\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, life, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>().StartsWith("/assets/models/environment/props/primitives/"))
                    {
                        var name = mr["model"]["resource_id"].Value<string>().Replace(".gr2", ".prefab");
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"{name}\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, life, name, other\"}}"));
                        continue;
                    }

                    else if (mr != null && mr["model"]["resource_id"].Value<string>().StartsWith("/assets/models/environment/props/bitgem/"))
                    {
                        var name = mr["model"]["resource_id"].Value<string>().Replace(".gr2", ".prefab");
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"{name}\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, life, name, other\"}}"));
                        continue;
                    }
                    else if (mr != null && mr["model"]["resource_id"].Value<string>().StartsWith("/assets/models/_thirdparty/synty_studios/basic_dungeon/"))
                    {
                        var name = mr["model"]["resource_id"].Value<string>().Replace(".gr2", ".prefab");
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"{name}\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, life, name, other\"}}"));
                        continue;
                    }

                    else if (mr != null)
                    {
                        var name = mr["model"]["resource_id"].Value<string>().Replace(".gr2", ".prefab");
                        name = "/missing" + name;
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"{name}\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, life, name, other\"}}"));
                        continue;
                    }


                    var pr = FindComponentById(content, "primitiveRender", id);
                    if (pr != null)
                    {
                        prefabDescs.Add(JObject.Parse($"{{\"id\":{id},\"prefabName\":\"/assets/models/environment/props/primitives/prim_box.prefab\",\"partName\":\"part_0\",\"userName\":\"\",\"ui_filter\":\"power, name, life, other, \"}}"));
                        continue;
                    }

                }
                catch (Exception) { }

                throw new ConversionError($"Could not resolve prefab for go {id}");
            }

            return (changed ? ConversionResult.Modified : ConversionResult.NoChange, content);
        }
    }
}