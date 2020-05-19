using System;
using System.Reflection;

namespace Common.Reflection
{
	abstract class _MethodWrapper
	{
		protected readonly MethodInfo method;
		protected _MethodWrapper(MethodInfo method) => this.method = method;

		public static implicit operator bool(_MethodWrapper mw) => mw?.method != null;
	}

	// slow wrapper that uses reflection
	class MethodWrapper: _MethodWrapper
	{
		public MethodWrapper(MethodInfo method): base(method) {}

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


	// fast wrapper that uses delegate
	class MethodWrapper<D>: _MethodWrapper where D: Delegate
	{
		readonly object obj;
		public MethodWrapper(MethodInfo method, object obj = null): base(method) => this.obj = obj;

		public static implicit operator bool(MethodWrapper<D> mw) => mw?.invoke != null;

		public D invoke => _delegate ??= init();
		D _delegate;

		D init()
		{
			try { return (D)Delegate.CreateDelegate(typeof(D), obj, method); }
			catch (Exception e) { Log.msg(e); return null; }
		}
	}
}