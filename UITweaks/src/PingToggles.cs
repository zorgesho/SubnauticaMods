using System;
using System.Linq;
using System.Collections;
using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

using Common;
using Common.Harmony;
using Common.GameSerialization;

namespace UITweaks
{
	using Debug = Common.Debug;

	static class PingToggles
	{
		static bool pingTogglesEnabled => Main.config.pdaTweaks.enabled && Main.config.pdaTweaks.pingTogglesEnabled;

		public static void setPingEnabled(PingType pingType, int colorIndex, bool enabled)
		{
			if (!pingTogglesEnabled)
				return;

			if (PingToggleToolbar.instance)
			{
				PingToggleToolbar.instance.pingStates.setPingState(pingType, colorIndex, enabled);
				PingToggleToolbar.instance.updateButtons();
			}
		}

		class PingToggleToolbar: MonoBehaviour
		{
			public static PingToggleToolbar instance { get; private set; }

			const float iconScale = 0.7f;
			const float groupSpacing = 10f;
			static readonly Color colorDisabled = new (0.4f, 0.4f, 0.4f, 0.5f);

			bool dirty = true;

			uGUI_PingTab pingTab;

			GameObject tertiaryPingButton;
			List<GameObject> primaryPingButtons, secondaryPingButtons;

			[Serializable]
			public class PingStates
			{
				const string saveName = "pings";

				readonly Dictionary<int, bool> pingEnabled = new();

				static int hash(PingType pingType, int colorIndex) => ((int)pingType << 8) | (colorIndex & 0xFF);

				public static PingStates create() => SaveLoad.load<PingStates>(saveName) ?? new PingStates();

				public void setPingState(PingType pingType, int colorIndex, bool enabled, bool save = true)
				{
					pingEnabled[hash(pingType, colorIndex)] = enabled;

					if (save)
						SaveLoad.save(saveName, this);
				}

				public bool? getPingState(PingType pingType, int colorIndex)
				{
					if (!pingTogglesEnabled)
						return true;

					bool typeExist = pingEnabled.TryGetValue(hash(pingType, -1), out bool typeEnabled);
					bool colorExist = pingEnabled.TryGetValue(hash(pingType, colorIndex), out bool colorEnabled);

					if (!typeExist && !colorExist)
						return null;

					return (typeEnabled || !typeExist) && (colorEnabled || !colorExist);
				}

				public bool? getPingState(PingInstance ping) => ping? getPingState(ping.pingType, ping.colorIndex): null;
			}
			public PingStates pingStates { get; private set; }

			void Awake()
			{																						"PingToggleToolbar: Awake()".logDbg();
				Debug.assert(instance == null);
				instance = this;

				pingTab = GetComponent<uGUI_PingTab>();
				Debug.assert(pingTab != null);

				GameObject content = gameObject.getChild("Content");
				GameObject buttonPrefab = content.getChild("ButtonAll");
				var btnPrefabPos = buttonPrefab.GetComponent<RectTransform>().localPosition;
				var btnPrefabSize = buttonPrefab.GetComponent<RectTransform>().rect.size;

				subscribeToPingManager(true);

				var pings = _createLayout("PingButtons", groupSpacing, true);
				pings.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
				pings.GetComponent<RectTransform>().sizeDelta = btnPrefabSize;
				pings.setParent(content, localPos: new Vector3(-btnPrefabPos.x + btnPrefabSize.x / 2f, btnPrefabPos.y, 0f));

				pingStates = PingStates.create();

				tertiaryPingButton = _addPingButton(Main.config.pdaTweaks.pingTypes.tertiary, -1, content);
				tertiaryPingButton.transform.localPosition = new Vector3(btnPrefabPos.x + btnPrefabSize.x + groupSpacing , btnPrefabPos.y, 0f);

				primaryPingButtons = _createPingButtons(Main.config.pdaTweaks.pingTypes.primary, "PrimaryPingButtons");
				secondaryPingButtons = _createPingButtons(Main.config.pdaTweaks.pingTypes.secondary, "SecondaryPingButtons");

				StartCoroutine(_initButtons());

				IEnumerator _initButtons()
				{
					yield return null; // wait one frame until uGUI_PingTab updates ping entries (in its LateUpdate())
					updateButtons();
				}

				static GameObject _createLayout(string name, float spacing, bool sizeControl = false)
				{
					GameObject layout = new (name);

					var hlg = layout.AddComponent<HorizontalLayoutGroup>();
					hlg.childControlHeight = hlg.childControlWidth = sizeControl;
					hlg.spacing = spacing;

					layout.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
					return layout;
				}

				GameObject _addPingButton(PingType pingType, int color, GameObject parent)
				{
					Debug.assert(pingStates != null); // buttons should be added after loading pingStates

					var btn = parent.createChild(buttonPrefab);

					Toggle.ToggleEvent toggleEvent = btn.GetComponent<Toggle>().onValueChanged = new();
					toggleEvent.AddListener(val => toggleButton(btn, color, pingType, val));

					btn.AddComponent<LayoutElement>();

					var eye = btn.getChild("Eye");
					eye.destroyComponent<Image>();

					var icon = eye.AddComponent<uGUI_Icon>();
					icon.rectTransform.localScale = new Vector3(iconScale, iconScale);
					icon.sprite = SpriteManager.Get(SpriteManager.Group.Pings, PingManager.sCachedPingTypeStrings.Get(pingType));

					bool? enabled = pingStates.getPingState(pingType, color);
					if (enabled == null)
						pingStates.setPingState(pingType, color, true, false);

					setButtonState(btn, color, enabled ?? true);

					return btn;
				}

				List<GameObject> _createPingButtons(PingType pingType, string layoutGroupName)
				{
					var layout = _createLayout(layoutGroupName, 0f);
					layout.setParent(pings);

					return Enumerable.Range(0, 5).Select(i => _addPingButton(pingType, i, layout)).ToList();
				}
			}

