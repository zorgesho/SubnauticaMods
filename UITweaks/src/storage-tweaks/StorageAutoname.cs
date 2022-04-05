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
	class StorageAutoname: StorageContentsListener
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

		uGUI_SignInput sign;
		string labelText => container.count == 0? Language.main.Get("Empty"): getItems()[0].name;

		protected override void onContentsChanged()
		{
			if (tweakEnabled)
				sign.inputField.text = labelText.ToUpper();
		}

		void Start()
		{
			sign = GetComponent<IStorageLabel>()?.label.signInput;

			if (sign)
			{
				onContentsChanged();
			}
			else
			{
				$"StorageAutoname.Start: container {name} doesn't has label".logError();
				Destroy(this);
			}
		}
	}
}