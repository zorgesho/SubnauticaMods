using UnityEngine;
using UnityEngine.UI;

using Common;
using Common.UI;

namespace Fatigue
{
	using static Common.AssetsHelper;

	class uGUI_EnergyBar: uGUI_StatsBar
	{
		public override float getUpdatedValue()
		{
			EnergySurvival energySurvival = Player.main?.GetComponent<EnergySurvival>();
			return energySurvival?.energy ?? 100f;
		}

		public override bool subscribe(bool val)
		{
			return true;
		}

		public static void create()
		{
			// changing stats bar backgrounds
			GameObject barsPanel = uGUI.main.barsPanel;											"Energy bar already created!".logDbg(barsPanel.getChild("EnergyBar") != null);

			Image image = barsPanel.getChild("BackgroundQuad").GetComponent<Image>();
			image.sprite = loadSprite("bar_background");
			image.rectTransform.setSize(264, 222);
			image.rectTransform.localPosition = new Vector3(22, 0, 0);

			image = barsPanel.getChild("BackgroundQuad/Quad").GetComponent<Image>();
			image.sprite = loadSprite("bar");
			image.rectTransform.setSize(250, 206);
			image.rectTransform.localPosition = new Vector3(-0.5f, -1f, 0f);

			barsPanel.getChild("BackgroundQuad/Center").GetComponent<Image>().rectTransform.localPosition = new Vector3(16, 38, 0);

			// adding energy circular bar (based on water bar)
			GameObject energyBar = GameObject.Instantiate(barsPanel.getChild("WaterBar"));

			energyBar.name = "EnergyBar";
			energyBar.setParent(barsPanel);
			energyBar.transform.localPosition = new Vector3(115.5f, -41.5f, 0f);
			energyBar.getChild("Icon/Icon").GetComponent<Image>().sprite = loadSprite("eye_icon");

			uGUI_CircularBar crbar = energyBar.GetComponentInChildren<uGUI_CircularBar>();
			crbar.color = new Color(0.5f, 0f, 1f);
			//crbar.borderColor = new Color(0.7f, 0f, 1f);

			energyBar.SetActive(false); // turn off Awake for uGUI_EnergyBar before we copy stuff from waterbar
			uGUI_WaterBar waterbar = energyBar.GetComponentInChildren<uGUI_WaterBar>();
			energyBar.AddComponent<uGUI_EnergyBar>().copyFieldsFrom(waterbar);
			Destroy(waterbar);
		}
	}
}