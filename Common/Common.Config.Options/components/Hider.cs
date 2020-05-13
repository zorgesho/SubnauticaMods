using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Common.Configuration
{
	partial class Options
	{
		partial class Factory
		{
			class HideableModifier: IModifier
			{
				public void process(ModOption option)
				{
					if (option.cfgField.getAttr<HideableAttribute>(true) is HideableAttribute hideableAttr)
					{
						string groupID = hideableAttr.groupID;

						if (groupID == null)
							option.cfgField.getAttrs<HideableAttribute>(true).forEach(attr => groupID ??= attr.groupID);

						option.addHandler(new Components.Hider.Add(hideableAttr.visChecker, groupID));
					}
				}
			}
		}


		public static partial class Components
		{
			// component for hiding options elements
			// we need separate component for this to avoid conflicts with toggleable option's headings
			public class Hider: MonoBehaviour
			{
				public interface IVisibilityChecker { bool visible { get; } }

				// for use with class targeted Hideable attribute
				public class Ignore: IVisibilityChecker
				{
					public bool visible => true;
				}

				public class Simple: Config.Field.IAction, IVisibilityChecker
				{
					readonly string groupID;
					readonly Func<bool> visChecker;

					public Simple(string groupID, Func<bool> visChecker)
					{
						this.groupID = groupID;
						this.visChecker = visChecker;
					}

					public bool visible => visChecker();
					public void action() => setVisible(groupID, visible);
				}

				public class Add: ModOption.IOnGameObjectChangeHandler
				{
					string id;
					readonly string groupID;
					readonly IVisibilityChecker visChecker;

					public void init(ModOption option) => id = option.id;

					public Add(IVisibilityChecker visChecker, string groupID = null)
					{
						this.visChecker = visChecker;
						this.groupID = groupID;
					}

					public void handle(GameObject gameObject) =>
						gameObject.AddComponent<Hider>().init(id, groupID, visChecker);
				}

				string id, groupID;
				IVisibilityChecker visChecker;

				bool visible = true;

				void init(string id, string groupID, IVisibilityChecker visChecker)
				{
					this.id = id;
					this.groupID = groupID;
					this.visChecker = visChecker;
				}

				static readonly List<Hider> hiders = new List<Hider>();

				static void register(Hider cmp) => hiders.Add(cmp);
				static void unregister(Hider cmp) => hiders.Remove(cmp);

				public static void setVisible(string id, bool val) =>
					hiders.Where(cmp => cmp.id == id || cmp.groupID == id).forEach(cmp => cmp.setVisible(val));

				public void setVisible(bool val) => gameObject.SetActive(visible = val);

				void Awake() => register(this);
				void OnDestroy() => unregister(this);

				void OnEnable()
				{
					if (!visible || !visChecker.visible)
						setVisible(false);
				}
			}
		}
	}
}