using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using Object = UnityEngine.Object;

namespace Common
{
	static class ObjectAndComponentExtensions
	{
		public static T getOrAddComponent<T>(this GameObject go) where T: Component
		{
			return go.GetComponent<T>() ?? go.AddComponent<T>();
		}


		public static void addComponentIfNeeded<T>(this GameObject go) where T: Component
		{
			if (!go.GetComponent<T>())
				go.AddComponent<T>();
		}


		public static void setParent(this GameObject go, GameObject parent, bool resetLocalTransform = true)
		{
			go.transform.parent = parent.transform;

			if (resetLocalTransform)
			{
				go.transform.localRotation = Quaternion.identity;
				go.transform.localPosition = Vector3.zero;
				go.transform.localScale = Vector3.one;
			}
		}


		public static GameObject getChild(this GameObject go, string name)
		{
			return go.transform.Find(name)?.gameObject;
		}


		public static T getComponentInHierarchy<T>(this GameObject go, bool checkChildren = true, bool checkParent = true) where T: Component
		{
			T cmp = go.GetComponent<T>();
			
			if (checkChildren && cmp == null)
				cmp = go.GetComponentInChildren<T>();
			
			if (checkParent && cmp == null)
				cmp = go.GetComponentInParent<T>();
			
			return cmp;
		}


		public static void destroyChild(this GameObject go, string name, bool immediate = true)
		{
			if (immediate)
				Object.DestroyImmediate(go.getChild(name));
			else
				Object.Destroy(go.getChild(name));
		}


		public static void destroyComponent<T>(this GameObject go, bool immediate = true) where T: Component
		{
			if (immediate)
				Object.DestroyImmediate(go.GetComponent<T>());
			else
				Object.Destroy(go.GetComponent<T>());
		}


		public static void destroyComponentInChildren<T>(this GameObject go, bool immediate = true) where T: Component
		{
			if (immediate)
				Object.DestroyImmediate(go.GetComponentInChildren<T>());
			else
				Object.Destroy(go.GetComponentInChildren<T>());
		}

		//TODO: refactor copyValuesFrom
		public static void copyValuesFrom<CT, CF>(this CT cmpTo, CF cmpFrom, params string[] fields) where CT: Component where CF: Component
		{
			try
			{
				Type typeTo = cmpTo.GetType(), typeFrom = cmpFrom.GetType();
				BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

				foreach (var fieldName in fields)
					typeTo.GetField(fieldName, bf).SetValue(cmpTo, typeFrom.GetField(fieldName, bf).GetValue(cmpFrom), bf, null, null);
			}
			catch (Exception e)
			{
				Log.msg(e);
			}
		}

		public static void copyValuesFrom<CT, CF>(this CT cmpTo, CF cmpFrom) where CT: Component where CF: Component
		{
			try
			{
				Type typeTo = cmpTo.GetType(), typeFrom = cmpFrom.GetType();
				BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

				FieldInfo[] fields = typeTo.GetFields(bf);

				foreach (var fieldTo in fields)
				{
					FieldInfo fieldFrom = typeFrom.GetField(fieldTo.Name, bf);//.GetValue(cmpFrom), 

					if (fieldFrom != null)
					{
						fieldTo.SetValue(cmpTo, fieldFrom.GetValue(cmpFrom));
					}
				}
			}
			catch (Exception e)
			{
				Log.msg(e);
			}
		}

		// copied from old solution, need to refactor all three of these

		//public static void copyValuesFrom<C>(this C cmpTo, C cmpFrom) where C: Component
		//{
		//	try
		//	{
		//		Type type = cmpTo.GetType();
		//		BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

		//		FieldInfo[] fields = type.GetFields(bf);
		//		Console.WriteLine($"[copyValuesFrom] fields count: {fields.Length}");

		//		foreach (var field in fields)
		//		{
		//			field.SetValue(cmpTo, field.GetValue(cmpFrom));
		//			Console.WriteLine($"[copyValuesFrom] {field.Name} = \"{field.GetValue(cmpFrom)}\"");
		//		}
		//	}
		//	catch (Exception e)
		//	{
		//		Log.msg(e);
		//	}
		//}
	}


	static class UnityHelper
	{
		public static GameObject createPersistentGameObject<T>(string name) where T: Component
		{
			GameObject obj = new GameObject(name, typeof(SceneCleanerPreserve), typeof(T));
			Object.DontDestroyOnLoad(obj);

			return obj;
		}
	}


	static class InputHelper
	{
		public static float getMouseWheelValue() => Input.GetAxis("Mouse ScrollWheel");

		public static void resetCursorToCenter()
		{
			Cursor.lockState = CursorLockMode.Locked; // warning: don't set lockState separately, use UWE utils for this if needed
			Cursor.lockState = CursorLockMode.None;
		}
	}


	static partial class Debug
	{
#if DEBUG
		const string pathForDumps = "c:/projects/subnautica/prefab_dumps/";
#endif
		public static string dumpGameObject(GameObject go, bool dumpProperties = true, bool dumpFields = false) =>
			ObjectDumper.dump(go, dumpProperties, dumpFields);

		public static void dump(this GameObject go, string filename = null)
		{
			if (filename == null)
				filename = go.name.Replace("(Clone)", "").ToLower();
#if DEBUG
			filename = pathForDumps + filename;
#endif
			ObjectDumper.dump(go, true, true).saveToFile(filename + ".yml");
		}


		//based on Yossarian King SceneDumper: http://wiki.unity3d.com/index.php?title=SceneDumper
		static class ObjectDumper
		{
			static readonly BindingFlags bf = _BindingFlags.all;// | BindingFlags.FlattenHierarchy;
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
				string _formatValue(object value) // used in properties now, it seems that fields don't need it
				{
					if (value == null)
						return "";

					string result = value.ToString();
					if (!string.IsNullOrEmpty(result))
						result = result.Replace("\n", " ").Replace("\t", " ").TrimEnd();

					return result;
				}

				Type cmpType = cmp.GetType();
				output.AppendLine($"{indent}component: {cmpType}");

				try
				{
					if (dumpProperties)
					{
						var properties = new List<PropertyInfo>(cmpType.GetProperties(bf));
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
						var fields = new List<FieldInfo>(cmpType.GetFields(bf));
						if (fields.Count > 0)
						{
							fields.Sort((f1, f2) => f1.Name.CompareTo(f2.Name));
							output.AppendLine($"{indent}\tFIELDS:");

							foreach (var field in fields)
								output.AppendLine($"{indent}\t{field.Name}: \"{_formatValue(field.GetValue(cmp))}\"");
						}
					}
				}
				catch (Exception e)
				{
					Log.msg(e);
				}
			}
		}
	}
}