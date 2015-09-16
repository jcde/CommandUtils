using System;
using System.Collections.Generic;

namespace StringUtils
{
	public class CommandLineArguments
	{
		/// <param name="option">without symbol '-', can be with symbol ':'</param>
		/// <returns>if option absent, then returns null</returns>
		public static string OptionValue(string option, string[] args)
		{
			foreach (string s in args)
			{
				int i = s.IndexOf(':');
				string par;
				if (i == -1)
				{
					par = s;
				}
				else
				{
					par = s.Substring(0, i);
				}
				if (par == "-" + option)
				{
					if (i == -1)
					{
						return "";
					}
					else
					{
						return s.Substring(i + 1).Trim(new[] { '\'', '"' });
					}
				}
			}
			return null;
		}		
	}
}