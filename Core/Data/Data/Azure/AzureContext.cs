using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Crey.Instrumentation.Configuration;

namespace Crey.Data.Azure
{
    public /*abstract*/ class AzureContextOptions
    {
        public string Service { get; set; }

        public string Stage { get; set; }

        private string storageAccountCns_;
        public string StorageAccountCns
        {
            get
            {
                return storageAccountCns_;
            }

            set
            {
                storageAccountCns_ = value;
                SetConnectionString(value);
            }
        }

        public Microsoft.Azure.Cosmos.Table.CloudStorageAccount CosmosStorageAccount { get; private set; }

        internal StorageSharedKeyCredential StorageCredentials { get; private set; }
        public string TableNamePattern { get; set; } = "${stage}${Service}${Name}";
        public string BlobContainerNamePattern { get; set; } = "${stage}${-service}${-name}";

        public string GetTableName(string name)
        {
            return SubstituteCreyStagePattern(TableNamePattern, Stage, Service, name);
        }

        public string GetBlobContainer(string name)
        {
            return SubstituteCreyStagePattern(BlobContainerNamePattern, Stage, Service, name);
        }

        private static string SubstituteCreyStagePattern(string pattern, string inputStage, string inputService, string inputName)
        {
            return pattern
                .SubstituteStagePattern(inputStage)
                .SubstituteServicePattern(inputService)
                .SubstituteNamePattern(inputName);
        }

        private void SetConnectionString(string connection)
        {
            if (string.IsNullOrWhiteSpace(connection))
            {
                throw new Exception("Azure connection cannot be null or empty");
            }

            CosmosStorageAccount = Microsoft.Azure.Cosmos.Table.CloudStorageAccount.Parse(connection);
            StorageCredentials = (StorageSharedKeyCredential)StorageConnectionString.Parse(connection).Credentials;
        }

        public bool UsesBlob { get; set; }
        public bool UsesTable { get; set; }
    }

    public class AzureContextOptions<T> : AzureContextOptions
        where T : AzureContext
    {
    }


    public class AzureContext
    {
        public class ConnectionBuilder
        {
            internal AzureContextOptions options_;
            internal BlobServiceClient blobClient_;
            internal StrongCloudTableClient cosmosTableClient_;


            public CosmosTableStorage CreateCosmosTableStorage(string name)
            {
                return new CosmosTableStorage(cosmosTableClient_, options_.GetTableName(name));
            }


            public CosmosTableStorage CreateCosmosTableStorageOnDemand(string name)
            {
                return new CosmosTableStorage(cosmosTableClient_, options_.GetTableName(name), true);
            }

            public CosmosTypedTableStorage<Entry> CreateCosmosTypedTableStorage<Entry>()
                where Entry : Microsoft.Azure.Cosmos.Table.TableEntity, new()
            {
                return new CosmosTypedTableStorage<Entry>(cosmosTableClient_, options_);
            }

            public CosmosTypedTableStorage<Entry> CreateCosmosTypedTableStorage<Entry>(string name)
                where Entry : Microsoft.Azure.Cosmos.Table.TableEntity, new()
            {
                return new CosmosTypedTableStorage<Entry>(cosmosTableClient_, options_, name);
            }


            public BlobContainer CreateBlobContainer(string name)
            {
                return new BlobContainer(blobClient_, options_.GetBlobContainer(name), options_.StorageCredentials);
            }

            public BlobContainer CreateBlobContainerOnDemand(string name)
            {
                return new BlobContainer(blobClient_, options_.GetBlobContainer(name), true, options_.StorageCredentials);
            }
        }

        public AzureContextOptions Options { get; }

        private ConnectionBuilder connections_;
        public ConnectionBuilder Connections => LazyInitializer.EnsureInitialized(ref connections_, Initialize);
        public BlobServiceClient BlobClient => Connections.blobClient_;

        public StrongCloudTableClient CosmosTableClient => Connections.cosmosTableClient_;

        public AzureContext(AzureContextOptions options)
        {
            Options = options;
        }

        public bool EnsureInitialized()
        {
            var b = Connections != null;
            Debug.Assert(b);
            return b;
        }

        protected virtual void OnConfiguring(ConnectionBuilder connections)
        {
            connections.blobClient_ = BlobContainer.CreateClient(Options.StorageAccountCns);
            connections.cosmosTableClient_ = TableStorageHelpers.CreateClient(Options.CosmosStorageAccount);
        }

        protected virtual void OnModelCreating(ConnectionBuilder connections)
        {
            // it's up to the client
        }

        private ConnectionBuilder Initialize()
        {
            var connections = new ConnectionBuilder() { options_ = Options };
            OnConfiguring(connections);
            OnModelCreating(connections);
            return connections;
        }

        public CosmosMigrationTableStorage CreateMigrationStorage<T>(string name)
            where T : class
        {
            return new CosmosMigrationTableStorage(CosmosTableClient, Options, name ?? typeof(T).Name);
        }
    }

    public static class AzureContextExtensions
    {
        public static IServiceCollection AddAzureContext<AZURE>(this IServiceCollection collectionBuilder, Action<AzureContextOptions<AZURE>> optionBuilder)
            where AZURE : AzureContext
        {
            collectionBuilder.Configure<AzureContextOptions<AZURE>>(options =>
            {
                optionBuilder(options);
                Debug.Assert(!string.IsNullOrEmpty(options.Stage));
                Debug.Assert(!string.IsNullOrEmpty(options.Service));
                if (options.UsesBlob)
                {
                    collectionBuilder.AddHealthChecks().AddAzureBlobStorage(options.StorageAccountCns, tags: new[] { "storages" });
                }

                if (options.UsesTable)
                {
                    // it requires table name to be ON, so register must pass names here
                    //collectionBuilder.AddHealthChecks().AddAzureTable(options.StorageAccountCns, tableName, tags: new []{"storages"});
                }
            });

            collectionBuilder.TryAddSingleton(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AzureContextOptions<AZURE>>>();
                var context = (AZURE)Activator.CreateInstance(typeof(AZURE), options.Value); // required as ctor requires parameters
                context.EnsureInitialized();

                return context;
            });

            return collectionBuilder;
        }

        public static T ConvertBackBase<T>(this Microsoft.Azure.Cosmos.Table.DynamicTableEntity entry, Type type)
        {
            MethodInfo method = typeof(Microsoft.Azure.Cosmos.Table.TableEntity).GetMethod("ConvertBack");
            MethodInfo generic = method.MakeGenericMethod(type);

            var returnValue = generic.Invoke(entry, new object[] { new OperationContext() });
            return (T)returnValue;
        }
    }
}
