using System;
using System.Collections.Generic;

using SMLHelper.V2.Options;
using SMLHelper.V2.Handlers;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		static bool inited = false;

		static Config mainConfig = null;
		static string name = Strings.modName;
		static List<ModOption> modOptions = new List<ModOption>();

		static public void init()
		{																								"Config.Options is already inited!".logDbgError(inited);
			if (!inited)
			{
				inited = true;
				OptionsPanelHandler.RegisterModOptions(new Options());
			}
		}

		static void add(ModOption option)
		{
			modOptions.Add(option);
		}

		Options(): base(name)
		{
			ToggleChanged  += (object sender, ToggleChangedEventArgs e)  => eventHandler(e.Id, e);
			SliderChanged  += (object sender, SliderChangedEventArgs e)  => eventHandler(e.Id, e);
			ChoiceChanged  += (object sender, ChoiceChangedEventArgs e)  => eventHandler(e.Id, e);
			KeybindChanged += (object sender, KeybindChangedEventArgs e) => eventHandler(e.Id, e);
		}
		
		void eventHandler(string id, EventArgs e)
		{
			try
			{
				modOptions.Find((o) => o.id == id)?.onEvent(e);
			}
			catch (Exception ex)
			{
				Log.msg(ex);
			}
		}

		override public void BuildModOptions()
		{
			modOptions.ForEach((o) => o.addOption(this));
		}
	}	
}