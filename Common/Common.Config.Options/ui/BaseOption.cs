using System;
using System.Collections.Generic;

using UnityEngine;

namespace Common.Configuration
{
	partial class Options
	{
		public abstract class ModOption
		{
			static readonly UniqueIDs uniqueIDs = new UniqueIDs();

			public interface IOnGameObjectChangeHandler
			{
				void init(ModOption option);
				void handle(GameObject gameObject);
			}

			readonly List<IOnGameObjectChangeHandler> handlers = new List<IOnGameObjectChangeHandler>();

			public void addHandler(IOnGameObjectChangeHandler handler)
			{
				handler.init(this);
				handlers.Add(handler);
			}

			public readonly string id;
			protected readonly string label;

			public GameObject gameObject { get; protected set; }
			public readonly Config.Field cfgField;

			public ModOption(Config.Field _cfgField, string _label)
			{
				cfgField = _cfgField;

				id = cfgField.path;
				uniqueIDs.ensureUniqueID(ref id);

				label = _label ?? id.clampLength(40);
				registerLabel(id, ref label);
			}

			public abstract void addOption(Options options);

			public abstract void onValueChange(EventArgs e);
			public virtual  void onGameObjectChange(GameObject go)
			{
				gameObject = go;
				handlers.ForEach(h => h.handle(gameObject));
			}
		}
	}
}