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
}