using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Collections.Generic;

namespace Common
{
	static class ObjectExtensions
	{
		public static int   toInt(this object obj)   => Convert.ToInt32(obj, CultureInfo.InvariantCulture);
		public static bool  toBool(this object obj)  => Convert.ToBoolean(obj, CultureInfo.InvariantCulture);
		public static float toFloat(this object obj) => Convert.ToSingle(obj, CultureInfo.InvariantCulture);

		// assigns value to field with types conversion
		// returns `false` if field have equal value after conversion (or in case of exception)
		// returns 'true' in case of successful assignment
		public static bool setFieldValue(this object obj, FieldInfo field, object value)
		{
			try
			{
				object newValue;

				if (field.FieldType.IsEnum)
					newValue = Convert.ChangeType(value, Enum.GetUnderlyingType(field.FieldType), CultureInfo.InvariantCulture);
				else
					newValue = Convert.ChangeType(value, field.FieldType, CultureInfo.InvariantCulture);

				if (Equals(field.GetValue(obj), newValue))
					return false;

				field.SetValue(obj, newValue);

				return true;
			}
			catch (Exception e) { Log.msg(e); return false; }
		}

		public static object getFieldValue(this object obj, string fieldName) => obj.GetType().field(fieldName)?.GetValue(obj);
	}


	static class ReflectionHelper
	{
		public static BindingFlags bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		public static Type getCallingType() => new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().ReflectedType;

		// for getting mod's defined types, don't include any of Common projects types (or types without namespace)
		public static readonly List<Type> definedTypes =
			Assembly.GetExecutingAssembly().GetTypes().Where(type => type.Namespace?.StartsWith(nameof(Common)) == false).ToList();

		public static Type safeGetType(string assemblyName, string typeName)
		{
			try   { return Assembly.Load(assemblyName)?.GetType(typeName, false); }
			catch { return null; }
		}

		static T _safecast<T>(object obj)
		{
			Debug.assert(obj is T, $"cast: {obj}; {obj.GetType()} -> {typeof(T)}");
			return (obj is T)? (T)obj: default;
		}

		public class MethodWrapper
		{
			readonly MethodBase method;
			public static implicit operator bool(MethodWrapper mw) => mw.method != null;

			public MethodWrapper(MethodBase method) => this.method = method;

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

			public T invoke<T>() => _safecast<T>(invoke());
			public T invoke<T>(object obj) => _safecast<T>(invoke(obj));
			public T invoke<T>(object obj, params object[] parameters) => _safecast<T>(invoke(obj, parameters));
		}

		public class PropertyWrapper
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

			public T get<T>(object obj = null) => _safecast<T>(get(obj));
		}

		// for use with publicized assemblies
		public class EventWrapper
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


	static class TypeExtensions
	{
		static MethodInfo _method(this Type type, string name, Type[] types)
		{
			try { return types == null? type.GetMethod(name, ReflectionHelper.bfAll): type.GetMethod(name, ReflectionHelper.bfAll, null, types, null); }
			catch (AmbiguousMatchException)
			{
				$"Ambiguous method: {type.Name}.{name}".logError();
				return null;
			}
			catch (Exception e) { Log.msg(e); return null; }
		}

		public static MethodInfo method(this Type type, string name) => _method(type, name, null);
		public static MethodInfo method(this Type type, string name, params Type[] types) => _method(type, name, types);

		public static FieldInfo field(this Type type, string name) => type.GetField(name, ReflectionHelper.bfAll);
		public static PropertyInfo property(this Type type, string name) => type.GetProperty(name, ReflectionHelper.bfAll);

		public static FieldInfo[] fields(this Type type) => type.GetFields(ReflectionHelper.bfAll);
		public static MethodInfo[] methods(this Type type) => type.GetMethods(ReflectionHelper.bfAll);
		public static MethodInfo[] methods(this Type type, BindingFlags bf) => type.GetMethods(ReflectionHelper.bfAll | bf);
		public static PropertyInfo[] properties(this Type type) => type.GetProperties(ReflectionHelper.bfAll);

		public static ReflectionHelper.MethodWrapper methodWrap(this Type type, string name) =>
			new ReflectionHelper.MethodWrapper(type.method(name));

