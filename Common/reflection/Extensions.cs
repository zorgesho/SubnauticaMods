using System;
using System.Reflection;
using System.Globalization;

namespace Common.Reflection
{
	static class ObjectExtensions
	{
		public static T cast<T>(this object obj)
		{
			try
			{																				$"cast<{typeof(T)}>(): object is null, default value is used".logDbg(obj == null);
				return obj == null? default: (T)obj;
			}
			catch
			{
				string msg = $"cast error: {obj}; {obj.GetType()} -> {typeof(T)}";
				Debug.assert(false, msg);
				msg.logError();

				return default;
			}
		}

		public static T convert<T>(this object obj) => obj.convert(typeof(T)).cast<T>();

		public static object convert(this object obj, Type targetType)
		{
			if (obj == null)
				return null;

			try
			{
				if (targetType.IsEnum)
				{
					if (obj is string str)
						return Enum.Parse(targetType, str, true);

					targetType = Enum.GetUnderlyingType(targetType);
				}
				else if (Nullable.GetUnderlyingType(targetType) is Type underlyingType)
				{
					return Activator.CreateInstance(targetType, obj.convert(underlyingType));
				}

				return Convert.ChangeType(obj, targetType, CultureInfo.InvariantCulture);
			}
			catch (Exception e) { Log.msg(e); return null; }
		}

		// assigns value to field with types conversion
		// returns `false` if field have equal value after conversion (or in case of exception)
		// returns 'true' in case of successful assignment
		public static bool setFieldValue(this object obj, FieldInfo field, object value)
		{
			try
			{
				object newValue = value.convert(field.FieldType);

				if (Equals(field.GetValue(obj), newValue))
					return false;

				field.SetValue(obj, newValue);

				return true;
			}
			catch (Exception e) { Log.msg(e); return false; }
		}

		public static void setFieldValue(this object obj, string fieldName, object value) => obj.GetType().field(fieldName)?.SetValue(obj, value);

		public static object getFieldValue(this object obj, string fieldName) => obj.GetType().field(fieldName)?.GetValue(obj);
		public static T getFieldValue<T>(this object obj, string fieldName) => obj.getFieldValue(fieldName).cast<T>();

		public static object getPropertyValue(this object obj, string propertyName) => obj.GetType().property(propertyName)?.GetValue(obj);
		public static T getPropertyValue<T>(this object obj, string propertyName) => obj.getPropertyValue(propertyName).cast<T>();
	}


	static class TypeExtensions
	{
		static MethodInfo _method(this Type type, string name, BindingFlags bf, Type[] types)
		{
			try { return types == null? type.GetMethod(name, bf): type.GetMethod(name, bf, null, types, null); }
			catch (AmbiguousMatchException)
			{
				$"Ambiguous method: {type.Name}.{name}".logError();
			}
			catch (Exception e) { Log.msg(e); }

			return null;
		}

		public static MethodInfo method(this Type type, string name, BindingFlags bf = ReflectionHelper.bfAll) => _method(type, name, bf, null);
		public static MethodInfo method(this Type type, string name, params Type[] types) => _method(type, name, ReflectionHelper.bfAll, types);
		public static MethodInfo method<T>(this Type type, string name, params Type[] types) => _method(type, name, ReflectionHelper.bfAll, types)?.MakeGenericMethod(typeof(T));

		public static EventInfo evnt(this Type type, string name, BindingFlags bf = ReflectionHelper.bfAll) => type.GetEvent(name, bf);
		public static FieldInfo field(this Type type, string name, BindingFlags bf = ReflectionHelper.bfAll) => type.GetField(name, bf);
		public static PropertyInfo property(this Type type, string name, BindingFlags bf = ReflectionHelper.bfAll) => type.GetProperty(name, bf);

		public static FieldInfo[] fields(this Type type, BindingFlags bf = ReflectionHelper.bfAll) => type.GetFields(bf);
		public static MethodInfo[] methods(this Type type, BindingFlags bf = ReflectionHelper.bfAll) => type.GetMethods(bf);
		public static PropertyInfo[] properties(this Type type, BindingFlags bf = ReflectionHelper.bfAll) => type.GetProperties(bf);
	}


	static class MemberInfoExtensions
	{
		public static string fullName(this MemberInfo memberInfo)
		{
			if (memberInfo == null)
				return "[null]";

			if ((memberInfo.MemberType & (MemberTypes.Method | MemberTypes.Field | MemberTypes.Property)) != 0)
				return $"{memberInfo.DeclaringType.FullName}.{memberInfo.Name}";

			if ((memberInfo.MemberType & (MemberTypes.TypeInfo | MemberTypes.NestedType)) != 0)
				return (memberInfo as Type).FullName;

			return memberInfo.Name;
		}

		public static EventWrapper wrap(this EventInfo evnt, object obj = null) => new EventWrapper(evnt, obj);
		public static MethodWrapper wrap(this MethodInfo method) => new MethodWrapper(method);
		public static MethodWrapper<D> wrap<D>(this MethodInfo method, object obj = null) where D: Delegate => new MethodWrapper<D>(method, obj);
		public static PropertyWrapper wrap(this PropertyInfo property) => new PropertyWrapper(property);


		public static A getAttr<A>(this MemberInfo memberInfo, bool includeDeclaringTypes = false) where A: Attribute
		{
			A[] attrs = null;
			memberInfo.getAttrs(ref attrs, includeDeclaringTypes, earlyExit: true);

			return attrs.Length > 0? attrs[0]: null;
		}

		public static A[] getAttrs<A>(this MemberInfo memberInfo, bool includeDeclaringTypes = false) where A: Attribute
		{
			A[] attrs = null;
			memberInfo.getAttrs(ref attrs, includeDeclaringTypes, earlyExit: false);

			return attrs;
		}

		static void getAttrs<A>(this MemberInfo memberInfo, ref A[] attrs, bool includeDeclaringTypes, bool earlyExit) where A: Attribute
		{
			attrs = attrs.append(Attribute.GetCustomAttributes(memberInfo, typeof(A)) as A[]);

			if (!includeDeclaringTypes)
				return;

			Type declaringType = memberInfo.DeclaringType;

			while (declaringType != null && (!earlyExit || attrs.Length == 0))
			{
				declaringType.getAttrs(ref attrs, false, false);
				declaringType = declaringType.DeclaringType;
			}
		}

		public static bool checkAttr<A>(this MemberInfo memberInfo) where A: Attribute => Attribute.IsDefined(memberInfo, typeof(A));
	}
}