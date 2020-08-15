using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

namespace Common.Configuration.Utils
{
	static class ConfigReloader
	{
		static Dictionary<Config, DateTime> timestamps;
		static Dictionary<Config, List<Config.Field>> reloadableFields;

		public static void addField(Config.Field cfgField)
		{
			if (timestamps == null)
			{
				UnityHelper.createPersistentGameObject<FocusListener>($"{Mod.id}.ConfigReloaderFocusListener");
				timestamps = new Dictionary<Config, DateTime>();
				reloadableFields = new Dictionary<Config, List<Config.Field>>();
			}

			var config = cfgField.rootConfig;

			if (!reloadableFields.TryGetValue(config, out var fieldList))
			{
				timestamps[config] = File.GetLastWriteTime(config.configPath);
				reloadableFields[config] = fieldList = new List<Config.Field>();
			}

			fieldList.Add(cfgField);																	$"Reloadable field added: {cfgField.path}".logDbg();
		}

		class FocusListener: MonoBehaviour
		{
			bool firstSkipped = false; // skip first focus event that occurs on start

			void OnApplicationFocus(bool focus)
			{
				if (!focus || (!firstSkipped && (firstSkipped = true)))
					return;

				foreach (var fields in reloadableFields)
				{
					Config cfg = fields.Key;

					if (File.GetLastWriteTime(cfg.configPath) == timestamps[cfg])
						continue;

					Config newCfg = Config.tryLoad(cfg.GetType(), cfg.configPath, Config.LoadOptions.ForcedLoad | Config.LoadOptions.ReadOnly);

					if (newCfg != null)
						fields.Value.ForEach(field => field.value = newCfg.getFieldValueByPath(field.path));

					timestamps[cfg] = File.GetLastWriteTime(cfg.configPath);
				}
			}
		}
	}
}