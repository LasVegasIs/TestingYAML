using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Crey.Kernel.ContentConverter.Migration
{
    public class LevelMigrate_RenameResource : LevelConverterBase
    {

        public LevelMigrate_RenameResource()
        {
            _renames = new Dictionary<string, string>();
            // select CONCAT('_renames.Add("',OldName,'", "/',NewName,'");')from GameResourceRename
            _renames.Add("/assets/models/environment/props/gameplay/inverter.prefab", "/assets/prefabs/data-3699856a84ee4e2f9fec1cca0ea802d3.prefab");
            _renames.Add("/assets/models/characters/player/mm_adult/mm_adult_averagejoe/mm_adult_joe.prefab", "/assets/prefabs/data-35ba958d64324ca1834fd200c0664409.prefab");
            _renames.Add("/assets/models/characters/player/mm_trumph/mm_trumph.prefab", "/assets/prefabs/data-ca6e573aa0c44a668541f11c29018d0b.prefab");
            _renames.Add("/assets/models/characters/player/test/sphere/sphere.prefab", "/assets/prefabs/data-a16c641b878d4e87833566f2c65b4539.prefab");
            _renames.Add("/assets/models/characters/player/test/barrel/barrel.prefab", "/assets/prefabs/data-ea4f5616e0264f9c89b835f8f3bf0a1f.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/animator.prefab", "/assets/prefabs/data-718b65753ad24f178b28a701f17495b5.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/generator.prefab", "/assets/prefabs/data-1a9d3d276076401a96ccbfb3bae11a80.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/audio.prefab", "/assets/prefabs/data-38c01237ad62411f9dfba8379cb774c4.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/box.prefab", "/assets/prefabs/data-624600c6e2c94429b453a81c4ded6ca1.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/checkpoint.prefab", "/assets/prefabs/data-658eb85ef2fc405882655a0c457740fc.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/connector.prefab", "/assets/prefabs/data-b9958eff96824659a8d8b949fda11489.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/damage.prefab", "/assets/prefabs/data-5ea6695c6bfc42e68fcd9544adac1fca.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/directional_mover.prefab", "/assets/prefabs/data-09c4b5ed4deb4e2c8cc0b4c27f33805d.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/gameflow/levelfinish.prefab", "/assets/prefabs/data-77f47c24a9c6494ebcc8aef62eb5db0d.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/game_fail.prefab", "/assets/prefabs/data-ca9e2b6e4da049b48b44c9ba14b85dca.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/badge.prefab", "/assets/prefabs/data-cda82bc8a54440b889c49a77a6c0ce4d.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/counter.prefab", "/assets/prefabs/data-fadf31bd863d4b23b4eae5ef3b0126a5.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/input_trigger.prefab", "/assets/prefabs/data-78882b9e4560481a9c5a5b98b37f18e2.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/joint.prefab", "/assets/prefabs/data-402c27eb770a44b18905152d0cc1be65.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/lift.prefab", "/assets/prefabs/data-fee247121fc242bfa30e055766439a6e.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/rotator.prefab", "/assets/prefabs/data-ed24db254e3844b091230ce10646a22f.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/seeker.prefab", "/assets/prefabs/data-cf8d7a08637f44db9bf41fa494572248.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/sensor.prefab", "/assets/prefabs/data-00dc344c8e8346908c6c402fb6f95c4c.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/gameflow/startpoint.prefab", "/assets/prefabs/data-14997a027661471a80cc2d96150224f4.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/static_camera.prefab", "/assets/prefabs/data-eba481013a594b10bc5486add74c006b.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/timer.prefab", "/assets/prefabs/data-a6808fbeba86444b8c941a561bbf56af.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/textoverlay.prefab", "/assets/prefabs/data-6d668c73a601458999a569d6feafa805.prefab");
            _renames.Add("/assets/models/environment/props/primitives/prim_box.prefab", "/assets/prefabs/data-61084095e4a84662a3d0997996d7ec0f.prefab");
            _renames.Add("/assets/models/environment/props/primitives/prim_sphere.prefab", "/assets/prefabs/data-6fdcd2d939c3458aaf02539d9c5db871.prefab");
            _renames.Add("/assets/models/environment/props/primitives/prim_cylinder.prefab", "/assets/prefabs/data-b2f511ca634e46c182ceda223b8bec94.prefab");
            _renames.Add("/assets/models/environment/props/primitives/prim_cone.prefab", "/assets/prefabs/data-337aeab0f97d47f3958914df22883521.prefab");
            _renames.Add("/assets/models/environment/props/primitives/prim_pyramid.prefab", "/assets/prefabs/data-4407d3787141470fa588524135fe4ad0.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/ash/ash_small_1.prefab", "/assets/prefabs/data-90aabcd3b1f449b18bce1b074e1761fa.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/ash/ash_medium_1.prefab", "/assets/prefabs/data-a23c2642397a41c3822050b872456ab1.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/ash/ash_medium_red.prefab", "/assets/prefabs/data-08e9745913b64666bc1abddd2d82e646.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/ash/ash_giant_1.prefab", "/assets/prefabs/data-21811dea88ff49e9b6add9fab3c76f1d.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/ash/ash_regular_1.prefab", "/assets/prefabs/data-bd20747dfd3e42b7873f2053d4ed9485.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/ash/ash_blobs_1.prefab", "/assets/prefabs/data-804566b7085b4bdca3f770aa3a6686ee.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/pine/tree_pine_3.prefab", "/assets/prefabs/data-c6320f562e23409b964b9bcec285341f.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/pine/tree_pine_4.prefab", "/assets/prefabs/data-9aedfdd0acef4ae1a587c82cfd5afed5.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/pine/tree_pine_5.prefab", "/assets/prefabs/data-f182d1c7c61b416f83f423e4d51705cd.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/pine/tree_pine_6.prefab", "/assets/prefabs/data-dc203e9079334c9f9eb3a9433e8c3294.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/oak/tree_oak_1.prefab", "/assets/prefabs/data-0a31fd5a744d4ffdba5761697da2c854.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/oak/tree_oak_yellow.prefab", "/assets/prefabs/data-2eafdcefa8974eb2866cce0d00f35e1c.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/oak/tree_oak_red.prefab", "/assets/prefabs/data-2477d912c53e4c0698e6c4039e3bbde9.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/oak/oak_blob_green.prefab", "/assets/prefabs/data-b2391ac507f04e38b10e457f3385672c.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/pine/trunk_1.prefab", "/assets/prefabs/data-ae0c4b5a6dd442659af888ef895ec24d.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/oak/oak_trunk_1.prefab", "/assets/prefabs/data-7095b906d18445beaa42f3ff1d6d2946.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/pine/pine_foli_big.prefab", "/assets/prefabs/data-05f3f5656a4d477e848d21fa72aa5130.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/pine/pine_foli_small.prefab", "/assets/prefabs/data-fa251cf65e3f42ba893cfcf4429edd82.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/pine/branch_pine_5.prefab", "/assets/prefabs/data-63829163346948fc8e45105aa78af3a4.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks/block_rocks/block_rock_mossy_5.prefab", "/assets/prefabs/data-afe8ee664e46413eb7a688d6f85786c1.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks/block_rocks/block_rock_sandy_4.prefab", "/assets/prefabs/data-d525400f933c43c3bc2451656753e442.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/fern_1/fern_1.prefab", "/assets/prefabs/data-c2db1a7a42c8411eba099f57bf0fa4b4.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/water_lilies_1/water_lilies_1.prefab", "/assets/prefabs/data-0a7109563ec3464fb2d97eff8304a963.prefab");
            _renames.Add("/assets/models/environment/props/nature/plants/bushes/wheat_01.prefab", "/assets/prefabs/data-aba4b906f9b743b1a9fdb34f3e22b213.prefab");
            _renames.Add("/assets/models/environment/props/landscapes/lowpoly/mountain_50_a.prefab", "/assets/prefabs/data-3bdf2d485801471999c041be0fce7e9e.prefab");
            _renames.Add("/assets/models/environment/props/landscapes/lowpoly/mountain_100_c.prefab", "/assets/prefabs/data-185189c7ff4045ae94c874e519d36d8a.prefab");
            _renames.Add("/assets/models/environment/props/nature/atmospherics/clouds/cloud_white_small.prefab", "/assets/prefabs/data-19a0065bba2e4dcea3bd14a4c6fdd749.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/pickups/pickup_01.prefab", "/assets/prefabs/data-dedf64781e034c06bbbc72fd4e751f4c.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/pickups/pickup_02.prefab", "/assets/prefabs/data-750711679d544affa131c07212164352.prefab");
            _renames.Add("/assets/models/environment/props/fx/puff/puff.prefab", "/assets/prefabs/data-837229f06f1540a38888477436492116.prefab");
            _renames.Add("/assets/models/environment/props/fx/sun/sun_01.prefab", "/assets/prefabs/data-ec7bf1eef5684489879419e381985cc0.prefab");
            _renames.Add("/assets/models/environment/props/fx/fog/fog_01.prefab", "/assets/prefabs/data-384c2161ab5849aa8de8b76cad030b28.prefab");
            _renames.Add("/assets/models/characters/player/mm_karate/karate_red.prefab", "/assets/prefabs/data-58a2edf6ed9b4882b25996111f837225.prefab");
            _renames.Add("/assets/models/characters/player/mm_karate/karate_white.prefab", "/assets/prefabs/data-5f162960f13a45908ae909d5c9027787.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/christmasset/christmasarrangement.prefab", "/assets/prefabs/data-37453bb59a634800b4fd6f0548878dc5.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/christmasset/tree.prefab", "/assets/prefabs/data-90b8675f4bf34c9e8353de7899afef05.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/christmasset/present_square_blue.prefab", "/assets/prefabs/data-edfdd4356b594f9c948b7959c719c315.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/christmasset/present_square_white.prefab", "/assets/prefabs/data-86b71bb0d2ee41b79e22b58637edfba4.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/christmasset/present_square_red.prefab", "/assets/prefabs/data-ae728b81b2cf492b863bdd0cb17c66ce.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/tree_birch/birch_collection_autumn.prefab", "/assets/prefabs/data-7c5e1bf3d6424d579dd8efaac6e0cf10.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/tree_birch/birch_collection_lateautumn.prefab", "/assets/prefabs/data-7bcaffd0660d4f1f8c8118ab55679df7.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/tree_birch/birch_collection_summer.prefab", "/assets/prefabs/data-75d248a596144bf4a21bb74249750f09.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/tree_birch/birch_collection_winter.prefab", "/assets/prefabs/data-8d512c4fd81645b3af415a4fe76cc049.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/tree_pine/pine_group_snow.prefab", "/assets/prefabs/data-9b91d0a0715448d6acfc6dd28d4dfc0b.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/tree_pine/pine_group_green.prefab", "/assets/prefabs/data-8e5452238ca34ba28b21aed86675f180.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/tree_pine/pine_group_red.prefab", "/assets/prefabs/data-1d9bfc32eaa94594a73c4ddbfb9cdb60.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/tree_pine/pine1_green.prefab", "/assets/prefabs/data-bfc34c58e9ab4208aeee9061e927976e.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/tree_pine/pine1_snow.prefab", "/assets/prefabs/data-bb710acdb69e4c9dba74006cc8559279.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/tree_pine/pine1_red.prefab", "/assets/prefabs/data-95bc0770ce284190b0eb8aee0185234e.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/karate/bench.prefab", "/assets/prefabs/data-816819a9939b47c5b108e36a06f07a77.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/karate/floor_tile_x2.prefab", "/assets/prefabs/data-eeb848ba101b46ba94cab71e0796f6be.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/karate/floor_x4.prefab", "/assets/prefabs/data-838ba573cd564811bb6d2a625490d310.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/karate/gate.prefab", "/assets/prefabs/data-f418fb8fe96541ae8582240d4fa685f5.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/karate/island_x1.prefab", "/assets/prefabs/data-b4948405954a4185a848f89859052703.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/karate/island_x2.prefab", "/assets/prefabs/data-e1b23bf8f28144bfa8516df280bad857.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/karate/island_x4.prefab", "/assets/prefabs/data-ecde429bdf5e4ad0b0f9903b7d9d718c.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/karate/pillar_small.prefab", "/assets/prefabs/data-874ca244772649f8bfe07ce9eb9ca227.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/karate/stone_01.prefab", "/assets/prefabs/data-27b255a8939245df99656eb4d30b94eb.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/karate/stone_02.prefab", "/assets/prefabs/data-f64c9e5153f04a2c8316ee63ae165a1d.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/karate/tree.prefab", "/assets/prefabs/data-213cd3f75c69431695246abab46c3d40.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/house.prefab", "/assets/prefabs/data-22ae6e3b0d3543ab8277c13f3a45152f.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/wooden_beam_long.prefab", "/assets/prefabs/data-a9b6c583faef4ac7980510789ab55ab0.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/wooden_beam_short.prefab", "/assets/prefabs/data-b6ce986465254b1881eb82eeb0f0595f.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/tower.prefab", "/assets/prefabs/data-2120356a64764d4b87a31eba87a3fd1b.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/barrel.prefab", "/assets/prefabs/data-646ad2ead1764cc0843bf99f4e86f788.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/box_large.prefab", "/assets/prefabs/data-31373662599648289de403473fb8fa3a.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/box_small.prefab", "/assets/prefabs/data-26e106a37e494c33b3a88ec52e91110c.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/lantern_mounted.prefab", "/assets/prefabs/data-0eea858aee0e44618538d412367792bf.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/lantern_street.prefab", "/assets/prefabs/data-50387ed1adb942ef8bc58eacc9a2ed00.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/pillar_1.prefab", "/assets/prefabs/data-b1e62dda79d0416ca3e15bf0e1a1547b.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/pillar_2.prefab", "/assets/prefabs/data-2117f8e6fb934d189f44c54b6d06bd6c.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/platform_x4.prefab", "/assets/prefabs/data-6ab7b0070c60407a90aca053ac75bc8d.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/practice_dummy.prefab", "/assets/prefabs/data-efe2534d7e49409fa161c5c0007dea53.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/target.prefab", "/assets/prefabs/data-00b179052ab940f99e899ec395d52494.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/trap_door.prefab", "/assets/prefabs/data-7604c4c755b749f3bfe20f2770f3a3a3.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/trap_door_flap.prefab", "/assets/prefabs/data-ed8dba9a1fe14abcbde75578de2c6e81.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/wall_x1.prefab", "/assets/prefabs/data-a34affece1d2480aa2ea1b468b054e29.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/wall_x2.prefab", "/assets/prefabs/data-c9c4fa9ac3bc44e0aa5c750ccc06b36d.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/wall_x4.prefab", "/assets/prefabs/data-c2b6be934edb431ba4d2c6cd40d42c2b.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_spiralstairs_01.prefab", "/assets/prefabs/data-1ab7da7765704e77b1de3e42e5040bd8.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_stairs_02.prefab", "/assets/prefabs/data-990a6daae0e04a9dbad97ed797a75061.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_wall_01.prefab", "/assets/prefabs/data-3ee52fe764d042d3a06d7baedcbf2764.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_wall_alcove_round_01.prefab", "/assets/prefabs/data-dde1c7c18fbb419fa4684d52a355d80b.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_wall_pillar_large_01.prefab", "/assets/prefabs/data-f27def6318a4478d94a78beef8dca0e8.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_wall_round_01.prefab", "/assets/prefabs/data-47ac8754c13a45dc9d0035e71d783f57.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_prop_trap_grinder_01.prefab", "/assets/prefabs/data-7b1876920bd745fea6ebd51ce5c9c5f1.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_prop_trap_wallslab_spikes_01.prefab", "/assets/prefabs/data-a7c2dca0dcf54da3ab83e9f81fdfdb19.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_trap_saw_02.prefab", "/assets/prefabs/data-e842e37750464884bff0a811d9f93115.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_wep_goblin_club_01.prefab", "/assets/prefabs/data-59204d7da2124aa0b48739cd443f7aad.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_door_frame_round_01.prefab", "/assets/prefabs/data-457ca0b75abc4546b6cb3f3b10fb933a.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_entrance_crypt_03.prefab", "/assets/prefabs/data-9f3aa3001519443bac8abed3551d365f.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_entrance_large_01.prefab", "/assets/prefabs/data-52bcc14cd1b84345bf6a659f9b468c2d.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_glowingorb_01.prefab", "/assets/prefabs/data-df3acd36e27444f9939e1c0eb2333e6f.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_minetrack_straight_01.prefab", "/assets/prefabs/data-602ff9230ef94cec9c3dd32484d025e2.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_minetrack_turn_01.prefab", "/assets/prefabs/data-1352382a5fff4abaae4405a9a6d88e46.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_mushroom_giant_05.prefab", "/assets/prefabs/data-6217f1e6d286446e924dcc814e460c4e.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_obelisk_01.prefab", "/assets/prefabs/data-2a6d8bb4bae84c799ef58dd6f3933bc9.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_pillar_broken_pile_02.prefab", "/assets/prefabs/data-2c52ffe5659b48c5a6e8289513c80661.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_pillar_round_02.prefab", "/assets/prefabs/data-5f5d8cc433f54f86b5b00611f12670a5.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_pillar_square_07.prefab", "/assets/prefabs/data-5cc050c4028145e0a794356e41824ecf.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_railing_angle_01.prefab", "/assets/prefabs/data-1d63385bd10941c78a14b5a4e13e0038.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_stairs_large_02.prefab", "/assets/prefabs/data-183928487f23480e9fa7d289333a4fd0.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_statue_01.prefab", "/assets/prefabs/data-a3559b20e8534596a994104dc605eab5.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_tree_dandelion_01.prefab", "/assets/prefabs/data-65acc52c4c4d4299b05291edbb33827e.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_tree_dandelion_02.prefab", "/assets/prefabs/data-3a0108db1ffb4814a0019a65106b5671.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_vine_spikes_01.prefab", "/assets/prefabs/data-26af9ffc6c9944fe9b9ba173a8d2ada8.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_wall_broken_edge_01.prefab", "/assets/prefabs/data-f9d5562c16d74d4cb52bd370d89a3f40.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_wall_window_02.prefab", "/assets/prefabs/data-07128db8ac0d4397ba2f39ffb91b71b0.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_wood_platform_01.prefab", "/assets/prefabs/data-0db61f2e5cb245588ee623e1e5bf7a76.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_prop_log_spike_large_02.prefab", "/assets/prefabs/data-883fc75f84e8497281412bfce5f69744.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_prop_minecart_01.prefab", "/assets/prefabs/data-e4042248ca214977a9f3ca38f43bd923.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_trap_spears_01.prefab", "/assets/prefabs/data-0a184f2dffd044e0ab7cdafea25ecfea.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_trap_swing_blade_01.prefab", "/assets/prefabs/data-af0944e493d74e6fa1d34ccd7ea26b4a.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks/block_rocks/block_rock_sandy_3.prefab", "/assets/prefabs/data-e3eed347620640e8983122708a42e245.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks/block_rocks/block_rock_mossy_1.prefab", "/assets/prefabs/data-1448cce746674dc999f508ea0cf2dfcb.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks/block_rocks/block_rock_mossy_2.prefab", "/assets/prefabs/data-cbaa8e29af5445eabedb2ff765f3551f.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks/block_rocks/block_rock_mossy_3.prefab", "/assets/prefabs/data-2f8bffef82f54f889ea0ad8f4303a4c4.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks/block_rocks/block_rock_mossy_4.prefab", "/assets/prefabs/data-0dae825b1b5d42518125b0e6a4039ca2.prefab");
            _renames.Add("/assets/models/environment/props/landscapes/lowpoly/mountain_100_b.prefab", "/assets/prefabs/data-1519c051382d47159cf3a1d8fabf3e38.prefab");
            _renames.Add("/assets/models/environment/props/landscapes/lowpoly/mountain_100_a.prefab", "/assets/prefabs/data-4dd2519969124d6f8710edc6c92f13b0.prefab");
            _renames.Add("/assets/prefabs/data-d7ce2e46a24643f2a49e820283187f35.prefab", "/assets/prefabs/data-624600c6e2c94429b453a81c4ded6ca1.prefab");
            _renames.Add("/assets/prefabs/data-7912bed4cf2649b09283265abe92501a.prefab", "/assets/prefabs/data-624600c6e2c94429b453a81c4ded6ca1.prefab");
            _renames.Add("/assets/prefabs/7912bed4cf2649b09283265abe92501a.prefab", "/assets/prefabs/data-624600c6e2c94429b453a81c4ded6ca1.prefab");
            _renames.Add("/assets/prefabs/dbed4795f3534f5b92f857154d92c803.prefab", "/assets/prefabs/data-00dc344c8e8346908c6c402fb6f95c4c.prefab");
            _renames.Add("/assets/prefabs/675365b73056487da25d59a64043db27.prefab", "/assets/prefabs/data-61084095e4a84662a3d0997996d7ec0f.prefab");
            _renames.Add("/assets/prefabs/7c78af0fdf5e4ae790b5ced940202bc6.prefab", "/assets/prefabs/data-cf8d7a08637f44db9bf41fa494572248.prefab");
            _renames.Add("/assets/prefabs/2f99a5b8dff04a3fb3f1a2535efe81e8.prefab", "/assets/prefabs/data-5ea6695c6bfc42e68fcd9544adac1fca.prefab");
            _renames.Add("/assets/prefabs/bdf145ec190e4d769541ed655825c402.prefab", "/assets/prefabs/data-61084095e4a84662a3d0997996d7ec0f.prefab");
            _renames.Add("/assets/prefabs/d7ce2e46a24643f2a49e820283187f35.prefab", "/assets/prefabs/data-624600c6e2c94429b453a81c4ded6ca1.prefab");
            _renames.Add("/assets/prefabs/data-dbed4795f3534f5b92f857154d92c803.prefab", "/assets/prefabs/data-00dc344c8e8346908c6c402fb6f95c4c.prefab");
            _renames.Add("/assets/prefabs/data-675365b73056487da25d59a64043db27.prefab", "/assets/prefabs/data-61084095e4a84662a3d0997996d7ec0f.prefab");
            _renames.Add("/assets/prefabs/data-7c78af0fdf5e4ae790b5ced940202bc6.prefab", "/assets/prefabs/data-cf8d7a08637f44db9bf41fa494572248.prefab");
            _renames.Add("/assets/prefabs/data-2f99a5b8dff04a3fb3f1a2535efe81e8.prefab", "/assets/prefabs/data-5ea6695c6bfc42e68fcd9544adac1fca.prefab");
            _renames.Add("/assets/prefabs/data-bdf145ec190e4d769541ed655825c402.prefab", "/assets/prefabs/data-61084095e4a84662a3d0997996d7ec0f.prefab");
            _renames.Add("/assets/prefabs/data-8fff698afeee4fb7b616b6075f327051.prefab", "/assets/prefabs/data-a793fe21fdf24744a207232c4cb8295d.prefab");
            _renames.Add("/assets/models/environment/props/gameplay/hider.prefab", "/assets/prefabs/data-a793fe21fdf24744a207232c4cb8295d.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_env_entrance_crypt_01.prefab", "/assets/prefabs/data-ade1bb8c538f4aecaf16675709b6ca52.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_medieval/plant_02.prefab", "/assets/prefabs/data-3e0ccd66f92d406e8427e56dcdb68141.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_basic/sword.prefab", "/assets/prefabs/data-e2ef069d86fe49e9bca36b4e6d5d7d63.prefab");
            _renames.Add("/assets/models/environment/props/primitives/colors/brick_square_yellow.prefab", "/assets/prefabs/data-9ae2755475d644eeb1c1959cc39c3eee.prefab");
            _renames.Add("/assets/models/environment/props/primitives/colors/brick_square_green.prefab", "/assets/prefabs/data-9483eb5ebbc0486182d63aa094a34818.prefab");
            _renames.Add("/assets/models/environment/props/fx/glows/glowplane_06.prefab", "/assets/prefabs/data-331e6c6e6af0477185e2b3703cd9b482.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks/block_rocks/block_rock_sandy_2.prefab", "/assets/prefabs/data-6a2d91ace25c4480a15ad79ac1f01c8e.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks/block_rocks/block_rock_sandy_1.prefab", "/assets/prefabs/data-0c0d6a7260ef431e9e3741c074ebd7d8.prefab");
            _renames.Add("assets/models/characters/npc/animals/bacon/bacon_1.prefab", "/assets/prefabs/data-2ef1a534e6c74b01a0cf26141729f783.prefab");
            _renames.Add("/assets/models/environment/props/primitives/prim_sphere_dynamic_50cm/prim_sphere_dynamic_50cm.prefab", "/assets/prefabs/data-081cd80d8c774274839f3a608bcedca7.prefab");
            _renames.Add("/assets/models/environment/props/buildings/greeble/greeble_01.prefab", "/assets/prefabs/data-4ef6e94c3b4d4cd0a93cf178e60f2472.prefab");
            _renames.Add("/assets/models/environment/props/primitives/colors/brick_square_large_green.prefab", "/assets/prefabs/data-fd78831d9490434287de07387cd9b58a.prefab");
            _renames.Add("/assets/models/environment/props/primitives/colors/brick_square_large_black.prefab", "/assets/prefabs/data-97f11a115ac7411fb2064455003472b2.prefab");
            _renames.Add("/assets/models/environment/props/primitives/colors/brick_square_large_red.prefab", "/assets/prefabs/data-f5d2b8793d43466f8475993093f0fcd9.prefab");
            _renames.Add("/assets/models/environment/props/fx/smoke/smoke_boxes.prefab", "/assets/prefabs/data-0c4cf00ada6743fabe0d1955af9256b7.prefab");
            _renames.Add("/assets/models/environment/props/primitives/tube_1/tube_1.prefab", "/assets/prefabs/data-11152d6a890d41e1a3fc9dd1afa40074.prefab");
            _renames.Add("/assets/models/_thirdparty/synty_studios/basic_dungeon/sm_trap_base_01.prefab", "/assets/prefabs/data-ce24a39ec1f54d0eb36803603dab1869.prefab");
            _renames.Add("/assets/models/characters/npc/animals/gorilla/gorilla.prefab", "/assets/prefabs/data-6fb5ed0952f247458769c55c2a119bdf.prefab");
            _renames.Add("/assets/models/characters/npc/animals/jelly/jelly.prefab", "/assets/prefabs/data-ace8c8af78fc4474b211466133847b73.prefab");
            _renames.Add("/assets/models/characters/npc/neanderthalwoman/neanderthalwoman.prefab", "/assets/prefabs/data-0cc852f2294a4866ab7e81d02981a749.prefab");
            _renames.Add("/assets/models/characters/player/neanderthalman/neanderthalman_1.prefab", "/assets/prefabs/data-fccf3a94a0104ac6b6ffd4999c79d767.prefab");
            _renames.Add("/assets/models/characters/npc/animals/insects/dragonfly.prefab", "/assets/prefabs/data-f67573c133e347dda5715aa20a238696.prefab");
            _renames.Add("/assets/models/characters/npc/animals/insects/bee.prefab", "/assets/prefabs/data-ae61ee38b5a341d8abc89b37d8530e78.prefab");
            _renames.Add("/assets/models/characters/npc/lm_queen/lm_queen.prefab", "/assets/prefabs/data-9b85611f654b48409d486878c5de0199.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_medieval/plant_03.prefab", "/assets/prefabs/data-b3683f6d2fe74331b20a406d76af9c6e.prefab");
            _renames.Add("/assets/models/environment/props/bitgem/castle_medieval/stone_03.prefab", "/assets/prefabs/data-a611766f6baa443ca45bab6e462cc50c.prefab");
            _renames.Add("assets/models/environment/props/nature/rocks//block_rocks/block_rock_sandy_1.prefab", "/assets/prefabs/data-0c0d6a7260ef431e9e3741c074ebd7d8.prefab");
            _renames.Add("assets/models/environment/props/nature/rocks//block_rocks/block_rock_sandy_2.prefab", "/assets/prefabs/data-6a2d91ace25c4480a15ad79ac1f01c8e.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks//block_rocks/block_rock_sandy_2.prefab", "/assets/prefabs/data-6a2d91ace25c4480a15ad79ac1f01c8e.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks//block_rocks/block_rock_sandy_1.prefab", "/assets/prefabs/data-0c0d6a7260ef431e9e3741c074ebd7d8.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks//block_rocks/block_rock_sandy_3.prefab", "/assets/prefabs/data-e3eed347620640e8983122708a42e245.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks//block_rocks/block_rock_sandy_4.prefab", "/assets/prefabs/data-d525400f933c43c3bc2451656753e442.prefab");
            _renames.Add("/assets/models/characters/npc/animals/bacon/bacon_1.prefab", "/assets/prefabs/data-2ef1a534e6c74b01a0cf26141729f783.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks//block_rocks/block_rock_mossy_1.prefab", "/assets/prefabs/data-1448cce746674dc999f508ea0cf2dfcb.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks//block_rocks/block_rock_mossy_2.prefab", "/assets/prefabs/data-cbaa8e29af5445eabedb2ff765f3551f.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks//block_rocks/block_rock_mossy_3.prefab", "/assets/prefabs/data-2f8bffef82f54f889ea0ad8f4303a4c4.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks//block_rocks/block_rock_mossy_4.prefab", "/assets/prefabs/data-0dae825b1b5d42518125b0e6a4039ca2.prefab");
            _renames.Add("/assets/models/environment/props/nature/rocks//block_rocks/block_rock_mossy_5.prefab", "/assets/prefabs/data-afe8ee664e46413eb7a688d6f85786c1.prefab");
            _renames.Add("/assets/models/environment/props/primitives/prim_plane.prefab", "/assets/prefabs/data-5633424c75b6440a8e297f2d50f49ac5.prefab");
        }

        private readonly Dictionary<string, string> _renames;

        private string NewName(string oldName)
        {
            string newName;
            if (!_renames.TryGetValue(oldName, out newName))
                newName = oldName;
            return newName;
        }

        public override (ConversionResult, JObject) Convert(JObject content)
        {
            var changed = false;
            foreach (var res in AllResources(content))
            {
                if (IsResourceRefNode(res))
                {
                    var name = res["resource_id"].ToString();
                    var newName = NewName(name);
                    //Console.WriteLine($"replace resource ref: {res["resource_id"]} -> {newName}");
                    changed = (name != newName);
                    res["resource_id"] = newName;

                }
                else if (IsPrefabDescNode(res))
                {
                    var name = res["prefabName"].ToString();
                    if (name == null || name == "")
                        throw new ConversionError($"Empty original prefab name");
                    var newName = NewName(name);
                    //Console.WriteLine($"replace prefab desc: {res["prefabName"]} -> {newName}");
                    changed = (name != newName);
                    res["prefabName"] = newName;
                    if (res["prefabName"] == null || res["prefabName"].Value<string>() == "")
                        throw new ConversionError($"Empty new prefab name");
                }
            }

            return (changed ? ConversionResult.Modified : ConversionResult.NoChange, content);
        }
    }
}