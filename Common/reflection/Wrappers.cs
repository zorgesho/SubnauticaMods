using System;
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

		public T invoke<T>() => ReflectionHelper.safeCast<T>(invoke());
		public T invoke<T>(object obj) => ReflectionHelper.safeCast<T>(invoke(obj));
		public T invoke<T>(object obj, params object[] parameters) => ReflectionHelper.safeCast<T>(invoke(obj, parameters));
	}


	class PropertyWrapper
	{
		readonly PropertyInfo propertyInfo;
		MethodBase setter, getter;

		public PropertyWrapper(PropertyInfo propertyInfo)
		{
			Debug.assert(propertyInfo != null);
			this.propertyInfo = propertyInfo;
		}

		public void set(object value) => set(null, value);

		public void set(object obj, object value)
		{
			setter ??= propertyInfo?.GetSetMethod();
			Debug.assert(setter != null);

			setter?.Invoke(obj, new object[] { value });
		}

		public object get(object obj = null)
		{
			getter ??= propertyInfo?.GetGetMethod();
			Debug.assert(getter != null);

			return getter?.Invoke(obj, null);
		}

		public T get<T>(object obj = null) => ReflectionHelper.safeCast<T>(get(obj));
	}


	class EventWrapper // for use with publicized assemblies
	{
		readonly object obj;
		readonly EventInfo eventInfo;

		MulticastDelegate eventDelegate;
		MethodInfo adder, remover;

		public EventWrapper(EventInfo eventInfo, object obj = null)
		{
			Debug.assert(eventInfo != null);

			this.obj = obj;
			this.eventInfo = eventInfo;
		}

		public void add<D>(D eventDelegate) => add(obj, eventDelegate);
		public void add<D>(object obj, D eventDelegate)
		{
			adder ??= eventInfo?.GetAddMethod();
			Debug.assert(adder != null);

			adder?.Invoke(obj, new object[] { eventDelegate });
		}

		public void remove<D>(D eventDelegate) => remove(obj, eventDelegate);
		public void remove<D>(object obj, D eventDelegate)
		{
			remover ??= eventInfo?.GetRemoveMethod();
			Debug.assert(remover != null);

			remover?.Invoke(obj, new object[] { eventDelegate });
		}

		public void raise(params object[] eventParams) // only for initial 'obj' for now
		{
			Debug.assert(eventInfo.IsMulticast);

			eventDelegate ??= eventInfo.DeclaringType.field(eventInfo.Name)?.GetValue(obj) as MulticastDelegate;
			eventDelegate?.GetInvocationList().forEach(dlg => dlg.Method.Invoke(dlg.Target, eventParams));
		}
	}
}