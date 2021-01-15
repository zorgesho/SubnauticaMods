using System;

namespace Common.Configuration.Actions
{
	using Harmony;
	using Reflection;

	// action for updating optional patches
	// updates all patches if used without params or only selected patches
	class UpdateOptionalPatches: Config.Field.IAction, Config.Field.IActionArgs
	{
		object[] args;
		public void setArgs(object[] args) => this.args = args;

		public void action()
		{
			if (args.isNullOrEmpty())
				OptionalPatches.update();
			else
				args.forEach(arg => OptionalPatches.update(arg as Type));
		}
	}
}