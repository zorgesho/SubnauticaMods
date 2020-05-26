﻿using System;
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

			public ModOption(Config.Field cfgField, string label)
			{
				this.cfgField = cfgField;

				id = cfgField.id;
				uniqueIDs.ensureUniqueID(ref id);

				this.label = label ?? id.clampLength(40);
				registerLabel(id, ref this.label);
			}

			public abstract void addOption(Options options);

			public abstract void onValueChange(EventArgs e);
			public virtual  void onGameObjectChange(GameObject go)
			{
				gameObject = go;
				handlers.ForEach(h => h.handle(gameObject));
			}

			public void onRemove() => uniqueIDs.freeID(id);
#if DEBUG
			~ModOption() => $"ModOption '{id}' is gc'ed".logDbg();
#endif
		}
	}
}