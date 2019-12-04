using System;
using System.Reflection;
using System.Collections;

using UnityEngine;
using UnityEngine.Events;

namespace Common
{
	using Object = UnityEngine.Object;

	static class ObjectAndComponentExtensions
	{
		public static void callAfterDelay(this GameObject go, float delay, UnityAction action) =>
			go.AddComponent<CallAfterDelay>().init(delay, action);


		public static T getOrAddComponent<T>(this GameObject go) where T: Component =>
			go.GetComponent<T>() ?? go.AddComponent<T>();


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


		public static GameObject getChild(this GameObject go, string name) =>
			go.transform.Find(name)?.gameObject;


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

				foreach (var fieldName in fields)
					typeTo.field(fieldName).SetValue(cmpTo, typeFrom.field(fieldName).GetValue(cmpFrom), _BindingFlags.all, null, null);
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

				FieldInfo[] fields = typeTo.fields();

				foreach (var fieldTo in fields)
				{
					FieldInfo fieldFrom = typeFrom.field(fieldTo.Name);//.GetValue(cmpFrom), 

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

		//		FieldInfo[] fields = type.fields();
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


	class CallAfterDelay: MonoBehaviour
	{
		float delay;
		UnityAction action;

		public void init(float _delay, UnityAction _action)
		{
			action = _action;
			delay = _delay;
		}

		IEnumerator Start()
		{
			yield return new WaitForSeconds(delay);

			action();
			Destroy(this);
		}
	}
}