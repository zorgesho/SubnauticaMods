using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using Common;
using Common.Configuration;

namespace DebrisRecycling
{
	class L10n: LanguageHelper
	{
		public const string ids_smallScrapName = "Small piece of salvage";
		public const string ids_restartGame = "<color=yellow>Restart the game to apply changes.</color>";

		public static string ids_salvageableDebris = "Salvageable debris";
		public static string ids_tryMoveObject = "Try to move object"; // probably not used very often
		public static string ids_customDebrisAdded = "<color=#adf8ffff><b>{0}</b></color> is added as salvageable debris";
	}

#if DEBUG
	[AddToConsole("dr")]
#endif
	class ModConfig: Config
	{
		public class CraftConfig
		{
			[Options.Field("Dynamic recipe for titanium", "Dynamic recipe for titanium includes all scrap in the inventory.\n" + L10n.ids_restartGame)]
			public readonly bool dynamicTitaniumRecipe = true;

			public readonly int titaniumPerBigScrap = 4;
			public readonly int titaniumPerSmallScrap = 1;
		}
		public readonly CraftConfig craftConfig = new CraftConfig();

		[Options.Field("Show debris in the Scanner Room", L10n.ids_restartGame)]
		public readonly bool addDebrisToScannerRoom = true;

		public class CustomObjects
		{
			public readonly bool addToOptionsMenu =
#if DEBUG
				true;
#else
				false;
#endif
			public readonly int defaultResourceCount = 1;

			class Hider: Options.Components.Hider.IVisibilityChecker
			{ public bool visible => Main.config.customObjects.addToOptionsMenu; }

			class HotKeysHider: Options.Components.Hider.Simple
			{ public HotKeysHider(): base("hotkeys", () => Main.config.customObjects.addToOptionsMenu && Main.config.customObjects.hotkeysEnabled) {} }

			[Options.Field("Allow to add custom debris",
								"Allows to mark most of the objects as salvageab-\nle debris.\n" +
								"To mark an object point the <b>Habitat Builder</b> to it and press the corresponding hotkey.\n" +
								"Deconstructing some of the objects can cause various bugs. <color=yellow>Use at your own risk!</color>")]
			[Options.Hideable(typeof(Hider))]
			[Field.Action(typeof(HotKeysHider))]
			public readonly bool hotkeysEnabled = false;

			[Options.Field("\tMark object as debris",
								"Press to add an object to the <b>permanent</b> custom list in the <b>" + Main.prefabsConfigName + "</b>.")]
			[Options.Hideable(typeof(HotKeysHider), "hotkeys")]
			public readonly KeyCode hotkey = KeyCode.PageUp;

			[Options.Field("\tMark object as debris (temp.)",
								"Press to add an object to the <b>temporary</b> custom list in the <b>" + Main.prefabsConfigName + "</b> " +
								"(cleared at the beginning of each game session).")]
			[Options.Hideable(typeof(HotKeysHider), "hotkeys")]
			public readonly KeyCode hotkeyTemp = KeyCode.PageDown;
		}
		public readonly CustomObjects customObjects = new CustomObjects();

		public readonly bool deconstructValidStaticObjects = true;
		public readonly bool patchStaticObjects = true;
		public readonly bool extraPowerConsumption = false;
		public readonly bool fixLandscapeCollisions = true;
	};


	class PrefabsConfig: Config
	{
		public class PrefabList
		{
			class RefreshPrefabs: Field.IAction
			{ public void action() => DebrisPatcher.refreshValidPrefabs(Options.mode == Options.Mode.IngameMenu); }

			[Options.FinalizeAction(typeof(RefreshPrefabs))]
			public bool enabled = true;

			[NoInnerFieldsAttrProcessing]
			readonly Dictionary<string, int> prefabs; // key = prefab id, value = ScrapMetal * 10 + ScrapMetalSmall

			public PrefabList(bool enabled, Dictionary<string, int> prefabs)
			{
				this.enabled = enabled;
				this.prefabs = prefabs;
			}

			public bool empty => prefabs.Count == 0;
			public void clear() => prefabs.Clear();

			public string addPrefab(string prefabID, int resourceCount) // resourceCount: see above ^
			{
				UWE.PrefabDatabase.TryGetPrefabFilename(prefabID, out string prefabPath);
				prefabPath ??= prefabs.FirstOrDefault(p => p.Key.Contains(prefabID)).Key?.Split('.')[0] ?? "undefined";

				string prefabName = prefabPath.Substring(prefabPath.LastIndexOf("/") + 1);
				prefabs[$"{prefabName}.{prefabID}"] = resourceCount;

				return prefabName;
			}

