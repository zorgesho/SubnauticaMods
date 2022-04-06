using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using UnityEngine;

using Common;
using Common.GameSerialization;

namespace UITweaks.StorageTweaks
{
	partial class StorageAutoname
	{
		class Serializer: MonoBehaviour
		{
			const string saveName = "storage-autoname";

			class SaveData
			{
				public HashSet<string> storages;
			}

			[SuppressMessage("", "IDE0052")]
			static GameObject go;

			SaveLoadHelper helper;

			public static void init()
			{
				go ??= UnityHelper.createPersistentGameObject<Serializer>("UITweaks.Serializer");
			}

			void Awake()
			{																									"StorageAutoname.Serializer: Awake".logDbg();
				helper = new SaveLoadHelper(onLoad, onSave);
			}

			void Update()
			{
				helper.update();
			}

			void onLoad()
			{																									"StorageAutoname.Serializer: onLoad".logDbg();
				managedStorages = SaveLoad.load<SaveData>(saveName)?.storages ?? new();
			}

			void onSave()
			{																									"StorageAutoname.Serializer: onSave".logDbg();
				managedStorages.RemoveWhere(id => !UniqueIdentifier.TryGetIdentifier(id, out _));
				SaveLoad.save(saveName, new SaveData { storages = managedStorages });
			}
		}
	}
}