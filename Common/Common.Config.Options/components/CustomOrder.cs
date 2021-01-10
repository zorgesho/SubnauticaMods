using System;
using UnityEngine;

namespace Common.Configuration
{
	partial class Options
	{
		// for use on config class (for moving all mod's options before specified mod in the options tab)
		[AttributeUsage(AttributeTargets.Class)]
		public class CustomOrderAttribute: Attribute, Config.IConfigAttribute
		{
			public readonly string modIDBefore;
			public CustomOrderAttribute(string modIDBefore) => this.modIDBefore = modIDBefore;

			public void process(object config) => Debug.assert(config is Config); // just in case
		}

		partial class Factory
		{
			class CustomOrderModifier: IModifier
			{
				public void process(ModOption option)
				{
					if (option.cfgField.getAttr<CustomOrderAttribute>(true) is CustomOrderAttribute customOrder)
						option.addHandler(new CustomOrderHandler(customOrder.modIDBefore));
				}
			}
		}

		public class CustomOrderHandler: ModOption.IOnGameObjectChangeHandler
		{
			static int targetIndex;
			static int optionIndexLast;

			int optionIndex = -1;

			readonly string modIDBefore;
			public CustomOrderHandler(string modIDBefore) => this.modIDBefore = modIDBefore;

			public void init(ModOption option) {}

			public void handle(GameObject gameObject)
			{
				if (optionIndex == -1)
					optionIndex = optionIndexLast++;

				var got = gameObject.transform;

				if (optionIndex == 0)
				{
					targetIndex = -1;

					// searching for target heading (each time we open options, just in case)
					foreach (Transform option in got.parent.transform)
					{
						if (option.gameObject.GetComponentInChildren<TranslationLiveUpdate>().translationKey.Contains(modIDBefore))
						{
							targetIndex = option.GetSiblingIndex();
							break;
						}
					}

					Debug.assert(targetIndex != -1);

					// moving options heading (if this is the first option, then the previous sibling is heading)
					if (targetIndex != -1)
						got.parent.GetChild(got.GetSiblingIndex() - 1).gameObject.transform.SetSiblingIndex(targetIndex);
				}

				if (targetIndex != -1)
					got.SetSiblingIndex(targetIndex + 1 + optionIndex);
			}
		}
	}
}