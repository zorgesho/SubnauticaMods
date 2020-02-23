using System;
using System.Collections.Generic;

using SMLHelper.V2.Options;
using SMLHelper.V2.Handlers;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		static Options instance = null;
		static string  optionsName = Strings.modName;

		readonly List<ModOption> modOptions = new List<ModOption>();

		[Obsolete]
		public static void init() {}

		public static void add(ModOption option)
		{
			if (instance == null)
				OptionsPanelHandler.RegisterModOptions(instance = new Options());

			instance.modOptions.Add(option);
		}

		Options(): base(optionsName)
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
				modOptions.Find(o => o.id == id)?.onEvent(e);
			}
			catch (Exception ex) { Log.msg(ex); }
		}

		public override void BuildModOptions()
		{
			modOptions.ForEach(o => o.addOption(this));
		}

		// for using SMLHelper's language override files
		static void registerLabel(string id, ref string label)
		{
			id = Strings.modName + ".ids_options_" + id;
			LanguageHandler.SetLanguageLine(id, label);
			label = id;
		}
	}
}