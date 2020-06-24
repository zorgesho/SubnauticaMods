using System.Reflection;

namespace Common.Configuration.Utils
{
	class FieldRanged: Config.Field
	{
		readonly RangeAttribute range;

		public FieldRanged(object parent, FieldInfo field, Config rootConfig):
			base(parent, field, rootConfig)
		{
			range = getAttr<RangeAttribute>();
		}

		public override object value
		{
			set => base.value = range != null? range.clamp(value): value;
		}
	}
}