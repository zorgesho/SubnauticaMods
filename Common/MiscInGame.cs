namespace Common
{
	static class MiscGameExtensions
	{
		static public string onScreen(this string s)
		{
			ErrorMessage.AddDebug(s);
			return s;
		}
	}

	static class Strings
	{
		static public class Mouse
		{
			static public readonly string middleButton	= "<color=#ADF8FFFF>" + char.ConvertFromUtf32(57405) + "</color>";
			static public readonly string scrollUp		= "<color=#ADF8FFFF>" + char.ConvertFromUtf32(57406) + "</color>";
			static public readonly string scrollDown	= "<color=#ADF8FFFF>" + char.ConvertFromUtf32(57407) + "</color>";
		}
		
		static public readonly string modName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
	}
}