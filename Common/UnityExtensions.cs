using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace Common
{
	using Reflection;
	using Object = UnityEngine.Object;

	static class ObjectAndComponentExtensions
	{
		public static void callAfterDelay(this GameObject go, float delay, UnityAction action) =>
			go.AddComponent<CallAfterDelay>().init(delay, action);

		public static T ensureComponent<T>(this GameObject go) where T: Component => go.GetComponent<T>() ?? go.AddComponent<T>();
		public static Component ensureComponent(this GameObject go, Type type) => go.GetComponent(type) ?? go.AddComponent(type);

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

		public static GameObject getChild(this GameObject go, string name) => go.transform.Find(name)?.gameObject;

		public static T getComponentInHierarchy<T>(this GameObject go, bool checkChildren = true, bool checkParent = true) where T: Component
		{
			T cmp = go.GetComponent<T>();

			if (checkChildren && !cmp)
				cmp = go.GetComponentInChildren<T>();

			if (checkParent && !cmp)
				cmp = go.GetComponentInParent<T>();

			return cmp;
		}


		static void _destroy(this Object obj, bool immediate)
		{
			if (immediate)
				Object.DestroyImmediate(obj);
			else
				Object.Destroy(obj);
		}

		public static void destroyChild(this GameObject go, string name, bool immediate = true) =>
			go.getChild(name)?._destroy(immediate);

		public static void destroyComponent<T>(this GameObject go, bool immediate = true) where T: Component =>
			go.GetComponent<T>()?._destroy(immediate);

		public static void destroyComponentInChildren<T>(this GameObject go, bool immediate = true) where T: Component =>
			go.GetComponentInChildren<T>()?._destroy(immediate);


		// if fields is empty we try to copy all fields
		public static void copyFieldsFrom<CT, CF>(this CT cmpTo, CF cmpFrom, params string[] fieldNames) where CT: Component where CF: Component
		{
			try
			{
				Type typeTo = cmpTo.GetType(), typeFrom = cmpFrom.GetType();

				foreach (var fieldTo in fieldNames.Length == 0? typeTo.fields(): fieldNames.Select(name => typeTo.field(name)))
				{
					if (typeFrom.field(fieldTo.Name) is FieldInfo fieldFrom)
					{																										$"copyFieldsFrom: copying field {fieldTo.Name} from {cmpFrom} to {cmpTo}".logDbg();
						fieldTo.SetValue(cmpTo, fieldFrom.GetValue(cmpFrom));
					}
				}
			}
			catch (Exception e) { Log.msg(e); }
		}
	}


	static class VectorExtension
	{
		public static Vector2 setX(this Vector2 vec, float val)  { vec.x = val; return vec; }
		public static Vector2 setY(this Vector2 vec, float val)  { vec.y = val; return vec; }
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
		public struct KeyWithModifier
		{
			public readonly KeyCode modifier, key;

			public KeyWithModifier(KeyCode key1, KeyCode key2 = KeyCode.None)
			{
				if (key1 == KeyCode.None || key2 == KeyCode.None) // if only one key defined treat it like a normal key
				{
					key = key1 == KeyCode.None? key2: key1;
					modifier = KeyCode.None;
					return;
				}

				bool isKey1Mod = isModifier(key1), isKey2Mod = isModifier(key2);

				if (isKey1Mod && !isKey2Mod)
				{
					modifier = key1;
					key = key2;
				}
				else if (!isKey1Mod && isKey2Mod)
				{
					modifier = key2;
					key = key1;
				}
				else // if both keys are modifiers or non-modifiers then use only first key
				{
					key = key1;
					modifier = KeyCode.None;
				}
			}

			static readonly HashSet<KeyCode> modifiers = new HashSet<KeyCode>()
			{
				KeyCode.LeftControl, KeyCode.RightControl,
				KeyCode.LeftShift, KeyCode.RightShift,
				KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.AltGr
			};

			public static bool isModifier(KeyCode keyCode) => modifiers.Contains(keyCode);
		}


		public static float getMouseWheelValue() => getAxis? getAxis.invoke("Mouse ScrollWheel"): 0f;

		static readonly MethodWrapper<Func<string, float>> getAxis =
			ReflectionHelper.safeGetType("UnityEngine.InputLegacyModule", "UnityEngine.Input")?.method("GetAxis")?.wrap<Func<string, float>>();
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