using System;
using System.Diagnostics;

namespace runas64
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				var p = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = args[0].Trim(new[] {'\'','"' }),
						Arguments = ParseArguments(args),
						UseShellExecute = true,
						//do not help to hide black screen CreateNoWindow = true, and //WindowStyle = ProcessWindowStyle.Hidden,
						WorkingDirectory = Environment.CurrentDirectory,
					}
				};
				try {
					p.Start();
					p.WaitForExit();
				}
				catch (Exception)
				{
					//log
				}
			}
		}

		public static string ParseArguments(string[] args)
		{
			var arguments = "";
			for (int i = 1; i < args.Length; i++)
			{
				var a = args[i];
				if (args[i].Contains(" "))
					a = "\"" + args[i] + "\"";
				else if (args[i].StartsWith("'"))
					a = "\"" + args[i].Substring(1);
				else if (args[i].EndsWith("'"))
					a = args[i].Substring(0, args[i].Length - 1) + "\"";

				arguments += " " + a;
			}
			return arguments;
		}
	}
}