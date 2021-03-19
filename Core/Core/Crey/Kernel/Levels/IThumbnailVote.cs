using Core.Functional;
using Crey.Contracts;
using System.Threading.Tasks;

namespace Crey.Kernel.Levels
{
    public interface IThumbnailVoteRepository
    {
        Task<Result<(int, int, string), Error>> GetVoteCandidatesAsync();

        Task<Error> VoteForAsync(string voteKey, int selectionIndex);

        Task<Result<int, Error>> GetVoteCountAsync(int levelId);
    }
}
