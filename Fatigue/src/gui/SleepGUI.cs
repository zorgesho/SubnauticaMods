using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using Common;
using Common.UI;

#pragma warning disable

namespace Fatigue
{
	class SleepGUI: uGUI_InputGroup
	{
		public static SleepGUI main => _main ?? create();
		static SleepGUI _main = null;

		public enum State
		{
			Disabled,
			FadeIn,
			Enabled,
			Sleeping,
			FadeOut
		}

		public State state
		{
			get => _state;

			private set
			{
				_state = value;																		$"SleepGUI set state {value}".logDbg();

				switch (_state)
				{
					case State.Disabled:
						setVisible(false);

						break;
					case State.FadeIn:
						setVisible(true);

						canvasGroupUI.alpha = 0f;

						if (FPSInputModule.current)
							FPSInputModule.current.lockPauseMenu = true;

						break;
					case State.Enabled:
							canvasGroupUI.alpha = 0.5f;
							setControlsEnabled(true);

						break;
					case State.Sleeping:
							timeStartSleep = DayNightCycle.main.timePassedAsFloat;
							setControlsEnabled(false);

						break;
					case State.FadeOut:
						if (FPSInputModule.current)
							FPSInputModule.current.lockPauseMenu = false;

						break;
				}

				stateChangeEvent.Invoke(_state);
			}
		}
		State _state = State.Disabled;

		public class StateChangeEvent: UnityEvent<State> {}
		public StateChangeEvent stateChangeEvent { get; private set; } = new StateChangeEvent();

		// UI elements
		GameObject rootUI = null;
		CanvasGroup canvasGroupUI = null;

		Text currentTimeText = null;
		uGUI_SnappingSlider slider = null;
		GameObject wakeupLabel = null;
		GameObject sleepLabel = null;


		float timeStartSleep = 0;
		const float fadeInSpeed = 1f;
		const float fadeOutSpeed = 1f;


		public static SleepGUI create()
		{
			$"Try creating SleepGUI: uGUI_PlayerSleep '{uGUI_PlayerSleep.main}' uGUI_MainMenu '{uGUI_MainMenu.main}' IngameMenu '{IngameMenu.main}'".logDbg();

			if (!_main && uGUI_PlayerSleep.main && (uGUI_MainMenu.main || IngameMenu.main)) // we using uGUI_MainMenu and IngameMenu for ui prefabs
				_main = uGUI_PlayerSleep.main.gameObject.AddComponent<SleepGUI>();					if (_main)"SleepGUI added".logDbg();

			return _main;
		}

		public override void Awake()
		{																							"SleepGUI awake".logDbg();
			base.Awake();
			init();

			rootUI.SetActive(false); // do not use setVisible at this time
		}

		void OnDestroy()
		{
			_main = null;																			"SleepGUI destoryed".logDbg();
		}

		public override void Update()
		{
			base.Update();

			if (state == State.FadeIn)
			{
				canvasGroupUI.alpha += Time.deltaTime / fadeInSpeed;

				if (canvasGroupUI.alpha > 0.98f)
				{
					canvasGroupUI.alpha = 1f;
					state = State.Enabled;
				}
			}
			else if (state == State.FadeOut)
			{
				canvasGroupUI.alpha -= Time.deltaTime / fadeOutSpeed;

				if (canvasGroupUI.alpha < 0.02f)
				{
					canvasGroupUI.alpha = 0f;
					state = State.Disabled;
				}
			}
			else if (state == State.Sleeping)
			{
				if (DayNightCycle.main.timePassedAsFloat - timeStartSleep > 3600.0f / DayNightCycle.gameSecondMultiplier)
				{
					slider.value -= 1;
					timeStartSleep = DayNightCycle.main.timePassedAsFloat;
				}
			}

			currentTimeText.text = DayNightCycle.ToGameDateTime(DayNightCycle.main?.timePassedAsFloat ?? 0).ToString();
		}

		void setControlsEnabled(bool state)
		{
			slider.enabled = state;
			sleepLabel.GetComponent<EventTrigger>().enabled = state;
		}


		public void setVisible(bool state)
		{
			if (state)
				base.Select(true);
			else
				base.Deselect();

			rootUI.SetActive(state);
		}


		public void start()
		{																						"SleepGUI.start called".logDbg();
			if (state != State.FadeIn && state != State.Enabled)
				state = State.FadeIn;
		}


		public void stop()
		{																						"SleepGUI.stop called".logDbg();
			if (state != State.Disabled && state != State.FadeOut)
				state = State.FadeOut;
		}


		void startSleepMode(float hoursToSleep)
		{
			PlayerSleep playerSleep = Player.main.gameObject.GetComponent<PlayerSleep>();

			UnityAction<bool> sleepChangeEventAction = null;
			sleepChangeEventAction = new UnityAction<bool>((state) =>
			{
				if (!state)
				{
					stop();
					playerSleep.sleepChangeEvent.RemoveListener(sleepChangeEventAction);
				}
			});

			playerSleep.sleepChangeEvent.AddListener(sleepChangeEventAction);
			playerSleep.startSleep(hoursToSleep);
			state = State.Sleeping;
		}


