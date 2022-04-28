using UnityEngine;
using UnityEngine.UI;

using Common;

#if GAME_SN
using System.Collections;
#endif

namespace UITweaks.StorageTweaks
{
#if GAME_BZ
	using Text = TMPro.TMP_Text;
#endif

	static class StorageLabelFixers
	{
		public static bool tweakEnabled => Main.config.storageTweaks.enabled && Main.config.storageTweaks.multilineLabels;

		abstract class Fixer: MonoBehaviour
		{
			protected Text text;
			protected RectTransform textTransform;
			protected uGUI_InputField inputField;

			protected IStorageLabelInfo labelInfo;

			void valueListener(string _)
			{
				if (inputField.text.IndexOf('\n') != -1)
				{																							"StorageLabelFixers.Fixer: removing new line from the input".logDbg();
					inputField.text = inputField.text.Replace("\n", "");
					return;
				}

				using var __ = Common.Debug.profiler("StorageLabelFixer");

				fixLabel();
				ensureMaxLineCount(labelInfo.maxLineCount);
			}

			protected abstract void fixLabel();

			protected virtual void ensureMaxLineCount(int maxLineCount)
			{
#if GAME_SN
				string str = inputField.text;

				while (text.getLineCount() > maxLineCount)
				{
					str = str.Remove(str.Length - 1);
					text.forceRedraw(str);
				}

				if (inputField.text.Length != str.Length)
					inputField.text = str; // will call 'valueListener'
#elif GAME_BZ
				// SN approach doesn't work here because of the bug in TMP_Text (see 'Utils.forceRedraw')
				// will remove entire last word instead of just extra chars
				// also, can't use 'inputField.lineLimit', need more control over this
				if (text.getLineCount() > maxLineCount)
					inputField.text = text.text.Remove(text.getFirstCharIndexAtLine(maxLineCount));
#endif
			}

			void Awake()
			{
				if (!tweakEnabled)
					Destroy(this);
			}

			protected virtual void Start()
			{																								$"StorageLabelFixers.Fixer: Start (type: {GetType()})".logDbg();
				inputField = GetComponent<IStorageLabel>()?.label.signInput.inputField;
				labelInfo = GetComponent<IStorageLabelInfo>();

				if (!inputField)
				{
					Destroy(this);
					return;
				}

				text = inputField.textComponent;
				textTransform = text.transform as RectTransform;

				inputField.characterLimit = labelInfo.maxCharCount;
				text.GetComponent<ContentSizeFitter>().enabled = false;

				RectTransformExtensions.SetSize(textTransform, labelInfo.size.x, labelInfo.size.y);
#if GAME_SN
				text.alignment = TextAnchor.MiddleCenter;
				inputField.lineType = InputField.LineType.MultiLineSubmit;
#elif GAME_BZ
				text.alignment = TMPro.TextAlignmentOptions.Midline;
				inputField.lineType = TMPro.TMP_InputField.LineType.MultiLineSubmit;
#endif
				inputField.onValueChanged.AddListener(valueListener);
			}

			void OnDestroy()
			{
				inputField?.onValueChanged.RemoveListener(valueListener);
			}
		}


		[StorageHandler(TechType.SmallLocker)]
		class SmallLockerLabelFixer: Fixer
		{
			protected override void fixLabel()
			{
				text.forceRedraw(inputField.text);
				text.lineSpacing = text.getLineCount() > 3? (Mod.Consts.isGameSN? 0.7f: -35f): (Mod.Consts.isGameSN? 1f: 0f);
			}
		}


		[StorageHandler(TechType.SmallStorage)]
		class SmallStorageLabelFixer: Fixer
		{
			protected override void fixLabel()
			{
				text.forceRedraw(inputField.text);
				textTransform.localScale = new (1.0f, text.getLineCount() > 2? 1.2f: 1.7f, 1f);
			}

			protected override void ensureMaxLineCount(int maxLineCount)
			{
				const int maxSteps = 10;
				const float scaleStep = 0.05f;

				RectTransformExtensions.SetSize(textTransform, labelInfo.size.x, textTransform.rect.height);
				text.forceRedraw(inputField.text);

				// first, we'll try to make symbols narrower
				for (int i = 0; text.getLineCount() > maxLineCount && i < maxSteps; i++)
				{
					textTransform.localScale = textTransform.localScale.setX(textTransform.localScale.x - scaleStep);
					RectTransformExtensions.SetSize(textTransform, labelInfo.size.x / textTransform.localScale.x, textTransform.rect.height);
					text.forceRedraw(inputField.text);
				}

				// remove extra symbols if we still have too much lines
				if (text.getLineCount() > maxLineCount)
					base.ensureMaxLineCount(maxLineCount);
			}

