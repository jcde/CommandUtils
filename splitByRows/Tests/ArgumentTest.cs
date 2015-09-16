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
			Program.Main(new string[]{@"Properties\AssemblyInfo.cs","-n:5"});
			Assert.AreEqual("using System.Runtime.InteropServices;",
			                File.ReadAllLines(@"Properties\AssemblyInfo.cs.0")[4]);
		}
	}
}
#endif
