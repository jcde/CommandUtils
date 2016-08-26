#if DEBUG
using System;
using System.IO;
using NUnit.Framework;

namespace splitByRows.Tests
{
	[TestFixture]
	public class ArgumentTest
	{
		[Test]
		public void Main()
		{
			Program.Main(new []{@"Properties\AssemblyInfo.cs","-n:5",@"-d:.."});
			Assert.AreEqual("using System.Runtime.InteropServices;",
			                File.ReadAllLines(@"AssemblyInfo.cs.0")[4]);
		}

        [Test]
        public void Split()
        {
            Program.Split(100, "splitted", "Tests\\_paccess.log");
            Assert.AreEqual(2, Directory.GetFiles("Tests\\splitted").Length);
        }
	}
}
#endif
