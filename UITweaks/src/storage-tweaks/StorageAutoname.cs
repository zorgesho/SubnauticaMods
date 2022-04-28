#if DEBUG
//#define LABEL_TEST
#endif

using System.Text;
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
		public static bool tweakEnabled => StorageLabelFixers.tweakEnabled && Main.config.storageTweaks.autoname;

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

		string labelText
		{
			get
			{
#if LABEL_TEST
				return Utils.TechTypeNamesTest.getName();
#else
				if (container.count == 0)
					return Language.main.Get("Empty");

				var items = getItems();
				StringBuilder sb = new();

				for (int i = 0; i < items.Count && i < Main.config.storageTweaks.autonameMaxItemCount; i++)
					sb.Append($"{items[i].name}, ");

				return sb.removeFromEnd(2).ToString();
#endif
			}
		}

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