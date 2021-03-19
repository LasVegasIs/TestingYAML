using Crey.Misc;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Crey.Test
{
    public class CryptoTest
    {
        private readonly ITestOutputHelper output;

        public CryptoTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestMurmurHash()
        {
            var tests = new Dictionary<string, string>{
                { "done\n1525955463", "15766391286959756134" },
            };

            foreach (KeyValuePair<string, string> t in tests)
            {
                var data = Encoding.ASCII.GetBytes(t.Key);
                var hash = CryptoHelper.CalculateMurmurHash(data);
                Assert.Equal(t.Value, hash);
            }
        }
    }
}
