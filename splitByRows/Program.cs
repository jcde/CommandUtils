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

                Split(
                    int.Parse(CommandLineArguments.OptionValue("n", args) ?? "100"), 
                    destDir, 
                    filePath);
            }
        }

        internal static void Split(int byRowsQuantity, string destDir, string filePath)
        {
            if (!string.IsNullOrEmpty(destDir))
            {
                destDir = Path.Combine(Path.GetDirectoryName(filePath), destDir + @"\");
                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);
            }

            int fileIndex = 0;
            int lineIndex = 0;
            var lines = new List<string>();
            string path = null;
            foreach (var s in File.ReadAllLines(filePath))
            {
                lines.Add(s);
                lineIndex++;
                path = string.Format("{0}{1}.{2}",
                                     destDir,
                                     string.IsNullOrEmpty(destDir) ? filePath : Path.GetFileName(filePath),
                                     fileIndex);
                if (lineIndex >= byRowsQuantity)
                {
                    File.WriteAllLines(path, lines.ToArray());
                    fileIndex++;
                    lineIndex = 0;
                    lines.Clear();
                }
            }

            if (path != null)
                File.WriteAllLines(path, lines.ToArray());
        }
    }
}