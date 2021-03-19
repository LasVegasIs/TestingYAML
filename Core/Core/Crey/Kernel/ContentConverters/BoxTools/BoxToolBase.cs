using Newtonsoft.Json.Linq;

namespace Crey.Kernel.ContentConverter.BoxTools
{
    public abstract class BoxToolBase : LevelConverterBase
    {
        protected JObject FindRootBox(JObject content)
        {
            var rootGroup = content["root"];
            if (rootGroup == null) throw new ConversionError("Missing root");
            var rootIdNode = rootGroup["id"];
            if (rootIdNode == null) throw new ConversionError("Missing root id");
            var rootId = rootIdNode.ToObject<int>();

            var dispatchStateGroup = content["dispatchState"];
            if (rootGroup == null) throw new ConversionError("Missing dispatchState");
            foreach (var inst in dispatchStateGroup["instances"])
            {
                if (inst["id"].ToObject<int>() == rootId)
                    return (JObject)inst;
            }
            return null;
        }
    }
}