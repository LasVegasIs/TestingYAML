using Core.Functional;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crey.Contracts.Prefabs
{
    public interface IPrefabsRepository
    {
        Task<List<PrefabInfo>> ListByKindAsync(IEnumerable<PrefabKind> kind);
        Task<List<PrefabInfo>> ListByPackAsync(int packId);
        Task<List<PrefabInfo>> ListByUsageAsync(string usage);
        Task<List<PrefabInfo>> ListAllBoxesAsync();

        Task BuyPackAsync(int packId);

        Task ClearCachedPrefabReferencesAsync(PrefabTargetType type, int targetId);
        Task<Result<List<PrefabInfo>, Error>> SetCachedReferencesAsync(PrefabTargetType type, int targetId,
            IEnumerable<OwningPolicy> allowedOwnership,
            ICollection<string> prefabs);
        Task<Result<List<PrefabInfo>, Error>> GetCachedPrefabReferencesAsync(PrefabTargetType type, int targetId);
        Task<Result<List<PrefabInfo>, Error>> CheckCachedReferencesAsync(PrefabTargetType type, int targetId, IEnumerable<OwningPolicy> allowedOwnership);

        Task<Result<PrefabInfo, Error>> ModeratePrefabAsync(int targetId, bool ban);
    }
}