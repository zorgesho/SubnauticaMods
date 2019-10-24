using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

namespace Common
{
	static class ObjectAndComponentExtensions
	{
		static public T getOrAddComponent<T>(this GameObject go) where T: Component
		{
			return go.GetComponent<T>() ?? go.AddComponent<T>();
		}

		static public void addComponentIfNeeded<T>(this GameObject go) where T: Component
		{
			if (!go.GetComponent<T>())
				go.AddComponent<T>();
		}
	}


	static class InputHelper
	{
		static public float getMouseWheelValue() => Input.GetAxis("Mouse ScrollWheel");

		static public void resetCursorToCenter()
		{
			Cursor.lockState = CursorLockMode.Locked; // warning: don't set lockState separately, use UWE utils for this if needed
			Cursor.lockState = CursorLockMode.None;
		}
	}


	static partial class Debug
	{
		static public string dumpGameObject(GameObject go, bool dumpProperties = true, bool dumpFields = false)
		{
			return ObjectDumper.dump(go, dumpProperties, dumpFields);
		}
		
		//based on Yossarian King SceneDumper: http://wiki.unity3d.com/index.php?title=SceneDumper
		static class ObjectDumper
		{
			const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
			static readonly StringBuilder output = new StringBuilder();

			static bool dumpFields = true;
			static bool dumpProperties = true;
			
			static public string dump(GameObject go, bool _dumpProperties, bool _dumpFields)
			{
				output.Length = 0;
				dumpProperties = _dumpProperties;
				dumpFields = _dumpFields;

				dump(go, "");

				return output.ToString();
			}
 
			static void dump(GameObject go, string indent)
			{
				output.AppendLine($"{indent}obj: {go.name} active:{go.activeSelf}");
 
				foreach (var cmp in go.GetComponents<Component>())
					dump(cmp, indent + "\t");
 
				foreach (Transform child in go.transform)
					dump(child.gameObject, indent + "\t");
			}
 
			static void dump(Component cmp, string indent)
			{
				Type cmpType = cmp.GetType();

				output.AppendLine($"{indent}cmp: {cmpType} {cmp.name}");

				if (dumpProperties)
				{
					var properties = new List<PropertyInfo>(cmpType.GetProperties(bf));
					properties.Sort((p1, p2) => p1.Name.CompareTo(p2.Name));

					output.AppendLine($"{indent}properties:");

					foreach (var prop in properties)
						output.AppendLine($"{indent}\t{prop.Name}: {prop.GetValue(cmp, null)}");
				}

				if (dumpFields)
				{
					var fields = new List<FieldInfo>(cmpType.GetFields(bf));
					fields.Sort((f1, f2) => f1.Name.CompareTo(f2.Name));
					
					output.AppendLine($"{indent}fields:");
					
					foreach (var field in fields)
						output.AppendLine($"{indent}\t{field.Name}: {field.GetValue(cmp)}");
				}
			}
		}
	}
}