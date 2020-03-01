using System;

using UnityEngine;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		public abstract class ModOption
		{
#if DEBUG
			static readonly UniqueIDs uniqueIDs = new UniqueIDs();
#endif
			public readonly string id;
			protected readonly string label;

			protected GameObject gameObject;
			protected readonly Config.Field cfgField;

			public ModOption(Config.Field _cfgField, string _label)
			{
				cfgField = _cfgField;

				id = cfgField.path;
#if DEBUG
				uniqueIDs.ensureUniqueID(ref id);
#endif
				label = _label ?? id.clampLength(40);

				registerLabel(id, ref label);
			}

			public abstract void addOption(Options options);

			public abstract void onChangeValue(EventArgs e);
			public virtual  void onChangeGameObject(GameObject go)
			{
				 gameObject = go;
			}
		}
	}
}