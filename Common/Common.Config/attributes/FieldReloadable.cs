using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

namespace Common.Configuration
{
	partial class Config
	{
		public partial class Field
		{
			// field with this attribute will be reloaded if config changed outside of the game
			// value will change when application gets focus back
			[AttributeUsage(AttributeTargets.Field)]
			public class ReloadableAttribute: Attribute, IFieldAttribute, IRootConfigInfo
			{
				Config rootConfig;
				public void setRootConfig(Config config) => rootConfig = config;

				public void process(object config, FieldInfo field)
				{
					Reloader.addField(new Field(config, field, rootConfig));
				}


				static class Reloader
				{
					static Dictionary<Config, DateTime> timestamps;
					static Dictionary<Config, List<Field>> reloadableFields;

					public static void addField(Field cfgField)
					{
						if (timestamps == null)
						{
							UnityHelper.createPersistentGameObject<FocusListener>("CfgReloadFocusListener");
							timestamps = new Dictionary<Config, DateTime>();
							reloadableFields = new Dictionary<Config, List<Field>>();
						}

						var config = cfgField.rootConfig;

						if (!reloadableFields.TryGetValue(config, out var fieldList))
						{
							timestamps[config] = File.GetLastWriteTime(config.configPath);
							reloadableFields[config] = fieldList = new List<Field>();
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

								Config newCfg = tryLoad(cfg.GetType(), cfg.configPath, LoadOptions.ForcedLoad | LoadOptions.ReadOnly);

								if (newCfg != null)
									fields.Value.ForEach(field => field.value = newCfg.getFieldValueByPath(field.path));

								timestamps[cfg] = File.GetLastWriteTime(cfg.configPath);
							}
						}
					}
				}
			}
		}
	}
}