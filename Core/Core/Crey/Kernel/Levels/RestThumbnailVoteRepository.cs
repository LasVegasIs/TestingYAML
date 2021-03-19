using Core.Functional;
using Crey.Contracts;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Crey.Kernel.Levels
{
    public class RestThumbnailVoteRepository : IThumbnailVoteRepository
    {
        private readonly CreyRestClient creyClient_;

        public RestThumbnailVoteRepository(CreyRestClient creyClient)
        {
            creyClient_ = creyClient;
        }

        public async Task<Result<(int, int, string), Error>> GetVoteCandidatesAsync()
        {
            return await creyClient_.GetAsync<(int, int, string), Error>(LevelsDefaults.SERVICE_NAME, $"api/v1/levels/vote/candidates", null, null);
        }

        public async Task<Result<int, Error>> GetVoteCountAsync(int levelId)
        {
            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("levelId", levelId.ToString())
            };

            return await creyClient_.GetAsync<int, Error>(LevelsDefaults.SERVICE_NAME, $"api/v1/levels/vote/vote", null, new FormUrlEncodedContent(content));
        }

        public async Task<Error> VoteForAsync(string voteKey, int selectionIndex)
        {
            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("voteKey", voteKey),
                new KeyValuePair<string, string>("selectionIndex", selectionIndex.ToString())
            };

            return (await creyClient_.PostAsync<NoData, Error>(LevelsDefaults.SERVICE_NAME, $"api/v1/levels/vote/vote", null, new FormUrlEncodedContent(content)))
                .Match(ok => Error.NoError, error => error);
        }
    }
}
