﻿using System;
using System.Reflection;

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
		public object[] getArgs() => args;

		public void action()
		{
			if (args.isNullOrEmpty())
				OptionalPatches.update();
			else
				args.forEach(arg => OptionalPatches.update(arg as Type));
		}
	}

	// action for calling config method (not for use with nested classes)
	// first arg is the name of the method, other args if for calling method
	class CallMethod: Config.Field.IAction, Config.Field.IActionArgs, Config.IRootConfigInfo
	{
		MethodInfo targetMethod;

		object[] args, argsMethod;
		public void setArgs(object[] args)
		{
			Debug.assert(!args.isNullOrEmpty());

			this.args = args;
			argsMethod = args.Length == 1? null: args.subArray(1);
		}
		public object[] getArgs() => args;

		Config rootConfig;
		public void setRootConfig(Config config) => rootConfig = config;

		public void action()
		{
			Debug.assert(!args.isNullOrEmpty() && args[0] is string);
			Debug.assert(rootConfig != null);

			targetMethod ??= rootConfig.GetType().method(args[0] as string);
			Debug.assert(targetMethod != null);

			targetMethod.Invoke(rootConfig, argsMethod);
		}
	}
}