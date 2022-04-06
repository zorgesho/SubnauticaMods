using System.Collections.Generic;

using Common;
using Common.Configuration;

namespace UITweaks.StorageTweaks
{
	[StorageHandler(TechType.SmallLocker)]
	[StorageHandler(TechType.SmallStorage)]
#if GAME_BZ
	[StorageHandler(TechType.SeaTruckStorageModule)]
	[StorageHandler(TechType.SeaTruckFabricatorModule)]
#endif
	partial class StorageAutoname: StorageContentsListener
	{
		static bool tweakEnabled => Main.config.storageTweaks.enabled && Main.config.storageTweaks.autoname;

		public class UpdateLabels: Config.Field.IAction
		{
			public void action()
			{
				if (tweakEnabled)
					UnityHelper.FindObjectsOfTypeAll<StorageAutoname>().forEach(info => info.onContentsChanged());
			}
		}

		// IDs of storages that are allowed to be auto-named (goes to a save)
		static HashSet<string> managedStorages = new();

		static void manageStorage(string id, bool manage)
		{
			if (manage)
			{																								$"StorageAutoname: enabling auto-naming for storage (id: {id})".logDbg();
				managedStorages.Add(id);
			}
			else
			{																								$"StorageAutoname: disabling auto-naming for storage (id: {id})".logDbg();
				managedStorages.Remove(id);
			}
		}

		string storageID;
		uGUI_SignInput sign;

		string labelText => container.count == 0? Language.main.Get("Empty"): getItems()[0].name;
		bool shouldAutoname => sign.text == "" || sign.text == Language.main.Get(sign.stringDefaultLabel);

		protected override void onContentsChanged()
		{
			if (!sign || !tweakEnabled) // 'sign' can be null if we here before 'Start' (e.g. during loading)
				return;

			if (!managedStorages.Contains(storageID))
			{
				if (!shouldAutoname)
					return;

				manageStorage(storageID, true);
			}

			sign.inputField.text = labelText.ToUpper();
		}

		void Start()
		{
			sign = GetComponent<IStorageLabel>()?.label.signInput;
			storageID = GetComponent<StorageContainer>()?.storageRoot?.Id;

			if (!sign || storageID == null)
			{
				$"StorageAutoname.Start: container {name} is invalid (sign: '{sign}', storageID: '{storageID}')".logError();
				Destroy(this);
			}
		}
	}
}