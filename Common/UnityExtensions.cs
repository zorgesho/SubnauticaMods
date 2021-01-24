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

		public static T ensureComponent<T>(this GameObject go) where T: Component => go.ensureComponent(typeof(T)) as T;
		public static Component ensureComponent(this GameObject go, Type type) => go.GetComponent(type) ?? go.AddComponent(type);

		public static void setParent(this GameObject go, GameObject parent, Vector3? position = null, Quaternion? rotation = null, Vector3? scale = null)
		{
			go.transform.SetParent(parent.transform, false);

			if (position != null) go.transform.localPosition = (Vector3)position;
			if (rotation != null) go.transform.localRotation = (Quaternion)rotation;
			if (scale != null)	  go.transform.localScale	 = (Vector3)scale;
		}

		public static GameObject getChild(this GameObject go, string name) => go.transform.Find(name)?.gameObject;


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

		public static void destroyComponent<T>(this GameObject go, bool immediate = true) where T: Component =>
			destroyComponent(go, typeof(T), immediate);

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
			var obj = new GameObject(name, typeof(SceneCleanerPreserve));
			Object.DontDestroyOnLoad(obj);
			return obj;
		}

		public static GameObject createPersistentGameObject<T>(string name) where T: Component
		{
			var obj = createPersistentGameObject(name);
			obj.AddComponent<T>();
			return obj;
		}

		// using reflection to avoid including UnityEngine.UI in all projects
		static readonly Type eventSystem = Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI");
		static readonly PropertyWrapper currentEventSystem = eventSystem.property("current").wrap();
		static readonly MethodWrapper setSelectedGameObject = eventSystem.method("SetSelectedGameObject", typeof(GameObject)).wrap();

		// unselects currently selected object (needed for buttons)
		public static void clearSelectedUIObject()
		{
			setSelectedGameObject.invoke(currentEventSystem.get(), null);
		}

		public static C findNearestToCam<C>() where C: Component =>
			findNearest<C>(LargeWorldStreamer.main?.cachedCameraPosition, out _);

		public static C findNearestToPlayer<C>() where C: Component =>
			findNearest<C>(Player.main?.transform.position, out _);

		public static C findNearestToPlayer<C>(out float distSq) where C: Component =>
			findNearest<C>(Player.main?.transform.position, out distSq);

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