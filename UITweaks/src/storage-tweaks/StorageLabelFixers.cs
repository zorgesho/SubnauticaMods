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
		abstract class Fixer: MonoBehaviour
		{
			protected Text text;
			protected RectTransform textTransform;
			protected uGUI_InputField inputField;

			void valueListener(string _) => fixLabel();
			protected abstract void fixLabel();

			protected virtual void Start()
			{
				inputField = GetComponent<IStorageLabel>()?.label.signInput.inputField;

				if (!inputField)
				{
					Destroy(this);
					return;
				}

				text = inputField.textComponent;
				textTransform = text.transform as RectTransform;

				inputField.characterLimit = 80;
				text.GetComponent<ContentSizeFitter>().enabled = false;

				RectTransformExtensions.SetSize(textTransform, textTransform.rect.width, 200f);
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
				using var _ = Common.Debug.profiler("SmallLockerLabelFixer");

				text.forceRedraw(inputField.text); // at this point text in 'text' component is not yet updated

				int lineCount = text.getLineCount();
				text.lineSpacing = lineCount > 3? (Mod.Consts.isGameSN? 0.7f: -35f): (Mod.Consts.isGameSN? 1f: 0f);

				ensureMaxLineCount(4);
			}

			void ensureMaxLineCount(int maxLineCount)
			{
#if GAME_SN
				int charsToRemove = 0;

				while (text.getLineCount() > maxLineCount)
					text.forceRedraw(text.text.Remove(text.text.Length - ++charsToRemove));

				if (charsToRemove > 0)
					inputField.text = text.text; // will call 'fixLabel'
#elif GAME_BZ
				// SN approach doesn't work here because of the bug in TMP_Text (see 'Utils.forceRedraw')
				// will remove entire last word instead of just extra chars
				if (text.getLineCount() > maxLineCount)
					inputField.text = text.text.Remove(text.getFirstCharIndexAtLine(maxLineCount));
#endif
			}
		}


		[StorageHandler(TechType.SmallStorage)]
		class SmallStorageLabelFixer: Fixer
		{
			const float labelWidth = 270f;

			protected override void fixLabel()
			{
				using var _ = Common.Debug.profiler("SmallStorageLabelFixer");

				text.forceRedraw(inputField.text);

				int lineCount = text.getLineCount();
				textTransform.localScale = new (1.0f, lineCount > 2? 1.2f: 1.7f, 1f);

				ensureMaxLineCount(3);
			}

			void ensureMaxLineCount(int maxLineCount)
			{
				const int maxSteps = 10;
				const float scaleStep = 0.05f;

				RectTransformExtensions.SetSize(textTransform, labelWidth, textTransform.rect.height);
				text.forceRedraw(inputField.text);

				for (int i = 0; text.getLineCount() > maxLineCount && i < maxSteps; i++)
				{
					textTransform.localScale = textTransform.localScale.setX(textTransform.localScale.x - scaleStep);
					RectTransformExtensions.SetSize(textTransform, labelWidth / textTransform.localScale.x, textTransform.rect.height);
					text.forceRedraw(inputField.text);
				}
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
				RectTransformExtensions.SetSize(textTransform, labelWidth, textTransform.rect.height);
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
	}
}