			public void copyPrefabsTo(Dictionary<string, int> validPrefabs)
			{
				prefabs.ForEach(prefabInfo => validPrefabs[prefabInfo.Key.Split('.').ElementAtOrDefault(1) ?? prefabInfo.Key] = prefabInfo.Value);
			}
		}

		class AddPrefabListAttribute: Attribute, IFieldAttribute, IRootConfigInfo
		{
			class VisChecker: Options.Components.Hider.IVisibilityChecker
			{
				readonly PrefabList parentPrefabList;
				public VisChecker(PrefabList parentPrefabList) => this.parentPrefabList = parentPrefabList;

				public bool visible => !parentPrefabList.empty;
			}

			readonly string label;

			public AddPrefabListAttribute(string label = null) => this.label = label;

			PrefabsConfig rootConfig;
			public void setRootConfig(Config config) => rootConfig = config as PrefabsConfig;

			public void process(object config, FieldInfo field)
			{
				PrefabList prefabs = field.GetValue(config) as PrefabList;
				Common.Debug.assert(prefabs != null);

				rootConfig.allLists.Add(prefabs);

				if (label == null)
					return;

				var cfgField = new Field(prefabs, nameof(PrefabList.enabled), rootConfig);
				var option = new Options.ToggleOption(cfgField, label);
				option.addHandler(new Options.Components.Hider.Add(new VisChecker(prefabs)));

				Options.add(option);
			}
		}

		[NonSerialized]
		[NoInnerFieldsAttrProcessing]
		readonly List<PrefabList> allLists = new List<PrefabList>();

		public Dictionary<string, int> getValidPrefabs()
		{
			var validPrefabs = new Dictionary<string, int>();
			allLists.Where(list => list.enabled).ForEach(list => list.copyPrefabsTo(validPrefabs));

			return validPrefabs;
		}

		protected override void onLoad()
		{
			dbsCustomTemp.clear();
			dbsCustomTemp.enabled = true;

			_updateTo110();
		}

#if DEBUG
		[AddPrefabList("<color=#a0a0a0ff>debrisCargoOpened</color>")]
#else
		[AddPrefabList]
#endif
		public readonly PrefabList dbsCargoOpened = new PrefabList(true, new Dictionary<string, int>()
		{
			{"Starship_cargo_opened.c390fcfc-3bf4-470a-93bf-39dafb8b2267", 2},
			{"Starship_cargo_damaged_opened_01.8c3d54c0-4330-4949-91ad-f046cfd67c7c", 2},
			{"Starship_cargo_damaged_opened_02.ebc835bd-221a-4722-b1d0-becf08bd2f2c", 2},
			{"Starship_cargo_opened_large.af413920-4fe6-4447-9f62-4f04e605d6be", 10},
			{"Starship_cargo_damaged_opened_large_01.a2104a9e-fe84-4c51-8874-69350507ef98", 10},
			{"Starship_cargo_damaged_opened_large_02.fb2886c4-7e03-4a47-a122-dc7242e7de5b", 10},
		});

#if DEBUG
		[AddPrefabList("<color=#a0a0a0ff>debrisMiscMovable</color>")]
#else
		[AddPrefabList]
#endif
		public readonly PrefabList dbsMiscMovable = new PrefabList(true, new Dictionary<string, int>()
		{
			{"Starship_exploded_debris_01.5cd34124-935f-4628-b694-a266bc2f5517", 11},
			{"Starship_exploded_debris_06.df36cdfb-abee-41f1-bdc6-fec6566d3557", 10},
			{"Starship_exploded_debris_19.d88147fb-007c-481f-aa75-ebcbab24e4a8", 10},
			{"Starship_exploded_debris_20.0c65ee6e-a84a-4989-a846-19eb53c13071", 3},
			{"Starship_exploded_debris_22.72437ebc-7d61-49b8-bac4-cb7f3af3af8e", 12},
			{"docking_menu_01.67dd6f15-9ac5-4f87-b71a-ebd16f04f02b", 2},
			{"Starship_doors_door.ef1370e3-832f-4008-ac39-99ad24f43f76", 10},
			{"room_06_wreck.4e8f6009-fc9c-4774-9ddc-27a6b0081dde", 3},					// special processing
		});

		[AddPrefabList("Deconstruct closed crates")]
		public readonly PrefabList dbsCargoClosed = new PrefabList(true, new Dictionary<string, int>()
		{
			{"Starship_cargo.354ebf4e-def3-48a6-839d-bf0f478ca915", 2},
			{"Starship_cargo_02.d21bca5e-6dd2-48d8-bbf0-2f1d5df7fa9c", 2},
			{"Starship_cargo_large.8b43e753-29a6-4365-bc53-822376d1cfa2", 10},
			{"Starship_cargo_large_02.cc14ee20-80c5-4573-ae1b-68bebc0feadf", 10},
			{"Starship_cargo_damaged_01.65edb6a3-c1e6-4aaf-9747-108bd6a9dcc6", 2},
			{"Starship_cargo_damaged_02.7646d66b-01c0-4110-b6bf-305df024c2b1", 2},
			{"Starship_cargo_damaged_large_01.8ba3be30-d89f-474b-87ca-94d3bfff25a4", 10},
			{"Starship_cargo_damaged_large_02.423ab63d-38e0-4dd8-ab8d-fcd6c9ff0759", 10},
		});

