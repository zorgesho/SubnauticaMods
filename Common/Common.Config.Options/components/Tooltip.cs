using System;
using System.Collections.Generic;

using UnityEngine;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		public partial class Components
		{
			public class Tooltip: MonoBehaviour, ITooltip
			{
				// using TranslationLiveUpdate component instead of Text (same result in this case and we don't need to add reference to Unity UI)
				public static void addTo(GameObject gameObject, string _tooltip)
				{
					GameObject caption = gameObject.GetComponentInChildren<TranslationLiveUpdate>().gameObject;
					caption.AddComponent<Tooltip>().tooltip = _tooltip;
				}

				string tooltip;
				public void GetTooltip(out string tooltipText, List<TooltipIcon> _) =>
					tooltipText = LanguageHelper.str(tooltip);

				static readonly Type layoutElementType = ReflectionHelper.safeGetType("UnityEngine.UI", "UnityEngine.UI.LayoutElement");
				void Start() => Destroy(gameObject.GetComponent(layoutElementType));
			}
		}
	}
}