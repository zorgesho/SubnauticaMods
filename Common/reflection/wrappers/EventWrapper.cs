using System;
using System.Reflection;

namespace Common.Reflection
{
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