		[AddPrefabList("Deconstruct lockers")]
		public readonly PrefabList dbsLockers = new PrefabList(true, new Dictionary<string, int>()
		{
			{"submarine_locker_04_open.bca9b19c-616d-4948-8742-9bb6f4296dc3", 3},
			{"submarine_locker_04_door.779d4bbe-6e34-4ca5-bee5-b32d65288f5f", 1},
			{"submarine_locker_05.078b41f8-968e-4ca3-8a7e-4e3d7d98422c", 10},			// special processing
		});

		[AddPrefabList("Deconstruct furniture")]
		public readonly PrefabList dbsFurniture = new PrefabList(false, new Dictionary<string, int>()
		{
			{"discovery_lab_cart_01.af165b07-a2a3-4d85-8ad7-0c801334c115", 2},
			{"Starship_work_desk_01_empty.04a07ec0-e3f4-4285-a087-688215fdb142", 3},
			{"Starship_work_desk_01.9460942c-2347-4b58-b9ff-0f7f693dc9ff", 3},
			{"Starship_work_desk_screen_01.2de0fc33-0386-4b55-84d4-6ad6bffaf74f", 3},
			{"Starship_work_chair_01.adcace2c-509e-429e-9d24-9760a2d58ff4", 2},
			{"Starship_work_chair_02.cc0bc831-9cc1-4dac-8285-e0dc8ebb2dd9", 2},
			{"Starship_work_chair_04.286b44fc-8d89-4c2f-8154-2400624ad259", 10},
			{"biodome_lab_counter_01_cab1.42eae67f-f31a-45a0-95bf-27e189de65a0", 10},
			{"descent_bar_table_01.90148ef8-fda4-4a95-b2bc-d570543a1ecf", 2},
			{"descent_bar_seat_side_02.b79fb664-dd70-47ce-aa05-0a03a98cfb01", 2},
			{"Bench_deco.2e9b9389-cfa3-45b1-aee8-ea66b90e841d", 10},
		});

#if DEBUG
		[AddPrefabList("<color=#a0a0a0ff>debrisTech</color>")]
#else
		[AddPrefabList]
#endif
		public readonly PrefabList dbsTech = new PrefabList(false, new Dictionary<string, int>()
		{
			{"tech_light_deco.8ce870ba-b559-45d7-9c10-a5477967db24", 2},				// special processing
			{"Starship_tech_box_01_02.0f779340-8064-4308-8baa-6be9324a1e05", 3},		// special processing
			{"Starship_tech_box_01_03.c5d27b10-b02e-4063-9819-584dbfb721fa", 1},
			{"descent_trashcans_01.386f311e-0d93-44cf-a180-f388820cb35b", 1},			// special processing
			{"VendingMachine.40e2a610-19dc-4ae8-b0c1-816230ab1ce3", 2},					// special processing
			//{"generic_forklift.13d0fb01-2957-49e0-b153-6dc88332694c", 12},
			//{"submarine_engine_console_01.38b89b53-2506-4f90-aaaa-2f0174e6425f", 10},
		});

#if DEBUG
		[AddPrefabList("<color=#a0a0a0ff>debrisStatic</color>")]
		public readonly PrefabList dbsStatic = new PrefabList(false, new Dictionary<string, int>()
		{
			{"Starship_exploded_debris_25.a5f0e345-1e46-410f-8bf1-eeeed3e5a126", 10},
			{"Starship_exploded_debris_12.1235093d-3e84-4e98-9823-602db2e8fa5f", 10},
			{"starship_girder_06.4e36dbfa-fb59-4aa1-a997-d5624d23a350", 10},
			{"ExplorableWreckHull02.76825855-c939-48ae-812d-79b6d0529dd9", 10},
			{"Starship_exploded_debris_13.30fb51ee-73b6-4609-8e02-2804201987fb", 10},
			{"explorable_wreckage_modular_room_details_10.0c13f261-4093-47ff-a9ac-8750627ac8f7", 10},
			{"starship_girder_03.afc1cadd-6441-43c9-8d58-3eddd3289af1", 10},
			{"Starship_exploded_debris_05.4322ded1-04ba-44eb-afe5-44b9c4112c64", 10},
			{"Starship_exploded_debris_18.40cb0ae5-de47-4b18-9d1c-e572253afef4", 10},
			{"Starship_exploded_debris_28.1c147fcd-f727-4404-b10e-a1f03363e5bf", 10},
			{"Starship_exploded_debris_14.bdc7dc99-041a-4141-a673-f0ee0396c87e", 10},
			{"Starship_exploded_debris_10.a05a52de-d6fc-44f5-a908-668a5c255aca", 10},
			{"Starship_exploded_debris_17.ea9f43f5-373f-4276-8743-852fb8a2cb88", 10},
			{"explorable_wreckage_modular_room_details_07.ca66207a-ab0c-4974-80f2-abde941a2daa", 10},
			{"starship_girder_01.114e12f4-58c6-4d1d-8fd5-3bff03bca912", 10},
			{"Starship_exploded_debris_08.d6d58541-ad9f-4686-b909-50b1a0f5835b", 10},
			{"Starship_exploded_debris_04.669d26ab-81a0-4e4f-8bba-fac0d6cf8dab", 10},
			{"Starship_exploded_debris_27.84870949-8029-4971-97f8-f1d740b45e13", 10},
			{"starship_girder_10.99c0da07-a612-4cb7-9e16-e2e6bd3d6207", 10},
			{"Starship_exploded_debris_26.256db677-a0c5-4dc1-aefc-59def4fa4663", 10},
			{"explorable_wreckage_modular_room_details_14.a7b4dc5f-6603-4f27-99e1-2586a9ea20a4", 10},
			{"Starship_exploded_debris_02.953d4aad-0e94-47e5-8909-9454011bd79b", 10},
			{"vent_constructor_section_vertical_01.38cdbebf-a4c0-4235-8068-a1f752acee74", 10},
			{"vent_constructor_junction_horizontal_01.06fd4196-3058-4843-bfbf-9ea1f4985321", 10},
			{"explorable_wreckage_modular_wall_01.872c799a-4de2-4531-a846-3b362d666e0b", 10},
			{"explorable_wreckage_modular_room_details_04.11eaf4c6-8bf6-4c0a-a70e-2c6c87c9b4ff", 10},
			{"ExplorableWreckHull01.aad81104-9f02-47ec-8095-e99ede823b90", 10},
			{"explorable_wreckage_modular_room_details_08.07365b8b-4d3d-490c-9cbc-83af339a48e7", 10},
			{"explorable_wreckage_modular_room_details_05.a3d7ddd0-bdcb-4d7c-ab00-3003c9245180", 10},
			{"starship_girder_04.5cf44d20-ab07-4787-994b-35c2fd061959", 10},
			{"starship_girder_09.7ec3cd94-4981-4877-be57-e7bfdfbbce00", 10},
		});
#endif
		[AddPrefabList("Deconstruct custom objects")]
		public readonly PrefabList dbsCustom = new PrefabList(true, new Dictionary<string, int>());

