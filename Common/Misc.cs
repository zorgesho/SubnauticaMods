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
	}


	static class ReflectionHelper
	{
		public static BindingFlags bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		public static Type getCallingType() => new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().ReflectedType;

		// for getting mod's defined types, don't include any of Common projects types (or types without namespace)
		public static readonly List<Type> definedTypes =
			Assembly.GetExecutingAssembly().GetTypes().Where(type => !(type.Namespace?.StartsWith(nameof(Common)) ?? true)).ToList();

		public static Type safeGetType(string assemblyName, string typeName)
		{
			try   { return Assembly.Load(assemblyName)?.GetType(typeName, false); }
			catch { return null; }
		}
	}


	static class TypeExtensions
	{
		static MethodInfo _method(this Type type, string name, Type[] types)
		{
			try { return types == null? type.GetMethod(name, ReflectionHelper.bfAll): type.GetMethod(name, types); }
			catch (AmbiguousMatchException)
			{
				$"Ambiguous method: {type.Name}.{name}".logError();
				return null;
			}
		}

		public static MethodInfo method(this Type type, string name) => _method(type, name, null);
		public static MethodInfo method(this Type type, string name, params Type[] types) => _method(type, name, types);

		public static FieldInfo field(this Type type, string name) => type.GetField(name, ReflectionHelper.bfAll);
		public static PropertyInfo property(this Type type, string name) => type.GetProperty(name, ReflectionHelper.bfAll);

		public static FieldInfo[] fields(this Type type) => type.GetFields(ReflectionHelper.bfAll);
		public static MethodInfo[] methods(this Type type) => type.GetMethods(ReflectionHelper.bfAll);
		public static MethodInfo[] methods(this Type type, BindingFlags bf) => type.GetMethods(ReflectionHelper.bfAll | bf);
		public static PropertyInfo[] properties(this Type type) => type.GetProperties(ReflectionHelper.bfAll);
	}


	static class MemberInfoExtensions
	{
		public static string fullName(this MemberInfo memberInfo) => (memberInfo == null)? "[null]": memberInfo.DeclaringType.FullName + "." + memberInfo.Name;

		public static A getAttribute<A>(this MemberInfo memberInfo, bool includeDeclaringTypes = false) where A: Attribute
		{
			A attr = Attribute.GetCustomAttribute(memberInfo, typeof(A)) as A;

			if (!includeDeclaringTypes)
				return attr;

			Type declaringType = memberInfo.DeclaringType;

			while (attr == null && declaringType != null)
			{
				attr = declaringType.getAttribute<A>();
				declaringType = declaringType.DeclaringType;
			}

			return attr;
		}

		public static A[]  getAttributes<A>(this MemberInfo memberInfo)  where A: Attribute => Attribute.GetCustomAttributes(memberInfo, typeof(A)) as A[];
		public static bool checkAttribute<A>(this MemberInfo memberInfo) where A: Attribute => Attribute.IsDefined(memberInfo, typeof(A));
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
			if (sequence != null)
			{
				var enumerator = sequence.GetEnumerator();
				while (enumerator.MoveNext())
					action(enumerator.Current);
			}
		}

		public static T[] init<T>(this T[] array) where T: new()
		{
			for (int i = 0; i < array.Length; i++)
				array[i] = new T();
			return array;
		}

		public static int findIndex<T>(this T[] array, int beginIndex, int endIndex, Predicate<T> predicate) =>
			Array.FindIndex(array, beginIndex, endIndex - beginIndex, predicate);

		public static int findIndex<T>(this T[] array, Predicate<T> predicate) =>
			Array.FindIndex(array, predicate);

		public static T createDelegate<T>(this DynamicMethod dm) where T: class => dm.CreateDelegate(typeof(T)) as T;
	}


	static partial class StringExtensions
	{
		public static bool isNullOrEmpty(this string s) => (s == null || s == "");

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

			id += "." + counter;																		$"UniqueIDs: fixed ID: {id}".logWarning();

			Debug.assert(allIDs.Add(id)); // checking updated id just in case

			return false;
		}
	}
}