		void stopSleepMode()
		{
			PlayerSleep playerSleep = Player.main.gameObject.GetComponent<PlayerSleep>();

			if (playerSleep.isSleeping)
				playerSleep.stopSleep();
			else
				stop();
		}


		void init()
		{
			gameObject.destroyChild("Text", false);

			rootUI = new GameObject("UI", typeof(RectTransform));
			rootUI.setParent(uGUI_PlayerSleep.main.gameObject);

			canvasGroupUI = rootUI.AddComponent<CanvasGroup>();

			GameObject blackOverlay = gameObject.getChild("BlackOverlay");
			blackOverlay.setParent(rootUI);
			blackOverlay.transform.SetAsFirstSibling();
			blackOverlay.GetComponent<Image>().color = new Color(0, 0, 0, 1);

			uGUI_OptionsPanel prefabsContainer = null;

			if (uGUI_MainMenu.main)
				prefabsContainer = uGUI_MainMenu.main.gameObject.getChild("Panel/Options").GetComponent<uGUI_OptionsPanel>();
			else if (IngameMenu.main)
				prefabsContainer = IngameMenu.main.gameObject.getChild("Options").GetComponent<uGUI_OptionsPanel>();

			if (prefabsContainer)
			{
				addInputBlocker(); // must be added first

				addText();
				addText1();
				addText2();

				slider = addSlider(prefabsContainer.sliderOptionPrefab, new Vector3(0f, 100f, 0f), 500);
				//addButtons(prefabsContainer.sliderOptionPrefab);
				//addButtons(prefabsContainer.buttonPrefab);


				//Common.ObjectDumper.Dump(prefabsContainer.buttonPrefab);
			}
			else
				"uGUI_OptionsPanel prefabsContainer = null".logError();

//				Console.WriteLine("--------------------uGUI_OptionsPanel prefabsContainer = null !!!!!!!!!!!");

		//	Common.ObjectDumper.Dump(sleepScreenRoot);
		}


		uGUI_SnappingSlider addSlider(GameObject prefab, Vector3 position, float width)
		{
			GameObject sliderGO = GameObject.Instantiate(prefab);

			sliderGO.setParent(rootUI);

			sliderGO.transform.localPosition = position;//new Vector3(0f, 100f, 0f);
			sliderGO.GetComponent<RectTransform>().setWidth(width);

			uGUI_SnappingSlider slider = sliderGO.GetComponentInChildren<uGUI_SnappingSlider>();
			if (slider)
			{
				slider.minValue = 1;
				slider.maxValue = 10;
				slider.value = 4;
				slider.defaultValue = 8;
			}

			TranslationLiveUpdate caption = sliderGO.getChild("Slider/Caption").GetComponent<TranslationLiveUpdate>();
			caption.translationKey = "Hours to sleep:";

			// change textures
			GameObject back = sliderGO.getChild("Slider/Background/Handle Slide Area/Handle");
			Image image = back.GetComponent<Image>();



			//Texture2D newTexture = ImageUtils.loadTextureFromFile((ModPath)"assets\\slider_nub.png");

				//Texture2D newTexture = assets.LoadAsset<Texture2D>("slider_nub");
				//Texture2D newTexture = assets.LoadAsset<Texture2D>("bar_background");
				//Texture2D newTexture = AssetsHelper.loadTexture("bar_background");
				//$"{newTexture} 11".log();


			//ErrorMessage.AddDebug("new tex:" + newTexture.width);

			//image.sprite = AssetsHelper.textureToSprite(newTexture);
			//image.sprite = AssetsHelper.loadSprite("slider_nub");

			//Texture2D newTexture = AssetsHelper.load<Texture2D>("slider_nub");


			image.sprite = AssetsHelper.loadSprite("slider_nub");

			return slider;
		}



		void addText()
		{
			var textPrefab = HandReticle.main.interactPrimaryText;

			GameObject dbgText0 = new GameObject("SleepText", typeof(RectTransform));
			dbgText0.setParent(rootUI);

			Text text = dbgText0.AddComponent<Text>();

			currentTimeText = text;
			//text.rectTransform.setSize(500, 500);

			text.font = textPrefab.font;
			text.color = Color.white;
			text.fontSize = 50;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "You sleeping: 2h";
			text.raycastTarget = true;
			text.enabled = true;

			ContentSizeFitter ss = dbgText0.AddComponent<ContentSizeFitter>();
			ss.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			ss.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			EventTrigger trigger = dbgText0.AddComponent<EventTrigger>();

			trigger.addListener(EventTriggerType.PointerEnter, (eventData) => { text.color = new Color(1, 0, 0, 1); });
			trigger.addListener(EventTriggerType.PointerExit, (eventData) => { text.color = new Color(1, 1, 1, 1); });
		}
		void addText1()
		{
			var textPrefab = HandReticle.main.interactPrimaryText;

			GameObject dbgText0 = new GameObject("SleepText1", typeof(RectTransform));
			dbgText0.setParent(rootUI);

			wakeupLabel = dbgText0;

			Text text = dbgText0.AddComponent<Text>();

			//text.rectTransform.setSize(500, 500);
			dbgText0.transform.localPosition = new Vector3(-100, -100, 0);

			text.font = textPrefab.font;
			text.color = Color.white;
			text.fontSize = 30;
			text.alignment = TextAnchor.MiddleCenter;
			//text.text = "Wake up (ESC)";
			text.text = "Wake up (<color=#ADF8FFFF>Esc</color>)";
		//"Press <color=#ADF8FFFF>{0}</color> to detonate torpedoes remotely.", MainPatcher.config.hotkey)
			text.raycastTarget = true;
			text.enabled = true;

			ContentSizeFitter ss = dbgText0.AddComponent<ContentSizeFitter>();
			ss.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			ss.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			EventTrigger trigger = dbgText0.AddComponent<EventTrigger>();

			trigger.addListener(EventTriggerType.PointerEnter, (eventData) => { text.color = new Color(1, 1, 0, 1); });
			trigger.addListener(EventTriggerType.PointerExit, (eventData) => { text.color = new Color(1, 1, 1, 1); });
			trigger.addListener(EventTriggerType.PointerClick, (eventData) => { trigger.OnPointerExit(null); stopSleepMode(); });
		}

