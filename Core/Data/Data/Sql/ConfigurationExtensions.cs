#nullable enable
using Crey.Instrumentation.Configuration;
using Microsoft.Extensions.Configuration;

namespace Crey.Data.Sql
{
    public static class ConfigurationExtensions
    {
        public static string GetSQLConnectionString(this IConfiguration configuration, string service)
        {
            var slot = configuration.GetDeploymentSlot();
            var stage = configuration.GetStage();
            var (stageSlot, _) = configuration.GetSplitStage();
            if (stageSlot != configuration.GetDeploymentSlot())
                throw new Crey.Instrumentation.Web.InternalServerErrorException($"Configuration error stage-slot miss-match: {stage}, {slot}");

            var cns = configuration.GetValue<string>("SqlCns");
            return cns
                .SubstituteServicePattern(service)
                .SubstituteStagePattern(stage!);
        }
    }
}