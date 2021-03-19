using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Crey.Utils;
using System;
using System.Threading.Tasks;

namespace Core.Azure
{
    public class BlobContainer
    {
        public static BlobServiceClient CreateClient(string connectionString, System.Net.Http.IHttpClientFactory http = null)
        {
            var clientOptions = new BlobClientOptions
            {
                // notes: no options for this in new API,  there is GeoRedundantSecondaryUri, but we do not have second URI
                // LocationMode = LocationMode.PrimaryThenSecondary,
                // notes: next hash check is not recreated, either is is buildint or should be done manually
                // DisableContentMD5Validation = false, // controls whether or not the Storage Client will validate that MD5 hash on download
                Diagnostics = { IsDistributedTracingEnabled = true, IsLoggingEnabled = true },
                Retry = {
                    NetworkTimeout = TimeSpan.FromSeconds(120),
                    Delay = TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(20),
                    Mode = RetryMode.Exponential
                },
            };

            if (http != null)
            {
                var client = http.CreateClient("Blob");
                client.Timeout = TimeSpan.FromSeconds(720);
                clientOptions.Transport = new HttpClientTransport(client);
            }

            // is on per HTTP call basis, not per blob
            // clientOptions.AddPolicy(new HttpPipelinePolicy(), HttpPipelinePosition.PerCall)
            return new BlobServiceClient(connectionString, clientOptions);
        }

        private StorageSharedKeyCredential key_;

        public BlobContainerClient CloudContainer { get; }

        internal BlobContainer(BlobServiceClient blobClient, string containerName, StorageSharedKeyCredential key)
        {
            key_ = key;
            CloudContainer = blobClient.GetBlobContainerClient(containerName);
        }

        internal BlobContainer(BlobServiceClient blobClient, string containerName, bool ensureExists, StorageSharedKeyCredential key) : this(blobClient, containerName, key)
        {
            var exists = CloudContainer.ExistsAsync().Result;
            if (!exists)
            {
                if (ensureExists)
                {
                    CloudContainer.CreateIfNotExistsAsync().Wait();
                }
                else
                {
                    throw new Exception($"container does not exist and creation not allowed. name:[{containerName}]");
                }
            }
        }

        public async Task<bool> ExistsBlobAsync(string name)
        {
            return await CloudContainer.ExistsBlobAsync(name);
        }

        public async Task DeleteDirectoryAsync(string directoryName)
        {
            await CloudContainer.DeleteDirectoriesBlobsAsync(directoryName);
        }

        public async Task DeleteDirectoryAsnc(string directoryName)
        {
            await CloudContainer.DeleteDirectoriesBlobsAsync(directoryName);
        }

        public async Task<BlobContentInfo> UploadAsync(string blockName, byte[] data)
        {
            return await CloudContainer.UploadAsync(blockName, data);
        }

        public async Task<BlobContentInfo> CopyBlobAsync(string oldName, string newName)
        {
            var source = CloudContainer.GetBlobClient(oldName);
            var target = CloudContainer.GetBlobClient(newName);
            if (await target.ExistsAsync() || !await source.ExistsAsync()) return null;
            var data = await source.OpenReadAsync();
            return (await target.UploadAsync(data)).Value;
        }

        public bool RenameBlob(string oldName, string newName)
        {
            var blob = CopyBlobAsync(oldName, newName).Result;
            if (blob == null)
                return false;
            return CloudContainer.GetBlobClient(oldName).DeleteIfExistsAsync().Result;
        }


        public async Task<BlobContentInfo> UploadAsync(string blockName, byte[] data, int length, int startIndex = 0)
        {
            return await CloudContainer.UploadAsync(blockName, new ReadOnlyMemory<byte>(data, startIndex, length));
        }

        public async Task<BlobContentInfo> UploadAsync(string blockName, ReadOnlyMemory<byte> data)
        {
            return await CloudContainer.UploadAsync(blockName, data);
        }

        public async Task DeleteByPrefixAsync(string fileprefix)
        {
            await CloudContainer.DeleteBlobsAsync(fileprefix);
        }

        public async Task<byte[]> DownloadAsync(string blockName)
        {
            return await CloudContainer.DownloadBytesAsync(blockName);
        }

        public async Task DeleteAsync(string blockName)
        {
            await CloudContainer.DeleteBlobAsync(blockName);
        }

        public async Task MoveAsync(string sourceName, string targetName)
        {
            var source = CloudContainer.GetBlobClient(sourceName);
            var target = CloudContainer.GetBlobClient(targetName);
            var data = await source.OpenReadAsync();
            await target.UploadAsync(data);
            await source.DeleteAsync();
        }

        public string GetSASUri(bool write, int durationHours)
        {
            return BlobExtensions.GetContainerSas(CloudContainer.Name, key_, write ? BlobExtensions.ContainerReadWriteList : BlobContainerSasPermissions.Read, (uint)durationHours * 60).ToString();
        }
    }
}