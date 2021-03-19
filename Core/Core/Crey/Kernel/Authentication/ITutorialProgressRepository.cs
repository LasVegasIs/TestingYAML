using Core.Functional;
using Crey.Contracts;
using System.Threading.Tasks;

namespace Crey.Kernel.Authentication
{
    public interface ITutorialProgressRepository
    {
        Result<TutorialProgressInfo, Error> Get();
        Task<Result<NoData, Error>> SetAsync(TutorialProgressValue tutorialProgress, int levelId, bool allowDecrease);
    }
}