		public static ReflectionHelper.MethodWrapper methodWrap(this Type type, string name, params Type[] types) =>
			new ReflectionHelper.MethodWrapper(type.method(name, types));

		public static ReflectionHelper.PropertyWrapper propertyWrap(this Type type, string name) =>
			new ReflectionHelper.PropertyWrapper(type.property(name));

		public static ReflectionHelper.EventWrapper eventWrap(this Type type, string name, object obj = null) =>
			new ReflectionHelper.EventWrapper(type.GetEvent(name, ReflectionHelper.bfAll), obj);
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


	static class MiscExtensions
	{
		public static void addRange<T>(this ICollection<T> target, IEnumerable<T> source) => source.forEach(e => target.Add(e));

		public static void add<T>(this List<T> target, T item, int count)
		{
			for (int i = 0; i < count; i++)
				target.Add(item);
		}

		public static void forEach<T>(this IEnumerable<T> sequence, Action<T> action)
		{
			if (sequence == null)
				return;

			var enumerator = sequence.GetEnumerator();
			while (enumerator.MoveNext())
				action(enumerator.Current);
		}

		public static T createDelegate<T>(this DynamicMethod dm) where T: class => dm.CreateDelegate(typeof(T)) as T;
	}


	static class ArrayExtensions
	{
		public static T[] init<T>(this T[] array)
		{
			array.Initialize();
			return array;
		}

		public static int findIndex<T>(this T[] array, int beginIndex, int endIndex, Predicate<T> predicate) =>
			Array.FindIndex(array, beginIndex, endIndex - beginIndex, predicate);

		public static int findIndex<T>(this T[] array, Predicate<T> predicate) =>
			Array.FindIndex(array, predicate);

		public static T[] append<T>(this T[] array1, T[] array2)
		{
			if (array1 != null && (array2 == null || array2.Length == 0))
				return array1;

			if (array1 == null)
				return array2 ?? new T[0];

			T[] newArray = new T[array1.Length + array2.Length];

			array1.CopyTo(newArray, 0);
			array2.CopyTo(newArray, array1.Length);

			return newArray;
		}
	}


	static partial class StringExtensions
	{
		public static bool isNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

		public static string format(this string s, object arg0) => string.Format(s, arg0);
		public static string format(this string s, object arg0, object arg1) => string.Format(s, arg0, arg1);
		public static string format(this string s, object arg0, object arg1, object arg2) => string.Format(s, arg0, arg1, arg2);
		public static string format(this string s, params object[] args) => string.Format(s, args);

		public static string clampLength(this string s, int length)
		{
			if (length < 5 || s.Length <= length)
				return s;

			return s.Remove(length / 2, s.Length - length + 3).Insert(length / 2, "...");
		}

		static string formatFileName(string filename)
		{
			if (filename.isNullOrEmpty())
				return filename;

			if (Path.GetExtension(filename) == "")
				filename += ".txt";

			if (!Path.IsPathRooted(filename))
				filename = Paths.modRootPath + filename;

			return filename;
		}

		public static void saveToFile(this string s, string localPath)
		{
			try { File.WriteAllText(formatFileName(localPath), s); }
			catch (Exception e) { Log.msg(e); }
		}

		public static void appendToFile(this string s, string localPath)
		{
			try { File.AppendAllText(formatFileName(localPath), s + Environment.NewLine); }
			catch (Exception e) { Log.msg(e); }
		}
	}


	class UniqueIDs
	{
		readonly HashSet<string> allIDs = new HashSet<string>();
		readonly Dictionary<string, int> nonUniqueIDs = new Dictionary<string, int>();

		public bool ensureUniqueID(ref string id)
		{
			if (allIDs.Add(id)) // if this is new id, do nothing
				return true;

			nonUniqueIDs.TryGetValue(id, out int counter);
			nonUniqueIDs[id] = ++counter;

			id += "." + counter;
#if DEBUG
			$"UniqueIDs: fixed ID: {id}".logWarning();
#endif
			Debug.assert(allIDs.Add(id)); // checking updated id just in case

			return false;
		}
	}
}