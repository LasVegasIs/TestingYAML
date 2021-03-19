using Azure.Storage.Blobs.Models;
using Core.Azure;
using Core.Functional;
using Crey.Contracts;
using Crey.Exceptions;
using Crey.Utils;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading.Tasks;

namespace Crey.Extensions.CreyAzure
{
    public static class CreyAzureExtensions
    {
        [Obsolete("Result type is obsolate, use DownloadBinaryAsync2 intead")]
        public static async Task<Result<BinaryContent, Error>> DownloadBinaryAsync(this BlobContainer container, string blockName, string etag, string mimeType)
        {
            var content = await container.DownloadBinaryAsync2(blockName, etag, mimeType);
            if (content == null)
            {
                return ErrorCodes.ItemNotFound.IntoError($"No resource found at path: {blockName}");
            }

            return content;
        }

        public static async Task<BinaryContent> DownloadBinaryAsync2(this BlobContainer container, string blockName, string etag, string mimeType)
        {
            var blob = container.CloudContainer.GetBlobClient(blockName);
            if (!await blob.ExistsAsync())
                return null;

            var props = (await blob.GetPropertiesAsync()).Value;
            if (etag != null && props.GetContentHash() == etag)
            {
                // return ok with no data - this means the client should use the cached value
                return new BinaryContent { Data = null, ContentHash = etag, MimeType = mimeType };
            }

            var data = await container.CloudContainer.DownloadBytesAsync(blockName);
            return new BinaryContent { Data = data, ContentHash = props.GetContentHash(), MimeType = mimeType };
        }

        [Obsolete("Result type is obsolate, use UploadAsync instead")]
        public static async Task<Result<BlobContentInfo, Error>> UploadBinaryAsync(this BlobContainer container, string blockName, DataSpan data)
        {
            try
            {
                return await container.CloudContainer.UploadAsync(blockName, data);
            }
            catch (Exception ex)
            {
                return ErrorCodes.ServerError.IntoError($"Could not save {blockName}: {ex}");
            }
        }

        public static async Task<string> GetBlobETag(this BlobContainer container, string blockName)
        {
            return (await container.CloudContainer.GetBlobPropsAsync(blockName)).GetContentHash();
        }
    }
}
