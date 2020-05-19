using System.Reflection;

namespace Common.Reflection
{
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

		public T get<T>(object obj = null) => get(obj).cast<T>();
	}
}