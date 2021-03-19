#nullable enable

using Newtonsoft.Json;
using System.Text;
// https://stackoverflow.com/questions/58138765/compositetoken-in-new-microsoft-azure-cosmos-sdk
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Cosmos.Table;

namespace Crey.Data.Azure
{
    public class TableBasedContinuationToken
    {

        public TableContinuationToken? TableCursor { get; set; }

        public TableBasedContinuationToken(string? token)
        {
            if (token != null)
            {
                var decoded = WebEncoders.Base64UrlDecode(token);
                var json = Encoding.UTF8.GetString(decoded);
                TableCursor = JsonConvert.DeserializeObject<Microsoft.Azure.Cosmos.Table.TableContinuationToken>(json);
            }
        }

        public TableBasedContinuationToken()
        {
        }

        public override string? ToString()
        {
            if (TableCursor != null)
            {
                var json = JsonConvert.SerializeObject(TableCursor);
                var bytes = Encoding.UTF8.GetBytes(json);
                return WebEncoders.Base64UrlEncode(bytes);
            }

            return null;
        }

        public bool IsContinuation => TableCursor != null && (!string.IsNullOrEmpty(TableCursor.NextPartitionKey) || !string.IsNullOrEmpty(TableCursor.NextRowKey));
    }
}