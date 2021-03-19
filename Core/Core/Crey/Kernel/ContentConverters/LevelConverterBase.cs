using Crey.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Crey.Kernel.ContentConverter
{
    public enum ConversionResult
    {
        Modified,
        NoChange,
    }

    public class ConversionError : Exception
    {
        public ConversionError(string error) : base(error) { }
    }

    public abstract class LevelConverterBase
    {
        public static JObject IntoJObject(DataSpan input)
        {
            var mem = new MemoryStream(input.Buffer, input.Start, input.Length, false);
            var intput = new StreamReader(mem);
            return IntoJObject(intput);
        }

        public static JObject IntoJObject(StreamReader input)
        {
            var reader = new JsonTextReader(input) { FloatParseHandling = FloatParseHandling.Decimal };
            var content = JObject.Load(reader);
            return content;
        }

        public static DataSpan FromJObject(JObject content)
        {
            var mem = new MemoryStream();
            FromJObject(content, new StreamWriter(mem));
            return new DataSpan(mem.GetBuffer(), 0, (int)mem.Length);
        }

        public static void FromJObject(JObject content, StreamWriter output)
        {
            var writer = new JsonTextWriter(output);
            var converters = new JsonConverter[] { new CrayJsonDoubleConverter() };
            content.WriteTo(writer, converters);
            writer.Flush();
        }

        public ConversionResult Process(StreamReader input, StreamWriter output)
        {
            var content = IntoJObject(input);
            var (result, newContent) = Convert(content);
            if (newContent != null && output != null && result == ConversionResult.Modified)
            {
                FromJObject(newContent, output);
            }
            return result;
        }

        public virtual (ConversionResult, JObject) Convert(JObject content)
        {
            return (ConversionResult.NoChange, content);
        }

        #region Utility functions
        protected int GetLevelVersion(JObject content)
        {
            var streamInfo = content["streamInfo"];
            if (streamInfo == null) throw new ConversionError($"Missing streamInfo");
            var dataVersion = streamInfo["dataVersion"];
            if (dataVersion == null) throw new ConversionError($"Missing dataVersion");

            return dataVersion.ToObject<int>();
        }

        protected bool CheckLevelVersion(JObject content, int expected)
        {
            var version = GetLevelVersion(content);
            if (version < expected)
                throw new ConversionError($"Version missmatch. Expected: {expected}, found: {version}");
            return version == expected;
        }

        protected bool IncrementLevelVersion(JObject content, int expected)
        {
            if (!CheckLevelVersion(content, expected))
                return false;

            content["streamInfo"]["dataVersion"] = expected + 1;
            return true;
        }

        protected JObject FindComponentById(JObject content, string componentName, int id)
        {
            var component = content[componentName];
            if (component == null || component.Type != JTokenType.Object)
                return null;

            var inst = component["instances"];
            if (inst == null || inst.Type != JTokenType.Array)
                return null;

            try
            {
                foreach (var v in inst)
                {
                    if (v.Type != JTokenType.Object)
                        return null;

                    if (v["id"].Value<int>() == id)
                        return (JObject)v;
                }
            }
            catch (Exception) { }

            return null;
        }


        protected bool IsResourceRefNode(JToken node)
        {
            if (node.Type != JTokenType.Object)
                return false;

            var obj = (JObject)node;
            if (obj["resource_type"] == null)
                return false;
            if (obj["resource_id"] == null)
                return false;

            return true;
        }

        protected bool IsPrefabDescNode(JToken node)
        {
            if (node.Type != JTokenType.Object)
                return false;

            var obj = (JObject)node;
            if (obj["prefabName"] == null)
                return false;
            if (obj["partName"] == null)
                return false;
            if (obj["userName"] == null)
                return false;

            return true;
        }

        protected IEnumerable<string> AllPrefabsByPrefabDesc(JObject content)
        {
            var go = content["_go"];
            if (go == null)
                throw new ConversionError("Missing game objects");
            var goInstances = go["instances"].Children();
            if (!goInstances.Any())
                yield break;

            var prefab = content["PrefabDesc"];
            if (prefab == null)
                throw new ConversionError("Missing prefab description");

            var idToPrefab = new Dictionary<int, string>();
            foreach (var instance in prefab["instances"].Children())
            {
                var prefabId = instance["prefabName"].Value<string>();
                idToPrefab.Add(instance["id"].Value<int>(), prefabId);
            }

            foreach (var goInst in goInstances)
            {
                var id = goInst["id"].Value<int>();
                string prefabId = null;
                if (!idToPrefab.TryGetValue(id, out prefabId) || prefabId == null || prefabId == "")
                    throw new ConversionError($"Missing prefab description for game object {id}");
                if (prefabId.StartsWith('/')) prefabId = prefabId.Substring(1, prefabId.Length - 1);
                yield return prefabId;
            }
        }

        protected bool IsResourceNode(JToken node)
        {
            return IsPrefabDescNode(node) || IsResourceRefNode(node);
        }

        /// <summary>
        ///     Returns all the "known" resource reference (including prefab desc) nodes
        /// </summary>
        /// <param name="root">The root node to start from</param>
        /// <returns></returns>
        protected IEnumerable<JObject> AllResources(JObject root)
        {
            var toSearch = new Stack<JToken>(root.Children());
            while (toSearch.Count > 0)
            {
                var inspected = toSearch.Pop();
                if (IsResourceNode(inspected))
                    yield return (JObject)inspected;
                else
                    foreach (var child in inspected)
                        toSearch.Push(child);
            }
        }

        /// <summary>
        ///     Returns all the resource references (excluding prefab desc)
        /// </summary>
        /// <param name="root">The root node to start from</param>
        /// <returns></returns>
        protected IEnumerable<JObject> AllResourceRef(JObject root)
        {
            var toSearch = new Stack<JToken>(root.Children());
            while (toSearch.Count > 0)
            {
                var inspected = toSearch.Pop();
                if (IsResourceRefNode(inspected))
                    yield return (JObject)inspected;
                else
                    foreach (var child in inspected)
                        toSearch.Push(child);
            }
        }
        #endregion
    }
}