using System.Reflection;

namespace Common.Reflection
{
	class MethodWrapper
	{
		readonly MethodInfo method;
		public static implicit operator bool(MethodWrapper mw) => mw.method != null;

		public MethodWrapper(MethodInfo method) => this.method = method;

		public object invoke()
		{
			Debug.assert(method != null && method.IsStatic);
			return method?.Invoke(null, null);
		}

		public object invoke(object obj)
		{
			Debug.assert(method != null);
			return method.IsStatic? method?.Invoke(null, new object[] { obj }): method?.Invoke(obj, null);
		}

		public object invoke(object obj, params object[] parameters)
		{
			Debug.assert(method != null);
			return method?.Invoke(obj, parameters ?? new object[1]); // null check in case we need to pass one 'null' as a parameter
		}

		public T invoke<T>() => invoke().cast<T>();
		public T invoke<T>(object obj) => invoke(obj).cast<T>();
		public T invoke<T>(object obj, params object[] parameters) => invoke(obj, parameters).cast<T>();
	}
}