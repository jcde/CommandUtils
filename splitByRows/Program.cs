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
                var filePath = args[0].Trim('\'').Trim('"');
                var destDir = CommandLineArguments.OptionValue("d", args);
                if (!string.IsNullOrEmpty(destDir))
                {
                    destDir = Path.Combine(Path.GetDirectoryName(filePath), destDir + @"\");
                    if (!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);
                }
                var byRowsQuantity = int.Parse(CommandLineArguments.OptionValue("n", args) ?? "100");
                int fileIndex = 0;
                int lineIndex = 0;
                var lines = new List<string>();
                foreach (var s in File.ReadAllLines(filePath))
                {
                    lines.Add(s);
                    lineIndex++;
                    if (lineIndex >= byRowsQuantity)
                    {
                        File.WriteAllLines(string.Format("{0}{1}.{2}",
                                                         destDir,
                                                         string.IsNullOrEmpty(CommandLineArguments.OptionValue("d", args))
                                                             ? filePath
                                                             : Path.GetFileName(filePath),
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