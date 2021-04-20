using System;
using System.Collections.Generic;

using SMLHelper.V2.Options;
using SMLHelper.V2.Handlers;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		static Options instance = null;
		static string  optionsName = Mod.name;

		public enum Mode { Undefined, MainMenu, IngameMenu };

		public static Mode mode
		{
			get
			{
				if (uGUI_MainMenu.main) return Mode.MainMenu;
				if (IngameMenu.main)	return Mode.IngameMenu;
				return Mode.Undefined;
			}
		}
		static int modsTabIndex = -1;
		static uGUI_OptionsPanel optionsPanel;
		static uGUI_TabbedControlsPanel.Tab modOptionsTab => optionsPanel.tabs[modsTabIndex];

		readonly List<ModOption> modOptions = new();

		public static void add(ModOption option)
		{
			if (instance == null)
			{
				registerLabel("Name", ref optionsName);
				OptionsPanelHandler.RegisterModOptions(instance = new Options());
			}

			instance.modOptions.Add(option);
		}

		public static void remove(ModOption option)
		{
			Debug.assert(instance != null);

			option.onRemove();
			instance.modOptions.Remove(option);
		}

		Options(): base(optionsName)
		{
			ToggleChanged  += (object sender, ToggleChangedEventArgs e)  => eventHandler(e.Id, e);
			SliderChanged  += (object sender, SliderChangedEventArgs e)  => eventHandler(e.Id, e);
			ChoiceChanged  += (object sender, ChoiceChangedEventArgs e)  => eventHandler(e.Id, e);
			KeybindChanged += (object sender, KeybindChangedEventArgs e) => eventHandler(e.Id, e);

			GameObjectCreated += (object sender, GameObjectCreatedEventArgs e) => eventHandler(e.Id, e);
		}

		void eventHandler(string id, EventArgs e)
		{
			try
			{
				if (modOptions.Find(o => o.id == id) is not ModOption target)
					return;

				if (e is GameObjectCreatedEventArgs)
					target.onGameObjectChange((e as GameObjectCreatedEventArgs).GameObject);
				else
					target.onValueChange(e);
			}
			catch (Exception ex) { Log.msg(ex); }
		}

		public override void BuildModOptions()
		{
			updatePanelInfo();
			modOptions.ForEach(o => o.addOption(this));
		}

		void updatePanelInfo()
		{
			if (!optionsPanel)
			{
				optionsPanel = UnityEngine.Object.FindObjectOfType<uGUI_OptionsPanel>();
				modsTabIndex = optionsPanel.tabs.FindIndex(tab => tab.tab.GetComponentInChildren<TranslationLiveUpdate>().translationKey == "Mods");
			}

			Debug.assert(optionsPanel && modsTabIndex != -1);
		}

		static void registerLabel(string id, ref string label, bool uiInternal = true) => // uiInternal - for UI labels
			label = LanguageHelper.add("idsOptions." + id, label, uiInternal);
	}
}