using System.Runtime.Serialization;

namespace Crey.Contracts
{

    ///             Important: Blocky approach, you can add stuff to the END of the block !


    [DataContract]
    public enum TcpInstruction
    {
        [EnumMember] Unknown = 0,
        [EnumMember] Welcome,
        [EnumMember] WtfError, // generic error, like when determining the instruction fails
        [EnumMember] SessionShutdown,

        [EnumMember] SignInWithKey = 101,

        #region Game resources
        [EnumMember] ResourceUploadDev = 200,
        [EnumMember] ResourceUploadMyByName,
        [EnumMember] ResourceGetByNameWithPrefs,
        [EnumMember] ResourceGetById,
        [EnumMember] ResourceSetVersionPrefs,
        [EnumMember] RESOURCE_DISABLED_0,
        [EnumMember] ResourceListByIds,
        [EnumMember] ResourceUploadMyById,
        [EnumMember] ResourceDISABLED_2,
        [EnumMember] ResourceListByNameWithPrefs,
        [EnumMember] ResourceDeployVersion,
        [EnumMember] RESOURCE_DISABLED_3,
        [EnumMember] CreatePublicInitialResourceList,
        [EnumMember] GenerateDataHashOnStorage,
        #endregion

        #region Gallery handling
        [EnumMember] GalleryRegisterInitial = 400,
        [EnumMember] GalleryUpdateInitial,
        [EnumMember] GalleryListAllBoxes,
        [EnumMember] GalleryListByUsage,
        [EnumMember] GalleryListByPack,
        [EnumMember] GalleryGetByPrefabName,
        [EnumMember] GalleryAssignToPack,
        [EnumMember] GalleryUpdate,
        [EnumMember] GalleryRegister,
        [EnumMember] DISABLED_0,
        [EnumMember] GallerySetUsages,
        [EnumMember] GalleryGetUsages,
        #endregion

        #region Prefab pack handling
        [EnumMember] PrefabProviderRegister = 450,
        [EnumMember] PrefabProviderUpdate,
        [EnumMember] PrefabProviderListAll,
        [EnumMember] PrefabProviderGetById,

        [EnumMember] PrefabPackRegister = 460,
        [EnumMember] PrefabPackUpdate,
        [EnumMember] PrefabPackListByProvider,
        [EnumMember] PrefabPackGetById,
        #endregion

        #region UserProfile
        [EnumMember] UserProfileGet = 1200,
        [EnumMember] UserProfileListById,

        #endregion
    }
}