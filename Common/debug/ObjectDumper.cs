using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

namespace Common
{
	using Reflection;

	static partial class Debug
	{
#if DEBUG
		const string pathForDumps = "c:/projects/subnautica/dumps/";
#endif
		public static string dumpGameObject(GameObject go, bool dumpProperties = true, bool dumpFields = false) =>
			ObjectDumper.dump(go, dumpProperties, dumpFields);

		public static void dump(this GameObject go, string filename = null, int dumpParent = 0)
		{
			while (dumpParent > 0 && go.transform.parent)
			{
				go = go.transform.parent.gameObject;
				dumpParent--;
			}

			filename ??= go.name.Replace("(Clone)", "").ToLower();
#if DEBUG
			Directory.CreateDirectory(pathForDumps);
			filename = pathForDumps + filename;
#endif
			ObjectDumper.dump(go, true, true).saveToFile(filename + ".yml");
		}


		static class ObjectDumper
		{
			static readonly StringBuilder output = new StringBuilder();

			static bool dumpProperties;
			static bool dumpFields;

			public static string dump(GameObject go, bool dumpProperties, bool dumpFields)
			{
				output.Clear();
				ObjectDumper.dumpProperties = dumpProperties;
				ObjectDumper.dumpFields = dumpFields;

				dump(go, "");

				return output.ToString();
			}
 
			static void dump(GameObject go, string indent)
			{
				output.AppendLine($"{indent}object: {go.name} activeS/activeH:{go.activeSelf}/{go.activeInHierarchy}");
 
				foreach (var cmp in go.GetComponents<Component>())
					dump(cmp, indent + "\t");

				foreach (Transform child in go.transform)
					dump(child.gameObject, indent + "\t");
			}
 
			static void dump(Component cmp, string indent)
			{
				static void _sort<T>(List<T> list) where T: MemberInfo => list.Sort((m1, m2) => m1.Name.CompareTo(m2.Name));
				static string _formatValue(object value) => value?.ToString().Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Trim() ?? "";

				Type cmpType = cmp.GetType();
				output.AppendLine($"{indent}component: {cmpType}");

				try
				{
					if (dumpProperties)
					{
						var properties = cmpType.properties().ToList();
						if (properties.Count > 0)
						{
							_sort(properties);
							output.AppendLine($"{indent}\tPROPERTIES:");

							foreach (var prop in properties)
							{
								if (prop.GetGetMethod() != null)
									output.AppendLine($"{indent}\t{prop.Name}: \"{_formatValue(prop.GetValue(cmp, null))}\"");
							}
						}
					}

					if (dumpFields)
					{
						var fields = cmpType.fields().ToList();
						if (fields.Count > 0)
						{
							_sort(fields);
							output.AppendLine($"{indent}\tFIELDS:");

							foreach (var field in fields)
								output.AppendLine($"{indent}\t{field.Name}: \"{_formatValue(field.GetValue(cmp))}\"");
						}
					}
				}
				catch (Exception e) { Log.msg(e); }
			}
		}
	}
}