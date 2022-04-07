using System;
using System.Collections;

using UnityEngine;

namespace Common.GameSerialization
{
	class SaveLoadHelper
	{
		bool handledLoad, handledSave;
		readonly Action onLoad, onSave;

		public SaveLoadHelper(Action onLoad, Action onSave)
		{
			this.onLoad = onLoad;
			this.onSave = onSave;
		}

		public void update()
		{
			if (!SaveLoadManager.main)
				return;

			if (onLoad != null)
			{
				if (SaveLoadManager.main.isLoading && !handledLoad)
				{
					handledLoad = true;
					UnityHelper.startCoroutine(_load());
				}

				IEnumerator _load()
				{																							"SaveLoadHelper: load coroutine started".logDbg();
					// wait while files are copied from the save slot to the temporary path
					yield return new WaitWhile(() => SaveLoadManager.main.isLoading);

					if (!SaveLoadManager.GetTemporarySavePath().isNullOrEmpty())
					{																						"SaveLoadHelper: onLoad".logDbg();
						onLoad();
					}

					handledLoad = false;
				}
			}

			if (onSave != null)
			{
				if (!SaveLoadManager.main.isSaving)
				{
					handledSave = false;
				}
				else if (!handledSave)
				{																							"SaveLoadHelper: onSave".logDbg();
					handledSave = true;
					onSave();
				}
			}
		}
	}
}