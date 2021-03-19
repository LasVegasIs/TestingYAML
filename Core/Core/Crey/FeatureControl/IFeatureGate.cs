using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crey.FeatureControl
{
    public interface IFeatureGate
    {
        Task<bool> IsFeatureEnabledAsync(string name);
        Task<Dictionary<string, bool>> GetFeaturesAsync(IEnumerable<string> name);

        Task<bool> IsChaosAsync(string route);
    }
}
