using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Common.Configuration
{
	partial class Options
	{
		public static partial class Components
		{
			// component for hiding options elements
			// we need separate component for this to avoid conflicts with toggleable option's headings
			public class Hider: MonoBehaviour
			{
				public interface IVisibilityChecker { bool visible { get; } }

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

					public Add(IVisibilityChecker _visChecker, string _groupID)
					{
						visChecker = _visChecker;
						groupID = _groupID;
					}

					public void handle(GameObject gameObject) =>
						gameObject.AddComponent<Hider>().init(id, groupID, visChecker.visible);
				}

				string id, groupID;
				bool visible;

				void init(string _id, string _groupID, bool _visible)
				{
					id = _id;
					groupID = _groupID;
					visible = _visible;
				}

				static readonly List<Hider> hiders = new List<Hider>();

				static void register(Hider cmp) => hiders.Add(cmp);
				static void unregister(Hider cmp) => hiders.Remove(cmp);

				public static void setVisible(string id, bool val) =>
					hiders.Where(cmp => cmp.id == id || cmp.groupID == id).forEach(cmp => cmp.setVisible(val));

				public void setVisible(bool val) => gameObject.SetActive(visible = val);

				void Awake() => register(this);
				void OnDestroy() => unregister(this);

				void OnEnable() { if (!visible) setVisible(false); }
			}
		}
	}
}