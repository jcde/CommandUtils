#if DEBUG
using NUnit.Framework;

namespace runas64.Tests
{
	[TestFixture]
	public class ArgumentTest
	{
		public static string Run(string arg)
		{
			return Program.ParseArguments(("" + arg).Split(' '));
		}
		
		[Test]
		public void TestMethod()
		{
			//'E:\ccnet\Projects\AM\team\deploy\AMserver3\Release\Scripts\makevdir.vbs' -d AM -p 'C:\Program Files (x86)\A\A Mobile\v311\Scripts'
			Assert.AreEqual(@" """"E:\ccnet\Projects\AM\team\deploy\AM server3\Release\Scripts\makevdir.vbs"""""
			                + " -d AM -p "
			                + @"""""C:\Program Files (x86)\A\A Mobile\v311\Scripts""""",
			                Program.ParseArguments(new string[]{null,
			                                       	@"""E:\ccnet\Projects\AM\team\deploy\AM server3\Release\Scripts\makevdir.vbs""",
			                                       	"-d","AM","-p",
			                                       	@"""C:\Program Files (x86)\A\A Mobile\v311\Scripts"""}));
			//'makevdir.vbs' -d A -p '\wwwroo t'
			Assert.AreEqual(" -d A -p %27\\wwwroo t%27", Run(@"%27makevdir.vbs%2527 -d A -p %27\wwwroo t%27"));
		}
	}
}
#endif
