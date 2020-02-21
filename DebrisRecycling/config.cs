using Common;
using Common.Configuration;

namespace DebrisRecycling
{
	using PrefabsList = System.Collections.Generic.Dictionary<string, int>;

	class L10n: LanguageHelper
	{
		public const string ids_smallScrapName = "Small piece of salvage";
		public const string ids_smallScrapDesc = "Composed primarily of titanium.";

		public static string ids_salvageableDebris = "Salvageable debris";
		public static string ids_tryMoveObject = "Try to move object"; // probably not used very often
	}


	[Options.Name("Debris Recycling <color=#CCCCCCFF>(restart game to apply options)</color>")]
	class ModConfig: Config
	{
#pragma warning disable CS0414 // unused field
		[Field.LoadOnly] readonly bool dynamicMetalScrapRequirements = true; // obsolete
#pragma warning restore

		public class CraftConfig
		{
			[Options.Field("Dynamic titanium blueprint")]
			public readonly bool dynamicTitaniumBlueprint = true;

			public readonly int titaniumPerBigScrap = 4;
			public readonly int titaniumPerSmallScrap = 1;
		}
		public readonly CraftConfig craftConfig = new CraftConfig();

		protected override void onLoad() // v1.0.0 -> v1.0.1 (dynamicMetalScrapRequirements -> craftConfig.dynamicTitaniumBlueprint)
		{
			if (!dynamicMetalScrapRequirements)
				typeof(CraftConfig).field(nameof(CraftConfig.dynamicTitaniumBlueprint)).SetValue(craftConfig, false); // using reflection to keep field readonly
		}

		public class PrefabsConfig
		{
			[Options.Field("Deconstruct closed crates")]
			public readonly bool includeClosedCargo = true;

			[Options.Field("Deconstruct lockers")]
			public readonly bool includeLockers = true;

			[Options.Field("Deconstruct furniture")]
			public readonly bool includeFurniture = false;

			public readonly bool includeTech = false;
#if !EXCLUDE_STATIC_DEBRIS
			public readonly bool includeBigStatic = false;
#endif
		};
		public readonly PrefabsConfig prefabsConfig = new PrefabsConfig();

		public readonly bool deconstructValidStaticObjects = true;
		public readonly bool patchStaticObjects = true;
		public readonly bool hotkeyForNewObjects = false;
	};


	// key = prefab id, value = ScrapMetal * 10 + ScrapMetalSmall
	class PrefabIDs: Config
	{
		public readonly PrefabsList debrisCargoOpened = new PrefabsList()
		{
			{"c390fcfc-3bf4-470a-93bf-39dafb8b2267",  2}, // Starship_cargo_opened
			{"8c3d54c0-4330-4949-91ad-f046cfd67c7c",  2}, // Starship_cargo_damaged_opened_01
			{"ebc835bd-221a-4722-b1d0-becf08bd2f2c",  2}, // Starship_cargo_damaged_opened_02
			{"af413920-4fe6-4447-9f62-4f04e605d6be", 10}, // Starship_cargo_opened_large
			{"a2104a9e-fe84-4c51-8874-69350507ef98", 10}, // Starship_cargo_damaged_opened_large_01
			{"fb2886c4-7e03-4a47-a122-dc7242e7de5b", 10}  // Starship_cargo_damaged_opened_large_02
		};

		public readonly PrefabsList debrisMiscMovable = new PrefabsList()
		{
			{"5cd34124-935f-4628-b694-a266bc2f5517", 11}, // Starship_exploded_debris_01
			{"df36cdfb-abee-41f1-bdc6-fec6566d3557", 10}, // Starship_exploded_debris_06
			{"d88147fb-007c-481f-aa75-ebcbab24e4a8", 10}, // Starship_exploded_debris_19
			{"0c65ee6e-a84a-4989-a846-19eb53c13071",  3}, // Starship_exploded_debris_20
			{"72437ebc-7d61-49b8-bac4-cb7f3af3af8e", 12}, // Starship_exploded_debris_22
			{"67dd6f15-9ac5-4f87-b71a-ebd16f04f02b",  2}, // docking_menu_01
			{"ef1370e3-832f-4008-ac39-99ad24f43f76", 10}, // Starship_doors_door
			{"4e8f6009-fc9c-4774-9ddc-27a6b0081dde",  3}  // room_06_wreck					// special processing
		};

