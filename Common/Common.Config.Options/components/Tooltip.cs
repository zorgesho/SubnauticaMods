using System;
using System.Collections.Generic;

using UnityEngine;

namespace Common.Configuration
{
	partial class Options
	{
		partial class Factory
		{
			class TooltipModifier: IModifier
			{
				public void process(ModOption option)
				{
					if (option.cfgField.getAttr<FieldAttribute>() is FieldAttribute fieldAttr)
					{
						if (fieldAttr.tooltipType != null || fieldAttr.tooltip != null)
							option.addHandler(new Components.Tooltip.Add(fieldAttr.tooltipType, fieldAttr.tooltip));
					}
				}
			}

			class HeadingTooltipModifier: IModifier
			{
				static bool processed = false;

				public void process(ModOption option)
				{
					if (processed || !(processed = true)) // process only the first added option
						return;

					Debug.assert(instance == null); // if this the first option, ModOptions shouldn't be created yet

					if (option.cfgField.getAttr<NameAttribute>(true) is NameAttribute nameAttr)
					{
						if (nameAttr.tooltipType != null || nameAttr.tooltip != null)
							option.addHandler(new Components.Tooltip.AddToHeading(nameAttr.tooltipType, nameAttr.tooltip));
					}
				}
			}
		}


		public static partial class Components
		{
			#region base tooltip
			public class Tooltip: MonoBehaviour, ITooltip
			{
				const int defaultTextSize = 19;

				public class Add: ModOption.IOnGameObjectChangeHandler
				{
					string tooltip;
					readonly Type tooltipCmpType;

					static readonly UniqueIDs uniqueIDs = new UniqueIDs();

					ModOption parentOption;

					public Add(string tooltip)
					{
						this.tooltip = tooltip;
					}
					public Add(Type tooltipCmpType, string tooltip): this(tooltip)
					{
						this.tooltipCmpType = tooltipCmpType;

						Debug.assert(tooltipCmpType == null || typeof(Tooltip).IsAssignableFrom(tooltipCmpType),
							$"Tooltip type {tooltipCmpType} is not derived from Options.Components.Tooltip");
					}

					public void init(ModOption option)
					{
						parentOption = option;

						if (tooltip == null)
							return;

						// adjust text size for default tooltip (before we registering string with LanguageHelper)
						if (tooltipCmpType == null)
							tooltip = $"<size={defaultTextSize}>" + tooltip + "</size>";

						string stringID = option.id + ".tooltip";
						uniqueIDs.ensureUniqueID(ref stringID); // in case we add more than one tooltip to the option (e.g. for heading)

						registerLabel(stringID, ref tooltip, false);
					}

					protected virtual GameObject getTargetGameObject(GameObject optionGameObject) => optionGameObject;

					public void handle(GameObject gameObject)
					{
						GameObject targetGameObject = getTargetGameObject(gameObject);

						// using TranslationLiveUpdate component instead of Text (same result in this case and we don't need to add reference to Unity UI)
						GameObject caption = targetGameObject.GetComponentInChildren<TranslationLiveUpdate>().gameObject;

						Type cmpType = tooltipCmpType ?? typeof(Tooltip);
						(caption.AddComponent(cmpType) as Tooltip).init(tooltip, parentOption);
					}
				}

				// for addind tooltip to the options heading
				// warning: supposed to be used on the first added option only
				public class AddToHeading: Add
				{
					public AddToHeading(Type tooltipCmpType, string tooltip): base(tooltipCmpType, tooltip) {}

					protected override GameObject getTargetGameObject(GameObject optionGameObject)
					{
						int index = optionGameObject.transform.GetSiblingIndex();
						Debug.assert(index > 0);

						return optionGameObject.transform.parent.GetChild(index - 1).gameObject;
					}
				}

				void init(string tooltip, ModOption parentOption)
				{
					this.tooltip = tooltip;
					this.parentOption = parentOption;
				}

				protected ModOption parentOption;

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

				protected bool isParamsChanged(T1 param1)
				{
					if (Equals(this.param1, param1))
						return false;

					this.param1 = param1;
					return true;
				}
			}

			public abstract class TooltipCached<T1, T2>: TooltipCached where T1: struct where T2: struct
			{
				T1? param1 = null;
				T2? param2 = null;

				protected bool isParamsChanged(T1 param1, T2 param2)
				{
					if (Equals(this.param1, param1) && Equals(this.param2, param2))
						return false;

					this.param1 = param1;
					this.param2 = param2;
					return true;
				}
			}
			#endregion
		}
	}
}