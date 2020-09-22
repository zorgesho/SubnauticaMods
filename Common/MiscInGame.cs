using System.Text;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Common
{
	static partial class StringExtensions
	{
		public static string onScreen(this string s)
		{
			if (!GameUtils.isLoadingState && Time.timeScale != 0f)
				ErrorMessage.AddDebug(s);

			return s;
		}

		// messages with the same prefix will stay in the same message slot
		public static string onScreen(this string s, string prefix) { Debug.addMessage(s, prefix); return s; }

		public static void onScreen(this List<string> list, string msg = "", int maxCount = 30)
		{
			var listToPrint = list.Count > maxCount? list.GetRange(0, maxCount): list;
			listToPrint.ForEach(s => ErrorMessage.AddDebug(msg + s));
		}

		public static string onScreen(this List<string> list, string prefix)
		{
			var sb = new StringBuilder();
			list.ForEach(line => sb.AppendLine(line));
			return sb.ToString().onScreen(prefix);
		}
	}

	static class Strings
	{
		public static class Mouse
		{
			static string _str(int utf32) => $"<color=#ADF8FFFF>{char.ConvertFromUtf32(utf32)}</color>";

			public static readonly string rightButton	= _str(57404);
			public static readonly string middleButton	= _str(57405);
			public static readonly string scrollUp		= _str(57406);
			public static readonly string scrollDown	= _str(57407);
		}
	}

	static partial class SpriteHelper // extended in other Common projects
	{
		public static Atlas.Sprite getSprite(object spriteID)
		{
			$"TechSpriteHelper.getSprite({spriteID.GetType()}) is not implemented!".logError();
			return SpriteManager.defaultSprite;
		}
	}

	static class GameUtils
	{
		// can't use vanilla GetVehicle in OnPlayerModeChange after 06.11 update :(
		public static Vehicle getVehicle(this Player player) => player?.GetComponentInParent<Vehicle>();

		public static TechType getHeldToolType() => Inventory.main?.GetHeldTool()?.pickupable?.GetTechType() ?? TechType.None;

		public static bool isLoadingState => uGUI._main?.loading.loadingBackground.state == true;

		public static void clearScreenMessages() => // expire all messages except QMM main menu messages
			ErrorMessage.main?.messages.Where(m => m.timeEnd - Time.time < 1e3f).forEach(m => m.timeEnd = Time.time - 1f);
	}
}