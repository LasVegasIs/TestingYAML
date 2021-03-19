using Newtonsoft.Json.Linq;

namespace Crey.Kernel.ContentConverter.Migration
{
    public class LevelMigrate_8_PhxSensor : LevelConverterBase
    {
        private JObject ConvertProximityInstance(JObject prox, int version)
        {
            if (version > 5) throw new ConversionError($"powerProximity has a version higher than 5. Current version is {version}.");

            if (version == 5) throw new ConversionError($"powerProximity doesn't need conversion, its version is 5");

            var ret = new JObject();

            // id
            ret["id"] = prox["id"].DeepClone();

            // shape
            if (version < 2)
            {
                ret["shape"] = 3;
            }
            else if (version == 2)
            {
                var oldShape = (EOldShape)prox["shape_"].Value<int>(); // !!!: shape_
                var newShape = ENewShape.Sphere;
                switch (oldShape)
                {
                    case EOldShape.Box:
                        newShape = ENewShape.Box;
                        break;
                        ;
                    case EOldShape.Cylinder:
                        newShape = ENewShape.Cylinder;
                        break;
                    case EOldShape.Sphere:
                        newShape = ENewShape.Sphere;
                        break;
                }

                ret["shape"] = (int)newShape;
            }
            else
            {
                ret["shape"] = prox["shape"].DeepClone();
            }

            prox.Remove("shape");

            // extents
            ret["extents"] = prox["extents"].DeepClone();
            prox.Remove("extents");

            // filter type
            if (version == 0)
                ret["filterType"] = (int)EFilter.Filter_AnyPlayer;
            else
                ret["filterType"] = prox["filterType"].DeepClone();
            prox.Remove("filterType");

            // useOwnShape
            ret["useOwnShape"] = false;

            // triggerOnce - this attribute stays in the proxy object
            if (version <= 3)
            {
                var triggerOnce = prox["triggerOnce"].Value<bool>();
                var triggerType = triggerOnce ? ETrigger.TriggerOnce : ETrigger.TriggerAlways;
                prox["triggerOnce"] = (int)triggerType;
            }

            return ret;
        }

        private JObject ConvertContactDamage(JObject contact, int version)
        {
            var ret = new JObject();

            if (version < 2) throw new ConversionError($"Contact Damage component conversion is only supported after version 2. Current Version is {version}.");

            ret["id"] = contact["id"].DeepClone();

            // create default instance with dummy data
            // we don't need the data since the actual shape will be the mesh

            ret["shape"] = 3;
            ret["extents"] = JObject.Parse(@"{""x"": 3.0,""y"": 3.0,""z"": 3.0}");
            ret["filterType"] = (int)EFilter.Filter_AnyProp;
            ret["useOwnShape"] = true;

            return ret;
        }

        private JObject ConvertAreaDamage(JObject area, int version)
        {
            var ret = new JObject();
            if (version > 4) throw new ConversionError($"areaDamage has a version higher than 4. Current version is {version}.");

            if (version == 4) throw new ConversionError($"areaDamage doesn't need conversion, its version is 4");

            ret["id"] = area["id"].DeepClone();

            ret["shape"] = area["shape"].DeepClone();
            area.Remove("shape");
            ret["extents"] = area["extent"].DeepClone();
            area.Remove("extent");

            ret["filterType"] = (int)EFilter.Filter_AnyPropOrPlayer;

            ret["useOwnShape"] = false;

            // instantEffect
            if (version < 2) area["instantEffect"] = false;

            // damage
            if (version < 2)
            {
                var isHeal = true;
                var damage = area["damage"].Value<float>();
                if (damage < 0)
                {
                    isHeal = false;
                    damage = -damage;
                }

                area["damage"] = damage;
                area["isHeal"] = isHeal;
            }

            // suspendTime
            if (version < 1) area["suspendTime"] = 3.0f;

            // mask
            area.Remove("mask");

            return ret;
        }

        public override (ConversionResult, JObject) Convert(JObject content)
        {
            if (!IncrementLevelVersion(content, 8))
            {
                return (ConversionResult.NoChange, content);
            }

            JToken firstRelevantObject = null;
            var sensorInstances = new JArray();

            var powerProximity = content["powerProximity"];
            if (powerProximity != null)
            {
                var version = powerProximity["dataVersion"].Value<int>();
                if (version < 5)
                {
                    if (firstRelevantObject == null) firstRelevantObject = powerProximity;

                    powerProximity["dataVersion"] = 5;
                    foreach (var instance in powerProximity["instances"].Children()) sensorInstances.Add(ConvertProximityInstance((JObject)instance, version));
                }
            }

            var contactDamage = content["contactDamage"];
            if (contactDamage != null)
            {
                var version = contactDamage["dataVersion"].Value<int>();
                if (version < 2)
                {
                    content.Remove("contactDamage");
                }
                else if (version < 3)
                {
                    if (firstRelevantObject == null) firstRelevantObject = contactDamage;

                    contactDamage["dataVersion"] = 3;
                    foreach (var instance in contactDamage["instances"].Children()) sensorInstances.Add(ConvertContactDamage((JObject)instance, version));
                }
            }

            var areaDamage = content["areaDamage"];
            if (areaDamage != null)
            {
                var version = areaDamage["dataVersion"].Value<int>();
                if (version < 4)
                {
                    if (firstRelevantObject == null) firstRelevantObject = areaDamage;

                    areaDamage["dataVersion"] = 4;
                    foreach (var instance in areaDamage["instances"].Children()) sensorInstances.Add(ConvertAreaDamage((JObject)instance, version));
                }
            }

            if (firstRelevantObject != null)
            {
                var sensor = content["phxSensor"];
                if (sensor == null)
                {
                    sensor = new JObject();
                    sensor["dataVersion"] = 0;
                    sensor["instances"] = new JArray();

                    var sensorProperty = new JProperty("phxSensor", sensor);
                    firstRelevantObject.Parent.AddBeforeSelf(sensorProperty);
                }

                ((JArray)sensor["instances"]).Merge(sensorInstances);
            }

            return (ConversionResult.Modified, content);
        }

        private enum EOldShape
        {
            Box = 0,
            Cylinder = 1,
            Sphere = 2
        }

        private enum ENewShape
        {
            Box = 2,
            Sphere = 3,
            Cylinder = 5
        }

        private enum EFilter
        {
            Filter_AnyPlayer,
            Filter_AnyProp,
            Filter_LinkedProp,
            Filter_Player0,
            Filter_Player1,
            Filter_AnyPropOrPlayer
        }

        private enum ETrigger
        {
            TriggerOnce,
            TriggerAlways,
            TriggerOverridden
        }
    }
}