		void addText2()
		{
			var textPrefab = HandReticle.main.interactPrimaryText;

			GameObject dbgText0 = new GameObject("SleepText2", typeof(RectTransform));
			dbgText0.setParent(rootUI);

			sleepLabel = dbgText0;

			Text text = dbgText0.AddComponent<Text>();

			//text.rectTransform.setSize(500, 500);
			dbgText0.transform.localPosition = new Vector3(100, -100, 0);

			text.font = textPrefab.font;
			text.color = Color.white;
			text.fontSize = 30;
			text.alignment = TextAnchor.MiddleCenter;
			//text.text = "Wake up (ESC)";
			text.text = "Sleep (<color=#ADF8FFFF>S</color>)";
		//"Press <color=#ADF8FFFF>{0}</color> to detonate torpedoes remotely.", MainPatcher.config.hotkey)
			text.raycastTarget = true;
			text.enabled = true;

			ContentSizeFitter ss = dbgText0.AddComponent<ContentSizeFitter>();
			ss.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			ss.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			EventTrigger trigger = dbgText0.AddComponent<EventTrigger>();

			trigger.addListener(EventTriggerType.PointerEnter, (eventData) => { text.color = new Color(1, 0, 1, 1); });
			trigger.addListener(EventTriggerType.PointerExit, (eventData) => { text.color = new Color(1, 1, 1, 1); });
	//		trigger.addListener(EventTriggerType.PointerClick, (eventData) => { setSliderEnabled(slider, false); });
			trigger.addListener(EventTriggerType.PointerClick, (eventData) => { trigger.OnPointerExit(null); startSleepMode(slider.value); });
		}

		void setSliderEnabled(GameObject slider, bool enabled)
		{
			uGUI_SnappingSlider s = slider.getChild("Slider").GetComponent<uGUI_SnappingSlider>();
			s.enabled = enabled;
			s.value =7;
		}


		void addButtons(GameObject prefab)
		{
			GameObject newButton = GameObject.Instantiate(prefab);

			newButton.setParent(rootUI);
			newButton.transform.localPosition = new Vector3(0f, -100f, 0f);

			//newButton.transform.localPosition = new Vector3(0f, 0f, 0f);

			newButton.GetComponent<RectTransform>().setSize(500, 500);
		}


		void addInputBlocker()
		{
			GameObject inputBlocker = new GameObject("InputBlocker", typeof(uGUI_Block));
			inputBlocker.setParent(rootUI);
			inputBlocker.GetComponent<RectTransform>().setSize(1500, 1500);

			//EventTrigger trigger = inputBlocker.AddComponent<EventTrigger>();

			//trigger.addListener(EventTriggerType.Scroll, (eventData) => { ErrorMessage.AddDebug("scroll"); });
			//trigger.addListener(EventTriggerType.PointerExit, (eventData) => { text.color = new Color(1, 1, 1, 1); });
		}


		void changeSliderBack(GameObject slider)
		{
			GameObject back = slider.getChild("Slider/Background/Handle Slide Area/Handle");
			Image image = back.GetComponent<Image>();

			//Sprite s = image.sprite;

			//Texture2D newTexture = ImageUtils.loadTextureFromFile(Environment.CurrentDirectory + "\\QMods\\Fatigue\\assets\\SliderNub.png");
			//Texture2D newTexture = ImageUtils.loadTextureFromFile(Path.modRootPath + "assets\\SliderNub.png");
			//Path pp = "assets\\SliderNub.png";
			//Texture2D newTexture = ImageUtils.loadTextureFromFile((ModPath)"assets\\SliderNub.png");
//			Texture2D newTexture = ImageUtils.loadTextureFromFile(new Path("assets\\SliderNub.png"));

			//ErrorMessage.AddDebug("new tex:" + newTexture.width);

			//image.sprite = ImageUtils.TextureToSprite(newTexture);
		}
	}
}