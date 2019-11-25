using System;

namespace Common.GameSerialization
{
	class SaveLoadHelper
	{
		bool handledLoad, handledSave;
		readonly Action onLoad, onSave;

		public SaveLoadHelper(Action _onLoad, Action _onSave)
		{
			onLoad = _onLoad;
			onSave = _onSave;
		}

		public void update()
		{
			if (onLoad != null)
			{
				if (!SaveLoadManager.main.isLoading)
					handledLoad = false;
				else
				if (!handledLoad)
				{
					handledLoad = true;

					if (!SaveLoadManager.GetTemporarySavePath().isNullOrEmpty())
						onLoad();
				}
			}

			if (onSave != null)
			{
				if (!SaveLoadManager.main.isSaving)
					handledSave = false;
				else
				if (!handledSave)
				{
					handledSave = true;
					onSave();
				}
			}
		}
	}
}