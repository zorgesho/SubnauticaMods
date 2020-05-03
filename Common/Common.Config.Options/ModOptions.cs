using System;
using System.Collections.Generic;

using UnityEngine;
using SMLHelper.V2.Options;
using SMLHelper.V2.Handlers;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		static Options instance = null;
		static string  optionsName = Mod.name;

		public enum Mode { Undefined, MainMenu, IngameMenu };

		public static Mode mode { get; private set; } = Mode.Undefined;
		static GameObject optionsPanelGameObject;

		readonly List<ModOption> modOptions = new List<ModOption>();

		public static void add(ModOption option)
		{
			if (instance == null)
			{
				registerLabel("Name", ref optionsName);
				OptionsPanelHandler.RegisterModOptions(instance = new Options());
			}

			instance.modOptions.Add(option);
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
				if (modOptions.Find(o => o.id == id) is ModOption target)
				{
					if (e is GameObjectCreatedEventArgs)
						target.onGameObjectChange((e as GameObjectCreatedEventArgs).GameObject);
					else
						target.onValueChange(e);
				}
			}
			catch (Exception ex) { Log.msg(ex); }
		}

		public override void BuildModOptions()
		{
			modOptions.ForEach(o => o.addOption(this));

			updateCurrentMode();
		}

		void updateCurrentMode()
		{
			if (optionsPanelGameObject != null) // panel gameobject created once per mode
				return;

			optionsPanelGameObject = UnityEngine.Object.FindObjectOfType<uGUI_OptionsPanel>().gameObject;

			if (optionsPanelGameObject.GetComponent<MainMenuOptions>())
				mode = Mode.MainMenu;
			else
			if (optionsPanelGameObject.GetComponent<IngameMenuPanel>())
				mode = Mode.IngameMenu;
			else
				mode = Mode.Undefined;
		}

		static void registerLabel(string id, ref string label, bool uiInternal = true) => // uiInternal - for UI labels
			label = LanguageHelper.add("idsOptions." + id, label, uiInternal);
	}
}