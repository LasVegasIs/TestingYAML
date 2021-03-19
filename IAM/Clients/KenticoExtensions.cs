using Crey.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace IAM.Clients
{
    public static class KenticoExtensions
    {
        
        public static async Task<string> GetPrivacyPolicy(this KenticoClient kenticoClient_)
        {
            var content = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("elements.contentid", "content-privacy-policy")
            };

            try
            {
                JObject result = await kenticoClient_.Get("items", content);

                JArray items = (JArray)result["items"];
                JToken privacyPolicy = items[0];
                JToken privacyPolicyElements = privacyPolicy["elements"];

                return privacyPolicyElements["body"]["value"].Value<string>();
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> GetTermsAndConditions(this KenticoClient kenticoClient_)
        {
            var content = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("elements.contentid", "content-terms-and-conditions")
            };

            try
            {
                JObject result = await kenticoClient_.Get("items", content);

                JArray items = (JArray)result["items"];
                JToken privacyPolicy = items[0];
                JToken privacyPolicyElements = privacyPolicy["elements"];

                return privacyPolicyElements["body"]["value"].Value<string>();
            }
            catch
            {
                return null;
            }
        }



    }
}
