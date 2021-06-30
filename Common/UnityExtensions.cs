using System;
using System.Linq;
using System.Reflection;
using System.Collections;

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

		public static C ensureComponent<C>(this GameObject go) where C: Component => go.ensureComponent(typeof(C)) as C;
		public static Component ensureComponent(this GameObject go, Type type) => go.GetComponent(type) ?? go.AddComponent(type);

		public static GameObject getParent(this GameObject go) => go.transform.parent?.gameObject;
		public static GameObject getChild(this GameObject go, string name) => go.transform.Find(name)?.gameObject;

		public static void setTransform(this GameObject go, Vector3? pos = null, Vector3? localPos = null, Vector3? localAngles = null, Vector3? localScale = null)
		{
			var tr = go.transform;

			if (pos != null)			tr.position = (Vector3)pos;
			if (localPos != null)		tr.localPosition = (Vector3)localPos;
			if (localAngles != null)	tr.localEulerAngles = (Vector3)localAngles;
			if (localScale != null)		tr.localScale = (Vector3)localScale;
		}

		public static void setParent(this GameObject go, GameObject parent, Vector3? localPos = null, Vector3? localAngles = null)
		{
			go.transform.SetParent(parent.transform, false);
			go.setTransform(localPos: localPos, localAngles: localAngles);
		}

		public static GameObject createChild(this GameObject go, string name, Vector3? localPos = null)
		{
			GameObject child = new (name);
			child.setParent(go);

			child.setTransform(localPos: localPos);

			return child;
		}

		public static GameObject createChild(this GameObject go, GameObject prefab, string name = null,
											 Vector3? localPos = null, Vector3? localAngles = null, Vector3? localScale = null)
		{
			var child = Object.Instantiate(prefab, go.transform);

			if (name != null)
				child.name = name;

			child.setTransform(localPos: localPos, localAngles: localAngles, localScale: localScale);

			return child;
		}

		// for use with inactive game objects
		public static C getComponentInParent<C>(this GameObject go) where C: Component
		{
			return _get<C>(go);

			static _C _get<_C>(GameObject _go) where _C: Component => !_go? null: (_go.GetComponent<_C>() ?? _get<_C>(_go.getParent()));
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

		public static void destroyChildren(this GameObject go, params string[] children) =>
			children.forEach(name => go.destroyChild(name, true));

		public static void destroyComponent(this GameObject go, Type componentType, bool immediate = true) =>
			go.GetComponent(componentType)?._destroy(immediate);

		public static void destroyComponent<C>(this GameObject go, bool immediate = true) where C: Component =>
			destroyComponent(go, typeof(C), immediate);

		public static void destroyComponentInChildren<C>(this GameObject go, bool immediate = true) where C: Component =>
			go.GetComponentInChildren<C>()?._destroy(immediate);


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


	static class StructsExtension
	{
		public static Vector2 setX(this Vector2 vec, float val) { vec.x = val; return vec; }
		public static Vector2 setY(this Vector2 vec, float val) { vec.y = val; return vec; }

		public static Vector3 setX(this Vector3 vec, float val) { vec.x = val; return vec; }
		public static Vector3 setY(this Vector3 vec, float val) { vec.y = val; return vec; }
		public static Vector3 setZ(this Vector3 vec, float val) { vec.z = val; return vec; }

		public static Color setA(this Color color, float val) { color.a = val; return color; }
	}


	static class UnityHelper
	{
		public static GameObject createPersistentGameObject(string name)
		{
			GameObject obj = new (name, typeof(SceneCleanerPreserve));
			Object.DontDestroyOnLoad(obj);
			return obj;
		}

		public static GameObject createPersistentGameObject<C>(string name) where C: Component
		{
			var obj = createPersistentGameObject(name);
			obj.AddComponent<C>();
			return obj;
		}


		// using reflection to avoid including UnityEngine.UI in all projects
		static readonly Type eventSystem = Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI");
		static readonly PropertyWrapper currentEventSystem = eventSystem.property("current").wrap();
		static readonly MethodWrapper setSelectedGameObject = eventSystem.method("SetSelectedGameObject", typeof(GameObject)).wrap();

		// unselects currently selected object (needed for buttons)
		public static void clearSelectedUIObject() =>
			setSelectedGameObject.invoke(currentEventSystem.get(), null);

		// for use in non-performance critical code
		public static C findNearest<C>(Vector3? pos, out float distSq) where C: Component
		{
			distSq = float.MaxValue;

			if (pos == null)
				return null;

			C result = null;
			Vector3 validPos = (Vector3)pos;

			foreach (var c in Object.FindObjectsOfType<C>())
			{
				float tmpDistSq = (c.transform.position - validPos).sqrMagnitude;

				if (tmpDistSq < distSq)
				{
					distSq = tmpDistSq;
					result = c;
				}
			}

			return result;
		}
	}


	static class InputHelper
	{
		public static int getMouseWheelDir() => Math.Sign(getMouseWheelValue());
		public static float getMouseWheelValue() => getAxis? getAxis.invoke("Mouse ScrollWheel"): 0f;

		static readonly MethodWrapper<Func<string, float>> getAxis =
			Type.GetType("UnityEngine.Input, UnityEngine.InputLegacyModule")?.method("GetAxis")?.wrap<Func<string, float>>();
	}


	class CallAfterDelay: MonoBehaviour
	{
		float delay;
		UnityAction action;

		public void init(float delay, UnityAction action)
		{
			this.delay = delay;
			this.action = action;
		}

		IEnumerator Start()
		{
			yield return new WaitForSeconds(delay);

			action();
			Destroy(this);
		}
	}
}