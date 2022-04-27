using System;
using System.Linq;

using HarmonyLib;

using Common;
using Common.Harmony;
using Common.Reflection;

namespace UITweaks.StorageTweaks
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	class StorageHandlerAttribute: Attribute
	{
		readonly TechType techType;
		public string classId => CraftData.GetClassIdForTechType(techType);

		public StorageHandlerAttribute(TechType techType) => this.techType = techType;
	}

	static class StorageHandlerProcessor
	{
#if DEBUG
		static bool dbgDumpStorage = false;
		static int dbgDumpStorageParent = 0;

		public static void dbgDumpStorages(bool enabled, int dumpParent)
		{
			dbgDumpStorage = enabled;
			dbgDumpStorageParent = dumpParent;
		}
#endif
		static readonly ILookup<string, Type> handlersByClassId =
			ReflectionHelper.definedTypes.
			Where(type => type.Namespace == $"{nameof(UITweaks)}.{nameof(StorageTweaks)}").
			SelectMany(type => type.getAttrs<StorageHandlerAttribute>(), (type, attr) => (type, attr.classId)).
#if DEBUG
			forEach(pair => $"Storage handler added: {CraftData.entClassTechTable[pair.classId]} => {pair.type}".logDbg()).
#endif
			ToLookup(pair => pair.classId, pair => pair.type);

		public static bool hasHandlers(string classId) => handlersByClassId.Contains(classId);

		public static void ensureHandlers(StorageContainer container)
		{
			foreach (var handler in handlersByClassId[Utils.getPrefabClassId(container)])
				container.gameObject.ensureComponent(handler);
		}

		[OptionalPatch, PatchClass]
		static class Patches
		{
			static bool prepare() => Main.config.storageTweaks.enabled;

			[HarmonyPostfix, HarmonyPatch(typeof(StorageContainer), "Awake")]
			static void StorageContainer_Awake_Postfix(StorageContainer __instance)
			{
				ensureHandlers(__instance);
#if DEBUG
				if (dbgDumpStorage)
					__instance.gameObject.dump(dumpParent: dbgDumpStorageParent);
#endif
			}
		}
	}
}