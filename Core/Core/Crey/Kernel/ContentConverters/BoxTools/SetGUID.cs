using Newtonsoft.Json.Linq;

namespace Crey.Kernel.ContentConverter.BoxTools
{
    public class SetGUID : BoxToolBase
    {
        private string _name;

        public SetGUID(string name)
        {
            _name = name;
        }

        public override (ConversionResult, JObject) Convert(JObject content)
        {
            var box = FindRootBox(content);
            if (box == null) throw new ConversionError("Not a box");

            box["GUID"] = _name;
            return (ConversionResult.Modified, content);
        }
    }
}