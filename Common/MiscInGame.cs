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
}