			protected override void Start()
			{
#if GAME_SN
				if (inputField) // in case we already called 'Start' from 'OnDisable'
					return;
#endif
				base.Start();

				if (!inputField)
					return;

				text.lineSpacing = Mod.Consts.isGameSN? 0.7f: -35f;
#if GAME_SN
				storageModel = gameObject.getChild("../3rd_person_model");
				fixLabelPos();
#endif
			}

#if GAME_SN // additional fixing for label's position in SN
			Animator animator;
			GameObject storageModel;

			void fixLabelPos()
			{
				if (textTransform)
					textTransform.localPosition = storageModel.activeSelf? Vector3.zero: new (25f, -35f);
			}

			void OnEnable()
			{
				fixLabelPos();
			}

			void OnDisable()
			{
				if (!tweakEnabled)
					return;

				// 'OnDisable' can be called before 'Start' and nothing will be initialized
				// so we need this minor hack to do it ourselves
				if (!text)
					UWE.CoroutineHost.StartCoroutine(_startAndFix());
				else
					fixLabelPos();

				animator ??= gameObject.getChild("../3rd_person_model/floating_storage_cube_tp").GetComponent<Animator>();
				animator.Rebind();

				IEnumerator _startAndFix()
				{
					// just in case, to be sure that other components are initialized
					yield return null;

					Start();
					fixLabelPos();
				}
			}
#endif
		}

#if GAME_BZ
		abstract class SeaTruckLabelFixer: Fixer
		{
			protected abstract int lineTight { get; }
			protected virtual Vector2 labelPos => Vector2.zero;
			protected abstract Vector2 colorSelectorPos { get; }

			protected override void fixLabel()
			{
				text.forceRedraw(inputField.text);
				text.lineSpacing = text.getLineCount() > lineTight? -20f: 0f;
			}

			protected override void Start()
			{
				base.Start();

				if (inputField)
				{
					inputField.transform.localPosition = labelPos;
					inputField.transform.Find("../ColorSelector").localPosition = colorSelectorPos;
				}
			}
		}

		[StorageHandler(TechType.SeaTruckFabricatorModule)]
		class SeaTruckFabricatorFixer: SeaTruckLabelFixer
		{
			protected override int lineTight => 2;
			protected override Vector2 labelPos => new (110f, -30f);
			protected override Vector2 colorSelectorPos => new (-230f, -30f);
		}

		[StorageHandler(TechType.SeaTruckStorageModule)]
		class SeaTruckStorageFixer: MonoBehaviour
		{
			class FixerVertical: SeaTruckLabelFixer
			{
				protected override int lineTight => 4;
				protected override Vector2 colorSelectorPos => new (0f, -270f);
			}

			class FixerHorizontalBig: SeaTruckLabelFixer
			{
				protected override int lineTight => 2;
				protected override Vector2 labelPos => new (0f, -30f);
				protected override Vector2 colorSelectorPos => new (-400f, -30f);
			}

			class FixerHorizontalSmallLeft: SeaTruckLabelFixer
			{
				protected override int lineTight => 3;
				protected override Vector2 labelPos => new (-12f, 80f);
				protected override Vector2 colorSelectorPos => new (-245f, -50f);
			}

			class FixerHorizontalSmallRight: SeaTruckLabelFixer
			{
				protected override int lineTight => 3;
				protected override Vector2 labelPos => new (7f, 80f);
				protected override Vector2 colorSelectorPos => new (-225f, -50f);
			}

			void Start()
			{
				var fixerType = GetComponent<IStorageLabelInfo>()?.type switch
				{
					"Vertical" => typeof(FixerVertical),
					"HorizontalBig" => typeof(FixerHorizontalBig),
					"HorizontalSmallLeft" => typeof(FixerHorizontalSmallLeft),
					"HorizontalSmallRight" => typeof(FixerHorizontalSmallRight),
					_ => null
				};

				Common.Debug.assert(fixerType != null);

				if (fixerType != null)
					gameObject.AddComponent(fixerType);
			}
		}
#endif
	}
}