		[AddPrefabList("Deconstruct temporary custom objects")]
		public readonly PrefabList dbsCustomTemp = new PrefabList(true, new Dictionary<string, int>());


		#region v1.0.0 -> v1.1.0
		int __cfgVer = 0;

		void _updateTo110()
		{
			if (__cfgVer >= 110)
				return;

			try
			{
				foreach (var name in new string[] {"CargoOpened", "MiscMovable", "Lockers", "Tech", "CargoClosed", "Furniture"})
				{
					var oldList = GetType().field("debris" + name).GetValue(this) as Dictionary<string, int>;
					var newList = GetType().field("dbs" + name).GetValue(this) as PrefabList;

					oldList.ForEach(debrisInfo => newList.addPrefab(debrisInfo.Key, debrisInfo.Value));
				}
			}
			catch (Exception e) { Log.msg(e); }

			__cfgVer = 110;
		}

#pragma warning disable IDE0044, IDE0052
		[Field.LoadOnly] [NoInnerFieldsAttrProcessing] Dictionary<string, int> debrisCargoOpened = new Dictionary<string, int>();
		[Field.LoadOnly] [NoInnerFieldsAttrProcessing] Dictionary<string, int> debrisMiscMovable = new Dictionary<string, int>();
		[Field.LoadOnly] [NoInnerFieldsAttrProcessing] Dictionary<string, int> debrisLockers	 = new Dictionary<string, int>();
		[Field.LoadOnly] [NoInnerFieldsAttrProcessing] Dictionary<string, int> debrisTech		 = new Dictionary<string, int>();
		[Field.LoadOnly] [NoInnerFieldsAttrProcessing] Dictionary<string, int> debrisCargoClosed = new Dictionary<string, int>();
		[Field.LoadOnly] [NoInnerFieldsAttrProcessing] Dictionary<string, int> debrisFurniture	 = new Dictionary<string, int>();
#pragma warning restore
		#endregion
	}
}