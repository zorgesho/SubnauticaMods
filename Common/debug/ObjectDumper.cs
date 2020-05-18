using System;
using System.IO;
using System.Linq;
using System.Text;

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

			static bool dumpFields = true;
			static bool dumpProperties = true;

			public static string dump(GameObject go, bool _dumpProperties, bool _dumpFields)
			{
				output.Length = 0;
				dumpProperties = _dumpProperties;
				dumpFields = _dumpFields;

				dump(go, "");

				return output.ToString();
			}
 
			static void dump(GameObject go, string indent)
			{
				output.AppendLine($"{indent}object: {go.name} active:{go.activeSelf}");
 
				foreach (var cmp in go.GetComponents<Component>())
					dump(cmp, indent + "\t");

				foreach (Transform child in go.transform)
					dump(child.gameObject, indent + "\t");
			}
 
			static void dump(Component cmp, string indent)
			{
				static string _formatValue(object value)
				{
					if (value == null)
						return "";

					string result = value.ToString();
					if (!result.isNullOrEmpty())
						result = result.Replace("\n", " ").Replace("\t", " ").TrimEnd();

					return result;
				}

				Type cmpType = cmp.GetType();
				output.AppendLine($"{indent}component: {cmpType}");

				try
				{
					if (dumpProperties)
					{
						var properties = cmpType.properties().ToList();
						if (properties.Count > 0)
						{
							properties.Sort((p1, p2) => p1.Name.CompareTo(p2.Name));
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
							fields.Sort((f1, f2) => f1.Name.CompareTo(f2.Name));
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