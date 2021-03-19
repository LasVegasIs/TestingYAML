using Newtonsoft.Json.Linq;

namespace Crey.Kernel.ContentConverter.Migration
{

    public enum MigrationKind
    {
        Prefab,
        Box,
        Level,
    }

    public class LevelMigrate : LevelConverterBase
    {
        private readonly int _targetLevelVersion;

        public MigrationKind Kind { get; set; } = MigrationKind.Level;

        public LevelMigrate(int targetLevelVersion = int.MaxValue)
        {
            _targetLevelVersion = targetLevelVersion;
        }

        public override (ConversionResult, JObject) Convert(JObject content)
        {
            ConversionResult resultState = ConversionResult.NoChange;
            ConversionResult tempState = ConversionResult.NoChange;

            // level incrementing conversions
            if (content != null && GetLevelVersion(content) < _targetLevelVersion)
            {
                (tempState, content) = new LevelMigrate_8_PhxSensor().Convert(content);
                if (tempState == ConversionResult.Modified)
                    resultState = tempState;
            }


            //non level incrementing migrations
            if (content != null)
            {
                (tempState, content) = new LevelMigrate_DeDupBadgeGuid().Convert(content);
                if (tempState == ConversionResult.Modified)
                {
                    resultState = tempState;
                }
            }
            if (Kind != MigrationKind.Prefab && GetLevelVersion(content) < 10)
            {
                (tempState, content) = new LevelMigrate_HackPrefabDesc().Convert(content);
                if (tempState == ConversionResult.Modified)
                {
                    resultState = tempState;
                }
            }
            if (Kind != MigrationKind.Prefab && GetLevelVersion(content) <= 10)
            {
                (tempState, content) = new LevelMigrate_RenameResource().Convert(content);
                if (tempState == ConversionResult.Modified)
                {
                    resultState = tempState;
                }
            }

            return (resultState, content);
        }
    }
}
