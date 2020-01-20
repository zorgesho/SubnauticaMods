using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.Events;

namespace Common
{
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

		// if fields is empty we try to copy all fields
		public static void copyValuesFrom<CT, CF>(this CT cmpTo, CF cmpFrom, params string[] fields) where CT: Component where CF: Component
		{
			try
			{
				Type typeTo = cmpTo.GetType(), typeFrom = cmpFrom.GetType();

				if (fields.Length == 0)
				{
					foreach (var fieldTo in typeTo.fields())
					{
						if (typeFrom.field(fieldTo.Name) is FieldInfo fieldFrom)
						{																											$"copyValuesFrom: copying field {fieldTo.Name} from {cmpFrom} to {cmpTo}".logDbg();
							fieldTo.SetValue(cmpTo, fieldFrom.GetValue(cmpFrom));
						}
					}
				}
				else
				{
					foreach (var fieldName in fields)
					{
						if (typeTo.field(fieldName) is FieldInfo fieldTo && typeFrom.field(fieldName) is FieldInfo fieldFrom)
						{																											$"copyValuesFrom: copying field {fieldName} from {cmpFrom} to {cmpTo}".logDbg();
							fieldTo.SetValue(cmpTo, fieldFrom.GetValue(cmpFrom));
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.msg(e);
			}
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
		static InputHelper() => HarmonyHelper.patch(typeof(InputHelper).method(nameof(InputHelper.getMouseWheelValue)),
										transpiler: typeof(InputHelper).method(nameof(InputHelper.patch_getMouseWheelValue)));

		static IEnumerable<CodeInstruction> patch_getMouseWheelValue(IEnumerable<CodeInstruction> cins) // weird way to avoid including InputLegacyModule in all references
		{
			if (Assembly.Load("UnityEngine.InputLegacyModule")?.GetType("UnityEngine.Input")?.method("GetAxis") is MethodInfo GetAxis)
			{
				return new List<CodeInstruction>
				{
					new CodeInstruction(OpCodes.Ldstr, "Mouse ScrollWheel"),
					new CodeInstruction(OpCodes.Call, GetAxis),
					new CodeInstruction(OpCodes.Ret)
				};
			}

			return cins;
		}

		public static float getMouseWheelValue() { "InputHelper.getMouseWheelValue is not patched!".logError(); return 0f; } // need to logging anyway to avoid inlining

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