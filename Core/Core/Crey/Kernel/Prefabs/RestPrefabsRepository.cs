using Core.Functional;
using Crey.Contracts;
using Crey.Contracts.Prefabs;
using Crey.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace Crey.Kernel.Prefabs
{
    public class RestPrefabsRepository : IPrefabsRepository
    {
        private readonly CreyRestClient creyClient_;

        public RestPrefabsRepository(CreyRestClient creyClient)
        {
            creyClient_ = creyClient;
        }

        public async Task<List<PrefabInfo>> ListByKindAsync(IEnumerable<PrefabKind> kinds)
        {
            var content = new List<KeyValuePair<string, string>>();
            foreach (var kind in kinds)
            {
                content.Add(new KeyValuePair<string, string>("kinds", ((int)kind).ToString()));
            }

            return (await creyClient_.GetAsync<List<PrefabInfo>, Error>(PrefabsDefaults.SERVICE_NAME, $"v1/prefabs/list/kind", null, new FormUrlEncodedContent(content)))
                .UnwrapOr(err => throw new CommandErrorException<NoData>(err.Message));
        }

        public async Task<List<PrefabInfo>> ListByPackAsync(int packId)
        {
            return (await creyClient_.GetAsync<List<PrefabInfo>, Error>(PrefabsDefaults.SERVICE_NAME, $"v1/prefabs/list/pack/{packId}", null, null))
                .UnwrapOr(err => throw new CommandErrorException<NoData>(err.Message));
        }

        public async Task<List<PrefabInfo>> ListByUsageAsync(string usage)
        {
            return (await creyClient_.GetAsync<List<PrefabInfo>, Error>(PrefabsDefaults.SERVICE_NAME, $"v1/prefabs/list/usage/{usage}", null, null))
                .UnwrapOr(err => throw new CommandErrorException<NoData>(err.Message));
        }

        public async Task<List<string>> ListPrefabUsages(int prefabId)
        {
            return (await creyClient_.GetAsync<List<string>, Error>(PrefabsDefaults.SERVICE_NAME, $"/api/v1/prefabs/item/{prefabId}/usage", null, null))
                .UnwrapOr(err => throw new CommandErrorException<NoData>(err.Message));
        }

        public async Task<List<PrefabInfo>> ListAllBoxesAsync()
        {
            return (await creyClient_.GetAsync<List<PrefabInfo>, Error>(PrefabsDefaults.SERVICE_NAME, "v1/prefabs/list/boxes", null, null))
                .UnwrapOr(err => throw new CommandErrorException<NoData>(err.Message));
        }

        public async Task BuyPackAsync(int packId)
        {
            //var content = new List<KeyValuePair<string, int>>
            //{
            //    new KeyValuePair<string, int>("packId", packId),
            //};

            //var encodedContent = new FormUrlEncodedContent(content);
            //encodedContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            BuyParams buy = new BuyParams()
            {
                Packid = packId
            };
            var myContent = JsonConvert.SerializeObject(buy);
            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var code = await creyClient_.PostNoDataAsync(PrefabsDefaults.SERVICE_NAME, "api/v1/prefabs/buy", null, byteContent);
            if (code != HttpStatusCode.OK)
            {
                throw new ServerErrorException($"Unexpected status code: {code}");
            }
        }

        public async Task ClearCachedPrefabReferencesAsync(PrefabTargetType type, int targetId)
        {
            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("targetId", targetId.ToString()),
            };
            (await creyClient_.DeleteAsync<NoData, Error>(PrefabsDefaults.SERVICE_NAME, GetCachePath(type), null, new FormUrlEncodedContent(content)))
                .UnwrapOr(err => throw new CommandErrorException<NoData>(err.Message));
        }

        public async Task<Result<List<PrefabInfo>, Error>> SetCachedReferencesAsync(PrefabTargetType type, int targetId,
            IEnumerable<OwningPolicy> allowedOwnership,
            ICollection<string> prefabs)
        {
            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("targetId", targetId.ToString()),
            };
            foreach (var pref in prefabs)
            {
                content.Add(new KeyValuePair<string, string>("prefab", pref));
            }

            return await creyClient_.PutAsync<List<PrefabInfo>, Error>(PrefabsDefaults.SERVICE_NAME, GetCachePath(type), null, new FormUrlEncodedContent(content));
        }

        public async Task<Result<List<PrefabInfo>, Error>> GetCachedPrefabReferencesAsync(PrefabTargetType type, int targetId)
        {
            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("targetId", targetId.ToString()),
            };

            return await creyClient_.GetAsync<List<PrefabInfo>, Error>(PrefabsDefaults.SERVICE_NAME, GetCachePath(type), null, new FormUrlEncodedContent(content));
        }

        public async Task<Result<List<PrefabInfo>, Error>> CheckCachedReferencesAsync(PrefabTargetType type, int targetId, IEnumerable<OwningPolicy> allowedOwnership)
        {
            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("targetId", targetId.ToString()),
            };
            if (allowedOwnership != null)
            {
                foreach (var ownership in allowedOwnership)
                {
                    content.Add(new KeyValuePair<string, string>("allowedOwnership", ((int)ownership).ToString()));
                }
            }

            return await creyClient_.GetAsync<List<PrefabInfo>, Error>(PrefabsDefaults.SERVICE_NAME, GetCachePath(type) + "/check", null, new FormUrlEncodedContent(content));
        }

        private string GetCachePath(PrefabTargetType type)
        {
            switch (type)
            {
                case PrefabTargetType.LevelContent: return "v1/prefabs/cache/level";
                case PrefabTargetType.Box: return "v1/prefabs/cache/box";
            }

            throw new Exception($"Invalid PrefabTargetType: {type}");
        }

        public async Task<Result<PrefabInfo, Error>> ModeratePrefabAsync(int targetId, bool ban)
        {
            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("targetId", targetId.ToString()),
                new KeyValuePair<string, string>("ban", ban.ToString())
            };

            return await creyClient_.PostAsync<PrefabInfo, Error>(PrefabsDefaults.SERVICE_NAME, $"v1/comments/moderate", null, new FormUrlEncodedContent(content));
        }
    }
}
