using Core.Extensions.ThrowIf;
using MongoDB.Driver;
using System;
using System.Security.Authentication;

namespace Core.Azure
{
    public class MongoConnection
    {
        // todo: make it part of appsettings.json
        public class Credentials
        {
            public string Host { get; set; } = "kozmos.documents.azure.com";
            public int Port { get; set; } = 10255;
            public string UserName { get; set; } = "kozmos";
            public string UserName1 { get => UserName; set => UserName = value; }
            public string Password { get; set; } = "PoEdJIpPQZDk72QzSFoKZudgqo9BddojPYIrP5EJMPRFfT25gpWwqvy1o1kHqgdHAijMsxvpIRDBT4yeDbYcBg==";
            public string DbName { get; set; } = "crey";
        }

        public MongoClient Client { get; private set; }
        public IMongoDatabase Database { get; private set; }

        public MongoConnection(Credentials creds)
        {
            var settings = new MongoClientSettings
            {
                Server = new MongoServerAddress(creds.Host, creds.Port),
                UseSsl = true,
                SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 },
                Credential = MongoCredential.CreateCredential(creds.DbName, creds.UserName, creds.Password),
                ApplicationName = "CreyMongo",
                RetryWrites = true,
            };

            Client = new MongoClient(settings);
            Database = Client.GetDatabase(creds.DbName)
                .ThrowIfNull(() => new Exception($"Couldn not connect to [{creds.Host}:{creds.Port}]/[{creds.DbName}]"));
        }
    }

    public class MongoCollection<T> where T : class
    {
        private readonly MongoConnection connection_;
        public IMongoCollection<T> Collection { get; }

        public MongoCollection(MongoConnection connection, string colectionName)
        {
            connection_ = connection;
            var mcs = new MongoCollectionSettings
            {
                AssignIdOnInsert = true,
            };

            Collection = connection_.Database.GetCollection<T>(colectionName, mcs);
        }
    }
}
