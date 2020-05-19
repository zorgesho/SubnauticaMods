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
			{																				$"cast<{typeof(T)}>(): object is null !".logDbg(obj == null);
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
					targetType = Enum.GetUnderlyingType(targetType);

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

		public static object getFieldValue(this object obj, string fieldName) => obj.GetType().field(fieldName)?.GetValue(obj);
	}


	static class TypeExtensions
	{
		static MethodInfo _method(this Type type, string name, Type[] types)
		{
			try { return types == null? type.GetMethod(name, ReflectionHelper.bfAll): type.GetMethod(name, ReflectionHelper.bfAll, null, types, null); }
			catch (AmbiguousMatchException)
			{
				$"Ambiguous method: {type.Name}.{name}".logError();
			}
			catch (Exception e) { Log.msg(e); }

			return null;
		}

		public static MethodInfo method(this Type type, string name) => _method(type, name, null);
		public static MethodInfo method(this Type type, string name, params Type[] types) => _method(type, name, types);

		public static FieldInfo field(this Type type, string name) => type.GetField(name, ReflectionHelper.bfAll);
		public static PropertyInfo property(this Type type, string name) => type.GetProperty(name, ReflectionHelper.bfAll);

		public static FieldInfo[] fields(this Type type) => type.GetFields(ReflectionHelper.bfAll);
		public static MethodInfo[] methods(this Type type) => type.GetMethods(ReflectionHelper.bfAll);
		public static MethodInfo[] methods(this Type type, BindingFlags bf) => type.GetMethods(ReflectionHelper.bfAll | bf);
		public static PropertyInfo[] properties(this Type type) => type.GetProperties(ReflectionHelper.bfAll);

		public static MethodWrapper methodWrap(this Type type, string name) =>
			new MethodWrapper(type.method(name));

		public static MethodWrapper methodWrap(this Type type, string name, params Type[] types) =>
			new MethodWrapper(type.method(name, types));

		public static PropertyWrapper propertyWrap(this Type type, string name) =>
			new PropertyWrapper(type.property(name));

		public static EventWrapper eventWrap(this Type type, string name, object obj = null) =>
			new EventWrapper(type.GetEvent(name, ReflectionHelper.bfAll), obj);
	}


	static class MemberInfoExtensions
	{
		public static string fullName(this MemberInfo memberInfo) => (memberInfo == null)? "[null]": memberInfo.DeclaringType.FullName + "." + memberInfo.Name;

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