			void OnDestroy()
			{																						"PingToggleToolbar: OnDestroy()".logDbg();
				subscribeToPingManager(false);

				primaryPingButtons.ForEach(Destroy);
				secondaryPingButtons.ForEach(Destroy);
				Destroy(tertiaryPingButton);

				updateEntries();

				instance = null;
			}

			void LateUpdate()
			{
				if (dirty && !(dirty = false))
					updateButtons();
			}


			void subscribeToPingManager(bool subscribe)
			{
				if (subscribe)
				{
					PingManager.onAdd += _makeDirty;
					PingManager.onColor += _makeDirty;
					PingManager.onRemove += _makeDirty;
				}
				else
				{
					PingManager.onAdd -= _makeDirty;
					PingManager.onColor -= _makeDirty;
					PingManager.onRemove -= _makeDirty;
				}
			}
#if GAME_SN
			void _makeDirty(int _) => _makeDirty();
			void _makeDirty(int _1, Color _2) => _makeDirty();
			void _makeDirty(int _1, PingInstance _2) => _makeDirty();
#elif GAME_BZ
			void _makeDirty(string _) => _makeDirty();
			void _makeDirty(string _1, Color _2) => _makeDirty();
			void _makeDirty(PingInstance _) => _makeDirty();
#endif
			void _makeDirty()
			{																				"PingToggleToolbar: makeDirty()".logDbg();
				dirty = true;
			}

			void toggleButton(GameObject btn, int color, PingType pingType, bool val, bool updateEntries = true)
			{
				setButtonState(btn, color, val);
				pingStates.setPingState(pingType, color, val, updateEntries);

				if (updateEntries)
					this.updateEntries();
			}

			void setButtonState(GameObject btn, int color, bool val)
			{
				static Color _color(int colorIndex) => colorIndex != -1? PingManager.colorOptions[colorIndex]: Color.white;

				btn.GetComponent<Toggle>().isOn = val;
				btn.GetComponent<Image>().color = val? Color.white: _color(color);
				btn.GetComponentInChildren<uGUI_Icon>().color = val? _color(color): colorDisabled;
			}

			void updateEntries()
			{
				foreach (var entry in pingTab.entries)
				{
					bool? state = pingStates.getPingState(PingManager.Get(entry.Key));

					if (state == null || entry.Value.visibility.interactable == state)
						continue;

					bool enabled = (bool)state;
					entry.Value.visibility.isOn = enabled;
					entry.Value.visibility.interactable = enabled;
					entry.Value.visibility.gameObject.GetComponentsInChildren<Image>().forEach(image => image.color = enabled? Color.white: colorDisabled);

					entry.Value.label.color = enabled? Color.white: colorDisabled;
				}
			}

