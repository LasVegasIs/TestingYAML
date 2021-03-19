#nullable enable

using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using System;
using System.IO;
using System.Threading.Tasks;
using Crey.Utils;

namespace Crey.Data.Azure
{
    /// <summary>
    /// Is <see cref="HashMatched"/> or <see cref="BlobContent"/>.
    /// </summary>
    public abstract class DownloadResult
    {
        protected internal DownloadResult() { }
    }
    public sealed class HashMatched : DownloadResult
    {
    }
    public sealed class BlobContent : DownloadResult
    {
        public byte[] Data { get; set; } = null!;
        public string ContentHash { get; set; } = null!;
    }

    public static class BlobExtensions
    {

        public static BlobContainerSasPermissions ContainerReadWriteList = BlobContainerSasPermissions.List | BlobContainerSasPermissions.Read | BlobContainerSasPermissions.Write;

        public static async Task DeleteDirectoriesBlobsAsync(this BlobContainerClient self, string prefix)
        {
            var blobs = self.GetBlobsByHierarchyAsync(prefix: prefix);
            await foreach (var b in blobs)
                await self.GetBlobClient(b.Prefix).DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        public static async Task DeleteBlobsAsync(this BlobContainerClient self, string prefix)
        {
            var blobs = self.GetBlobsAsync(prefix: prefix);
            await foreach (var b in blobs)
                await self.GetBlobClient(b.Name).DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        public static async Task<BlobContentInfo> UploadAsync(this BlobContainerClient self, string blobName, ReadOnlyMemory<byte> data)
        {
            if (data.IsEmpty) throw new ArgumentException("data is empty");
            return (await self.GetBlobClient(blobName).UploadAsync(new MemoryStream(data.ToArray()), overwrite: true)).Value;
        }

        public static bool IsFolder(this BlobItem self)
        {
            return self.Name.EndsWith("/");
        }

        public static async Task<Stream> DownloadStreamAsync(this BlobContainerClient self, string blobName)
        {
            return (await self.GetBlobClient(blobName).DownloadAsync()).Value.Content;
        }

        public static Task DeleteAsync(this BlobContainerClient self, string blobName)
        {
            return self.GetBlobClient(blobName).DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        public static async Task<Uri> GetUserDelegationSasBlobAsync(this BlobServiceClient self, string containerName, string blobName, BlobSasPermissions permissions = BlobSasPermissions.Read, uint durationMinutes = 10)
        {
            var key = await self.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddDays(6)); // max is 7 days
            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(durationMinutes)
            };
            sasBuilder.SetPermissions(permissions);
            var sasToken = sasBuilder.ToSasQueryParameters(key, self.AccountName).ToString();
            var fullUri = new UriBuilder()
            {
                Scheme = "https",
                Host = $"{self.AccountName}.blob.core.windows.net",
                Path = $"{containerName}/{blobName}",
                Query = sasToken
            };
            return fullUri.Uri;
        }

        public static Uri GetContainerSas(string containerName, StorageSharedKeyCredential key, BlobContainerSasPermissions permissions = BlobContainerSasPermissions.Read, uint durationMinutes = 10)
        {
            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerName,
                Resource = "c",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(durationMinutes)
            };
            sasBuilder.SetPermissions(permissions);
            var sasToken = sasBuilder.ToSasQueryParameters(key).ToString();
            var fullUri = new UriBuilder()
            {
                Scheme = "https",
                Host = $"{key.AccountName}.blob.core.windows.net",
                Path = $"{containerName}",
                Query = sasToken
            };
            return fullUri.Uri;
        }

        public static async Task<bool> ExistsBlobAsync(this BlobContainerClient self, string blobName)
        {
            return (await self.GetBlobClient(blobName).ExistsAsync()).Value;
        }

        public static string GetETagString(this BlobProperties self)
        {
            return self.ETag.ToString().Trim('"');
        }

        public static string GetETagString(this BlobContentInfo self)
        {
            return self.ETag.ToString().Trim('"');
        }

        public static string GetContentHash(this BlobProperties self)
        {
            return Convert.ToBase64String(self.ContentHash);
        }

        public static string GetContentHash(this BlobContentInfo self)
        {
            return Convert.ToBase64String(self.ContentHash);
        }

        public static string GetContentHash(this BlobItemProperties self)
        {
            return Convert.ToBase64String(self.ContentHash);
        }

        public static Uri GetBlobContainerUri(this BlobServiceClient self, string containerName)
        {
            return self.GetBlobContainerClient(containerName).Uri;
        }

        public static async Task<BlobProperties?> GetBlobPropsAsync(this BlobContainerClient self, string blobName)
        {
            var blob = self.GetBlobClient(blobName);
            return (await blob.ExistsAsync()).Value switch
            {
                true => (await blob.GetPropertiesAsync())?.Value,
                false => null
            };
        }

        public static async Task<string?> GetContentHashAsync(this BlobContainerClient self, string blobName)
        {
            return (await self.GetBlobPropsAsync(blobName))?.GetETagString();
        }

        public static async Task<DownloadResult?> DownloadIfChangedAsync(this BlobContainerClient container, string blobName, string? etag)
        {

            var props = await container.GetBlobPropsAsync(blobName);
            if (props == null) return null;

            if (etag != null && props.GetETagString() == etag)
                return new HashMatched();
            var data = new byte[props.ContentLength];
            var mem = new MemoryStream(data);
            var reader = container.GetBlockBlobClient(blobName);
            await reader.DownloadToAsync(mem);
            return new BlobContent { Data = data, ContentHash = props.GetETagString() };
        }

        public static async Task<byte[]?> DownloadBytesAsync(this BlobContainerClient container, string blobName)
        {
            var props = await container.GetBlobPropsAsync(blobName);
            if (props == null) return null;
            var data = new byte[props.ContentLength];
            var mem = new MemoryStream(data);
            var reader = container.GetBlockBlobClient(blobName);
            await reader.DownloadToAsync(mem);
            return data;
        }

        public static async Task<BinaryContent?> DownloadBinaryAsync(this BlobContainer container, string blockName, string etag, string mimeType)
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
    }
}
