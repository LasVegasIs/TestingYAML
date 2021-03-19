using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Xunit
{
    public class AssertJSON : Xunit.Assert
    {
        public static void EqualJSON(JObject a, JObject b)
        {
            var ja = Newtonsoft.Json.JsonConvert.SerializeObject(a, Newtonsoft.Json.Formatting.None);
            var jb = Newtonsoft.Json.JsonConvert.SerializeObject(b, Newtonsoft.Json.Formatting.None);
            Assert.Equal(ja, jb);
        }
    }
}
