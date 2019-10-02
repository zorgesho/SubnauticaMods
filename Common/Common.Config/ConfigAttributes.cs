using System;
using System.Reflection;

namespace Common.Config
{
	static class ConvertObjectExtensions
	{
		static public int toInt(this object obj) => Convert.ToInt32(obj);
		static public bool toBool(this object obj) => Convert.ToBoolean(obj);
		static public float toFloat(this object obj) => Convert.ToSingle(obj);
	}

	abstract class ConfigAttribute: Attribute
	{
		abstract public void process(object config);
	}
	
	abstract class ConfigFieldAttribute: Attribute
	{
		abstract public void process(object config, FieldInfo field);
	}
}