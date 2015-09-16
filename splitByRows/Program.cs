using System;
using System.IO;
using System.Collections.Generic;
using StringUtils;

namespace splitByRows
{
	class Program
	{
		public static void Main(string[] args)
		{
			if (args==null||args.Length<1)
				Console.WriteLine(string.Format("Usage: {0} fileToSplit -n:ByRowsQuantity",
				                                AppDomain.CurrentDomain.FriendlyName));
			else{
				var byRowsQuantity = int.Parse(CommandLineArguments.OptionValue("n", args) ?? "100");
				int fileIndex = 0;
				int lineIndex = 0;
				var lines = new List<string>();
				foreach (var s in File.ReadAllLines(args[0]))
				{
					lines.Add(s);
					lineIndex++;
					if (lineIndex >= byRowsQuantity)
					{
						File.WriteAllLines(args[0] + "." + fileIndex, lines.ToArray());
						fileIndex++;
						lineIndex = 0;
						lines = new List<string>();
					}
				}
			}
		}
	}
}