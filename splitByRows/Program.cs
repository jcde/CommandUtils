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
            if (args == null || args.Length < 1)
                Console.WriteLine(string.Format(@"Usage: {0} fileToSplit -n:ByRowsQuantity d:subfolder",
                                                AppDomain.CurrentDomain.FriendlyName));
            else
            {
                var destDir = CommandLineArguments.OptionValue("d", args);
                if (!string.IsNullOrEmpty(destDir))
                {
                    destDir = Path.Combine(Environment.CurrentDirectory, destDir + @"\");
                    if (!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);
                }
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
                        File.WriteAllLines(string.Format("{0}{1}.{2}",
                                                         destDir,
                                                         string.IsNullOrEmpty(CommandLineArguments.OptionValue("d", args))
                                                             ? args[0]
                                                             : Path.GetFileName(args[0]),
                                                         fileIndex),
                                           lines.ToArray());
                        fileIndex++;
                        lineIndex = 0;
                        lines = new List<string>();
                    }
                }
            }
        }
    }
}