		public readonly PrefabsList debrisLockers = new PrefabsList()
		{
			{"bca9b19c-616d-4948-8742-9bb6f4296dc3",  3}, // submarine_locker_04_open
			{"779d4bbe-6e34-4ca5-bee5-b32d65288f5f",  1}, // submarine_locker_04_door
			{"078b41f8-968e-4ca3-8a7e-4e3d7d98422c", 10}, // submarine_locker_05			// special processing
		};

		public readonly PrefabsList debrisTech = new PrefabsList()
		{
			{"8ce870ba-b559-45d7-9c10-a5477967db24",  2}, // tech_light_deco				// special processing
			{"0f779340-8064-4308-8baa-6be9324a1e05",  3}, // Starship_tech_box_01_02		// special processing
			{"c5d27b10-b02e-4063-9819-584dbfb721fa",  1}, // Starship_tech_box_01_03
			{"386f311e-0d93-44cf-a180-f388820cb35b",  1}, // descent_trashcans_01			// special processing
			{"40e2a610-19dc-4ae8-b0c1-816230ab1ce3",  2}, // VendingMachine					// special processing
			//{"13d0fb01-2957-49e0-b153-6dc88332694c", 12}, // generic_forklift
			//{"38b89b53-2506-4f90-aaaa-2f0174e6425f", 10}, // submarine_engine_console_01,
		};

		public readonly PrefabsList debrisCargoClosed = new PrefabsList()
		{
			{"354ebf4e-def3-48a6-839d-bf0f478ca915",  2}, // Starship_cargo
			{"d21bca5e-6dd2-48d8-bbf0-2f1d5df7fa9c",  2}, // Starship_cargo_02
			{"8b43e753-29a6-4365-bc53-822376d1cfa2", 10}, // Starship_cargo_large
			{"cc14ee20-80c5-4573-ae1b-68bebc0feadf", 10}, // Starship_cargo_large_02
			{"65edb6a3-c1e6-4aaf-9747-108bd6a9dcc6",  2}, // Starship_cargo_damaged_01
			{"7646d66b-01c0-4110-b6bf-305df024c2b1",  2}, // Starship_cargo_damaged_02
			{"8ba3be30-d89f-474b-87ca-94d3bfff25a4", 10}, // Starship_cargo_damaged_large_01
			{"423ab63d-38e0-4dd8-ab8d-fcd6c9ff0759", 10}  // Starship_cargo_damaged_large_02
		};

