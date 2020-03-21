using System;
using System.Collections.Generic;

using UnityEngine;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		public static partial class Components
		{
			#region base tooltip
			public class Tooltip: MonoBehaviour, ITooltip
			{
				public class Add: ModOption.IOnGameObjectChangeHandler
				{
					string tooltip;
					readonly Type tooltipCmpType;

					public Add(string _tooltip)
					{
						tooltip = _tooltip;
					}
					public Add(Type _tooltipCmpType, string _tooltip): this(_tooltip)
					{
						tooltipCmpType = _tooltipCmpType;

						Debug.assert(tooltipCmpType == null || typeof(Tooltip).IsAssignableFrom(tooltipCmpType),
							$"Tooltip type {tooltipCmpType} is not derived from Options.Components.Tooltip");
					}

					public void init(ModOption option)
					{
						if (tooltip != null)
							registerLabel(option.id + ".tooltip", ref tooltip, false);
					}

					public void handle(GameObject gameObject)
					{
						// using TranslationLiveUpdate component instead of Text (same result in this case and we don't need to add reference to Unity UI)
						GameObject caption = gameObject.GetComponentInChildren<TranslationLiveUpdate>().gameObject;

						Type cmpType = tooltipCmpType ?? typeof(Tooltip);
						(caption.AddComponent(cmpType) as Tooltip).tooltip = tooltip;
					}
				}


				public virtual string tooltip
				{
					get => _tooltip;
					set => _tooltip = value;
				}
				protected string _tooltip;

				protected virtual string getTooltip() => tooltip;

				public void GetTooltip(out string tooltipText, List<TooltipIcon> _) => tooltipText = getTooltip();

				static readonly Type layoutElementType = ReflectionHelper.safeGetType("UnityEngine.UI", "UnityEngine.UI.LayoutElement");
				void Start()
				{
					Destroy(gameObject.GetComponent(layoutElementType)); // for removing empty space after label

					tooltip = LanguageHelper.str(_tooltip); // using field, not property
				}
			}
			#endregion

			#region tooltip with cache
			public abstract class TooltipCached: Tooltip // to avoid creating strings on each frame
			{
				protected abstract bool needUpdate { get; }

				string tooltipCached;
				protected sealed override string getTooltip() => needUpdate? (tooltipCached = tooltip): tooltipCached;
			}

			public abstract class TooltipCached<T1>: TooltipCached where T1: struct
			{
				T1? param1 = null;

				protected bool isParamsChanged(T1 _param1)
				{
					if (_param1.Equals(param1))
						return false;

					param1 = _param1;
					return true;
				}
			}

			public abstract class TooltipCached<T1, T2>: TooltipCached where T1: struct where T2: struct
			{
				T1? param1 = null;
				T2? param2 = null;

				protected bool isParamsChanged(T1 _param1, T2 _param2)
				{
					if (_param1.Equals(param1) && _param2.Equals(param2))
						return false;

					param1 = _param1;
					param2 = _param2;
					return true;
				}
			}
			#endregion
		}
	}
}