			// show buttons for existing pings only (pingType & color)
			public void updateButtons()
			{
				_updateButtons(primaryPingButtons, Main.config.pdaTweaks.pingTypes.primary);
				_updateButtons(secondaryPingButtons, Main.config.pdaTweaks.pingTypes.secondary);
				tertiaryPingButton.SetActive(_colors(Main.config.pdaTweaks.pingTypes.tertiary).Count > 0);

				updateEntries();

				List<int> _colors(PingType pingType) => // colors in use for pingType
					pingTab.entries.Keys.
						Select(id => PingManager.Get(id)).
						Where(ping => ping?.pingType == pingType).
						Select(ping => ping.colorIndex).
						Distinct().ToList();

				void _updateButtons(List<GameObject> buttons, PingType pingType)
				{
					var colors = _colors(pingType);

					for (int i = 0; i < 5; i++)
					{
						var btn = buttons[i];
						bool isActive = btn.activeSelf;
						bool isNeedActive = colors.Contains(i);

						btn.SetActive(isNeedActive);

						if (!isActive && isNeedActive) // if btn was hidden before we will show it as enabled
							toggleButton(btn, i, pingType, true, false);
						else
							toggleButton(btn, i, pingType, pingStates.getPingState(pingType, i) ?? true, false);
					}
				}
			}
		}

		[OptionalPatch, PatchClass]
		static class Patches
		{
			static bool prepare()
			{
				if (!pingTogglesEnabled)
					UnityEngine.Object.Destroy(PingToggleToolbar.instance);

				return pingTogglesEnabled;
			}

			[HarmonyPrefix, HarmonyPatch(typeof(uGUI_PingTab), "Open")]
			static void addToolbar(uGUI_PingTab __instance)
			{
				__instance.gameObject.ensureComponent<PingToggleToolbar>();
			}

			// skip disabled pings when ButtonAll clicked
			[HarmonyTranspiler, HarmonyPatch(typeof(uGUI_PingTab), "SetEntriesVisibility")]
			static IEnumerable<CodeInstruction> setEntriesVisibility(IEnumerable<CodeInstruction> cins, ILGenerator ilg)
			{
				var list = cins.ToList();
				int[] i = list.ciFindIndexes(OpCodes.Stloc_0,
											 OpCodes.Br,	// get label from it
											 OpCodes.Stloc_1,
											 OpCodes.Call);	// insert after this (call uGUI_PingEntry get_Value())
				if (i == null)
					return cins;

				CIHelper.LabelClipboard.__enabled = false;
				return list.ciInsert(i[3] + 1,
					OpCodes.Dup,
					CIHelper.emitCall<Func<uGUI_PingEntry, bool>>(_allowedToChange),
					OpCodes.Brtrue_S, list.ciDefineLabel(i[3] + 1, ilg),
					OpCodes.Pop,
					OpCodes.Br_S, list[i[1]].operand);

				static bool _allowedToChange(uGUI_PingEntry entry) => entry.visibility.interactable;
			}

			// compatibility patch for SubnauticaMap mod
			// hides ping icons from the map for disabled pings
			[HarmonyHelper.Patch(HarmonyHelper.PatchOptions.CanBeAbsent)]
			[HarmonyPostfix, HarmonyHelper.Patch("SubnauticaMap.Controller, SubnauticaMap" + (Mod.Consts.isGameBZ? "_BZ": ""), "UpdateIcons")]
			static void updateMapPings(object __instance)
			{
				if (!PingToggleToolbar.instance)
					return;

				object[] pingIcons = new object[PingManager.pings.Count];
				Traverse.Create(__instance).Field("pingMapIconList").Property("Values").Method("System.Collections.ICollection.CopyTo", pingIcons, 0).GetValue();

				foreach (var pingIcon in pingIcons)
				{
					var icon = Traverse.Create(pingIcon);
					bool? enabled = PingToggleToolbar.instance.pingStates.getPingState(icon.Field("ping").GetValue<PingInstance>());

					if (enabled != null)
						icon.Property("active").SetValue((bool)enabled);
				}
			}
		}
	}
}