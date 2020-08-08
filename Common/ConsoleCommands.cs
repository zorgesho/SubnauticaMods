using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common
{
	using Reflection;

	// base class for console commands which are exists between scenes
	abstract class PersistentConsoleCommands: MonoBehaviour
	{
		protected class CommandAttribute: Attribute
		{
			public bool caseSensitive = false;
			public bool combineArgs = false;
		}

		class CommandInfo
		{
			public MethodInfo method;
			public ParameterInfo[] paramInfo;
			public int requiredParamCount;

			public bool caseSensitive, combineArgs;
		}

		static GameObject hostGO;

		readonly Dictionary<string, CommandInfo> commands = new Dictionary<string, CommandInfo>();
		Component commandProxy;

		public static void register<T>() where T: PersistentConsoleCommands
		{
			hostGO ??= UnityHelper.createPersistentGameObject($"{Mod.id}.ConsoleCommands");
			hostGO.ensureComponent<T>();
		}

		void Awake()
		{
			const BindingFlags bf = ReflectionHelper.bfAll ^ BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

			var methods = GetType().methods(bf);

			if (methods.Length == 0) // if there are no public methods, try declared private methods
				methods = GetType().methods(bf | BindingFlags.NonPublic);

			methods.forEach(method => addCommand(method));

			commandProxy = gameObject.AddComponent(CommandProxy.create(this));

			SceneManager.sceneUnloaded += onSceneUnloaded;
			registerCommands();
		}

		void addCommand(MethodInfo method)
		{
			var paramInfo = method.GetParameters();
			int requiredParamCount = 0;

			for (int i = 0; i < paramInfo.Length; i++)
			{
				// parameter can be omitted if it's nullable or it have default value
				if (Nullable.GetUnderlyingType(paramInfo[i].ParameterType) == null && paramInfo[i].DefaultValue == DBNull.Value)
					requiredParamCount = i + 1;
			}

			var cmdAttr = method.getAttr<CommandAttribute>();

			commands[method.Name] = new CommandInfo()
			{
				method = method,
				paramInfo = paramInfo,
				requiredParamCount = requiredParamCount,
				caseSensitive = cmdAttr?.caseSensitive ?? false,
				combineArgs =  cmdAttr?.combineArgs ?? false
			};															$"PersistentConsoleCommands: command added: '{method.Name}', required params count: {requiredParamCount}".logDbg();
		}

		void registerCommands() // double registration is checked inside DevConsole
		{
			foreach (var cmd in commands)
				DevConsole.RegisterConsoleCommand(commandProxy, cmd.Key, cmd.Value.caseSensitive, cmd.Value.combineArgs);
		}

		void runCommand(string cmd, Hashtable data) // 'data' can be null
		{
			var cmdInfo = commands[cmd];

			if ((data?.Count ?? 0) < cmdInfo.requiredParamCount && $"Not enough parameters for console command '{cmd}'".logError())
				return;

			ParameterInfo[] paramInfo = cmdInfo.paramInfo;
			object[] cmdParams = new object[paramInfo.Length];

			if (paramInfo.Length == 1 && paramInfo[0].ParameterType == typeof(Hashtable)) // using raw data
			{
				cmdParams[0] = data;
			}
			else
			{
				for (int i = 0; i < paramInfo.Length; i++)
				{
					object param = data?[i];

					if (param == null && paramInfo[i].DefaultValue != DBNull.Value)
						cmdParams[i] = paramInfo[i].DefaultValue;
					else
						cmdParams[i] = param.convert(paramInfo[i].ParameterType); // it's ok if 'param' is null here
				}
			}

			cmdInfo.method.Invoke(this, cmdParams);
		}

		// notifications are cleared between some scenes, so we need to reregister commands
		void onSceneUnloaded(Scene _) => registerCommands();

		void OnDestroy() => SceneManager.sceneUnloaded -= onSceneUnloaded;


		// internal class for creating dynamic proxies for console commands
		// returns Type that can be used for creating component for registering in DevConsole
		// proxy component routes calls for OnConsoleCommand_<cmdname> methods to PersistentConsoleCommands.runCommand
		static class CommandProxy
		{
			const string cmdPrefix = "OnConsoleCommand_";
			const string nameSuffix = "ConsoleCommands";

			const string fieldnameHost = "host";
			const string fieldnameRunCommand = "runCommand";

			static AssemblyBuilder assemblyBuilder;
			static ModuleBuilder moduleBuilder;

			public static Type create(PersistentConsoleCommands host)
			{
				assemblyBuilder ??= AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName($"{Mod.id}.{nameSuffix}"), AssemblyBuilderAccess.Run);
				moduleBuilder	??= assemblyBuilder.DefineDynamicModule(nameSuffix);

				var typeBuilder = moduleBuilder.DefineType($"{host.GetType().FullName}.CommandProxy", TypeAttributes.Public, typeof(MonoBehaviour));

				const FieldAttributes fa = FieldAttributes.Static | FieldAttributes.Private;
				var fieldHost = typeBuilder.DefineField(fieldnameHost, typeof(PersistentConsoleCommands), fa);
				var fieldRunCommand = typeBuilder.DefineField(fieldnameRunCommand, typeof(MethodInfo), fa);

				host.commands.Keys.forEach(cmd => addMethod(typeBuilder, cmd, fieldHost, fieldRunCommand));

				Type proxyType = typeBuilder.CreateType();

				// can't use FieldBuilder for setting value :(
				proxyType.field(fieldnameHost).SetValue(null, host);
				proxyType.field(fieldnameRunCommand).SetValue(null, typeof(PersistentConsoleCommands).method(nameof(runCommand)));

				return proxyType;
			}

			static void addMethod(TypeBuilder builder, string cmdname, FieldInfo fieldHost, FieldInfo fieldRunCommand)
			{
				// create method 'public void OnConsoleCommand_<cmdname>(NotificationCenter.Notification cmdparams)'
				Type[] args = { typeof(NotificationCenter.Notification) };
				var method = builder.DefineMethod(cmdPrefix + cmdname, MethodAttributes.Public, CallingConventions.HasThis, null, args);

				// we will call PersistentConsoleCommands.runCommand(cmdname, cmdparams.data) in this method
				// reflection is used to keep PersistentConsoleCommands internal
				ILGenerator ilg = method.GetILGenerator();
				ilg.Emit(OpCodes.Ldsfld, fieldRunCommand);
				ilg.Emit(OpCodes.Ldsfld, fieldHost);

				ilg.Emit(OpCodes.Ldc_I4_2); // prepare array for params
				ilg.Emit(OpCodes.Newarr, typeof(object));
				ilg.Emit(OpCodes.Dup);
				ilg.Emit(OpCodes.Dup);

				ilg.Emit(OpCodes.Ldc_I4_0); // first param, command name (without prefix)
				ilg.Emit(OpCodes.Ldstr, cmdname);
				ilg.Emit(OpCodes.Stelem_Ref);

				ilg.Emit(OpCodes.Ldc_I4_1); // second param, data from notification
				ilg.Emit(OpCodes.Ldarg_1);
				ilg.Emit(OpCodes.Ldfld, typeof(NotificationCenter.Notification).field("data"));
				ilg.Emit(OpCodes.Stelem_Ref);

				ilg.Emit(OpCodes.Callvirt, typeof(MethodBase).method("Invoke", typeof(object), typeof(object[])));
				ilg.Emit(OpCodes.Pop);
				ilg.Emit(OpCodes.Ret);
			}
		}
	}
}