		public readonly PrefabsList debrisFurniture = new PrefabsList()
		{
			{"af165b07-a2a3-4d85-8ad7-0c801334c115",  2}, // discovery_lab_cart_01
			{"04a07ec0-e3f4-4285-a087-688215fdb142",  3}, // Starship_work_desk_01_empty
			{"9460942c-2347-4b58-b9ff-0f7f693dc9ff",  3}, // Starship_work_desk_01
			{"2de0fc33-0386-4b55-84d4-6ad6bffaf74f",  3}, // Starship_work_desk_screen_01
			{"adcace2c-509e-429e-9d24-9760a2d58ff4",  2}, // Starship_work_chair_01
			{"cc0bc831-9cc1-4dac-8285-e0dc8ebb2dd9",  2}, // Starship_work_chair_02
			{"286b44fc-8d89-4c2f-8154-2400624ad259", 10}, // Starship_work_chair_04
			{"42eae67f-f31a-45a0-95bf-27e189de65a0", 10}, // biodome_lab_counter_01_cab1
			{"90148ef8-fda4-4a95-b2bc-d570543a1ecf",  2}, // descent_bar_table_01
			{"b79fb664-dd70-47ce-aa05-0a03a98cfb01",  2}, // descent_bar_seat_side_02
			{"2e9b9389-cfa3-45b1-aee8-ea66b90e841d", 10}, // Bench_deco
		};

#if !EXCLUDE_STATIC_DEBRIS
		public readonly PrefabsList debrisStatic = new PrefabsList()
		{
			{"a5f0e345-1e46-410f-8bf1-eeeed3e5a126", 10}, // Starship_exploded_debris_25
			{"1235093d-3e84-4e98-9823-602db2e8fa5f", 10}, // Starship_exploded_debris_12
			{"4e36dbfa-fb59-4aa1-a997-d5624d23a350", 10}, // starship_girder_06
			{"76825855-c939-48ae-812d-79b6d0529dd9", 10}, // ExplorableWreckHull02
			{"30fb51ee-73b6-4609-8e02-2804201987fb", 10}, // Starship_exploded_debris_13
			{"0c13f261-4093-47ff-a9ac-8750627ac8f7", 10}, // explorable_wreckage_modular_room_details_10
			{"afc1cadd-6441-43c9-8d58-3eddd3289af1", 10}, // starship_girder_03
			{"4322ded1-04ba-44eb-afe5-44b9c4112c64", 10}, // Starship_exploded_debris_05
			{"40cb0ae5-de47-4b18-9d1c-e572253afef4", 10}, // Starship_exploded_debris_18
			{"1c147fcd-f727-4404-b10e-a1f03363e5bf", 10}, // Starship_exploded_debris_28
			{"bdc7dc99-041a-4141-a673-f0ee0396c87e", 10}, // Starship_exploded_debris_14
			{"a05a52de-d6fc-44f5-a908-668a5c255aca", 10}, // Starship_exploded_debris_10
			{"ea9f43f5-373f-4276-8743-852fb8a2cb88", 10}, // Starship_exploded_debris_17
			{"ca66207a-ab0c-4974-80f2-abde941a2daa", 10}, // explorable_wreckage_modular_room_details_07
			{"114e12f4-58c6-4d1d-8fd5-3bff03bca912", 10}, // starship_girder_01
			{"d6d58541-ad9f-4686-b909-50b1a0f5835b", 10}, // Starship_exploded_debris_08
			{"669d26ab-81a0-4e4f-8bba-fac0d6cf8dab", 10}, // Starship_exploded_debris_04
			{"84870949-8029-4971-97f8-f1d740b45e13", 10}, // Starship_exploded_debris_27
			{"99c0da07-a612-4cb7-9e16-e2e6bd3d6207", 10}, // starship_girder_10
			{"256db677-a0c5-4dc1-aefc-59def4fa4663", 10}, // Starship_exploded_debris_26
			{"a7b4dc5f-6603-4f27-99e1-2586a9ea20a4", 10}, // explorable_wreckage_modular_room_details_14
			{"953d4aad-0e94-47e5-8909-9454011bd79b", 10}, // Starship_exploded_debris_02
			{"38cdbebf-a4c0-4235-8068-a1f752acee74", 10}, // vent_constructor_section_vertical_01
			{"06fd4196-3058-4843-bfbf-9ea1f4985321", 10}, // vent_constructor_junction_horizontal_01
			{"872c799a-4de2-4531-a846-3b362d666e0b", 10}, // explorable_wreckage_modular_wall_01
			{"11eaf4c6-8bf6-4c0a-a70e-2c6c87c9b4ff", 10}, // explorable_wreckage_modular_room_details_04
			{"aad81104-9f02-47ec-8095-e99ede823b90", 10}, // ExplorableWreckHull01
			{"07365b8b-4d3d-490c-9cbc-83af339a48e7", 10}, // explorable_wreckage_modular_room_details_08
			{"a3d7ddd0-bdcb-4d7c-ab00-3003c9245180", 10}, // explorable_wreckage_modular_room_details_05
			{"5cf44d20-ab07-4787-994b-35c2fd061959", 10}, // starship_girder_04
			{"7ec3cd94-4981-4877-be57-e7bfdfbbce00", 10}, // starship_girder_09
		};
#endif
	}
}