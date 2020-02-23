using System;
using System.Collections.Generic;

using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		public abstract class ModOption
		{
			class UniqueIDs
			{
				readonly HashSet<string> allIDs = new HashSet<string>();
				readonly Dictionary<string, int> nonUniqueIDs = new Dictionary<string, int>();

				public void ensureUniqueID(ref string id)
				{
					if (allIDs.Add(id)) // if this is new id, do nothing
						return;

					nonUniqueIDs.TryGetValue(id, out int counter);
					nonUniqueIDs[id] = ++counter;

					id += "." + counter;																		$"UniqueIDs: fixed ID: {id}".logDbg();

					Debug.assert(allIDs.Add(id)); // checking updated id, just in case (in debug only)
				}
			}
			static readonly UniqueIDs uniqueIDs = new UniqueIDs();

			public readonly string id;
			protected readonly string label;

			protected readonly Config.Field cfgField;

			public ModOption(Config.Field _cfgField, string _label)
			{
				cfgField = _cfgField;

				id = cfgField.name;
				uniqueIDs.ensureUniqueID(ref id);

				label = _label;
				registerLabel(id, ref label);
			}

			public abstract void addOption(Options options);

			public virtual void onEvent(EventArgs e) => Config.main.save();
		}
	}
}