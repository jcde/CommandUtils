using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Globalization;

using AppConfiguration.Reflection;

using NumSite.DAL;
using NumSite.DAL.Commands;
using NumSite.DAL.Sql.Commands;
using NumSite.ORC.Descriptors;
using NumSite.ORC;
using NumSite.DAL.Properties;
using System.Configuration;

namespace NumSite.DBCreateScript
{
	internal class Start
	{
		private static string[] _args;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			_args = args;
			if (args.Length > 0 && args[0].ToLower() == "-help")
			{
				Console.WriteLine(
					@"
Usage	: DBCreateScript [options]
Options :
		-help                   Prints this message
		-dll:<sourceDir>			Directory where dll- and exe- files with assemblies are located
		-t:<targetDir>			Destination directory where in subfolder 'Sql', 'Oracle' etc CreateDBScript.sql will be created
		-a:<assemblyName>		Scripts will be generated for classes within specified assembly.
								Name is in form of substring.
		-b:<baseClassName>		Specifies the name of a base class. Scripts will be generated for classes with this base class.
								Name is in form of substring.
        -c:<classesExpression>  '-c:!UserLog;!SessionLog;!EntityLog;'
		-sp						If this option selected, then standard stored procedures (Select, Update, Insert, Delete, SelectAll)
								will be scripted.
		-r						If this option selected, then INSERT statements for reference data tables are created.
		-noTables					If this option selected, then CREATE TABLE statements will NOT be.
		-pressKey		If this option selected, then program will be exected after a key pressed.

Abstract classes are not scripted.
");
			}
			else
			{
				foreach (var s in NumSite.DBCreateScript.Properties.Settings.Default.DATypes.Split(';'))
				{
					try
					{
						AppConfiguration.ConfigurationWriter.WriteApplicationSetting(
							"NumSite.DAL.Properties.Settings.DefaultDAType", s);
						Settings.Default.Reload();
					}
					catch (Exception) { }
					Generate((DAType)Enum.Parse(typeof(DAType), s));
				}
			}
		}

		private static void Generate(DAType dt)
		{
			if (OptionValue("pressKey") != null)
			{
				Console.Read();
			}

			string assembliesDir = OptionValue("dll");
			if (!Directory.Exists(assembliesDir))
			{
				Console.WriteLine("Source directory does not exist. Current dir used.");
			}
			else
			{
				AssemblyTypesCollection.DefaultAssemblyScanStrategy =
					new AppDomainBaseDirectoryStrategy(assembliesDir);
			}

			string targetDir;
			if (OptionValue("t") != null)
			{
				targetDir = OptionValue("t");
			}
			else
				targetDir = AppDomain.CurrentDomain.BaseDirectory;
			targetDir = Path.Combine(targetDir, dt.ToString());
			if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
				Directory.CreateDirectory(targetDir);

			var targetFileName = Path.Combine(targetDir,
			                                  ((OptionValue("noTables") == null) ? "CreateDBScript.sql" : "DefaultInserts.sql"));

			Console.WriteLine("The result will be at {0}.", targetFileName);

			if (File.Exists(targetFileName))
			{
				Console.WriteLine("Target file name will be overwritten");
			}

			CultureInfo cUI = Thread.CurrentThread.CurrentUICulture;
			CultureInfo myCIclone = (CultureInfo)cUI.Clone();
			myCIclone.NumberFormat.NumberGroupSeparator = "";
			myCIclone.NumberFormat.NumberDecimalSeparator = ".";
			myCIclone.NumberFormat.CurrencyDecimalSeparator = ".";
			Thread.CurrentThread.CurrentUICulture = myCIclone;
			Thread.CurrentThread.CurrentCulture = myCIclone;

			using (StreamWriter Target = File.CreateText(targetFileName))
			{
				IList scripted = new ArrayList();
				IList notScripted = new ArrayList();
				IList scriptedTables = new ArrayList();
				DescriptorCache.Instance = null;
				foreach (TypeDescriptor td in DescriptorCache.Instance)
				{
					if (CanBeScripted(td.RealType, dt))
					{
						notScripted.Add(td);
					}
				}

				int i = 0;
				while (notScripted.Count > 0)
				{
					//if (i >= notScripted.Count)
					//	i = notScripted.Count-1;
					TypeDescriptor td = (TypeDescriptor)notScripted[i];
					bool notScriptedReference = false;
					foreach (ReferenceFieldDescriptor rfd in td.ReferenceFields)
					{
						if (!rfd.NoForeignKey)
						{
							TypeDescriptor refTd = DescriptorCache.Instance[rfd.ReferencedType];
							if (refTd != td // дозволяється посилатися на самого себе
							    && (CanBeScripted(refTd.RealType, dt))
							    && !scripted.Contains(refTd))
							{
								notScriptedReference = true;
								i++;
								if (i >= notScripted.Count)
								{
									Console.WriteLine("Circular references in foreign keys found in {0}", td.RealType.FullName);
									Console.WriteLine(" to {0}", refTd.RealType.FullName);
								}
								break;
							}
						}
					}
					if (!notScriptedReference)
					{
						i = 0;
						notScripted.Remove(td);
						scripted.Add(td);
					}
				}

				if (OptionValue("noTables") == null)
				{
					for (int j = scripted.Count - 1; j >= 0; j--)
					{
						TypeDescriptor td;
						IList tables2Delete = new ArrayList();
						for (int ii = j - 1; ii >= 0; ii--)
						{
							td = (TypeDescriptor)scripted[ii];
							foreach (string tableName in td.TableNames.Keys)
							{
								if (!tables2Delete.Contains(tableName))
									tables2Delete.Add(tableName);
							}
						}

						td = (TypeDescriptor)scripted[j];
						var tables = new List<string>();
						foreach (string tableName in td.TableNames.Keys)
						{
							if (!tables2Delete.Contains(tableName))
							{
								tables.Add(tableName);
							}
						}

						Target.WriteLine(new DeleteEntityCommand(dt).BuildSql(td, tables)[0]);
					}

					foreach (TypeDescriptor td in scripted)
					{
						var tables = new List<string>();
						foreach (string tableName in td.TableNames.Keys)
						{
							if (!scriptedTables.Contains(tableName))
							{
								tables.Add(tableName);
								scriptedTables.Add(tableName);
							}
						}

						Console.WriteLine("Parsing class {0}...", td.RealType.FullName);
						Target.WriteLine(new CreateEntityCommand(dt).BuildSql(td, tables)[0]);
					}
				}

				if (OptionValue("sp") != null)
				{
					Console.WriteLine("Generation standard stored procedures...");
					foreach (TypeDescriptor td in scripted)
					{
						Target.WriteLine(new CreateStandardProceduresCommand(dt).BuildSql(td)[0]);
					}
				}

				if (OptionValue("r") != null)
				{
					Console.WriteLine("Generation reference data...");
					foreach (TypeDescriptor td in scripted)
					{
						try
						{
							Target.WriteLine();
							Target.WriteLine("-- Table " + td.Name);
							IPersistent obj = (IPersistent)Activator.CreateInstance(td.RealType);
							foreach (IPersistent o in obj.DefaultValues())
							{
								var command = new InsertCommand(dt);
								string sql = command.BuildSql(td, SqlGenerator.GetObjectParameters(o), false)[0];
								if (dt == DAType.Oracle)
									sql += "\r\n;\r\n";
								Target.WriteLine(sql);
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine(string.Format("Error for class {0}: {1}", td.Name, ex.Message));
						}
					}
				}
				Target.Flush();
				Console.WriteLine("Creation scripts successfully created.");
			}
		}

		internal static bool CanBeScripted(Type type, DAType dt)
		{
			if (OptionValue("a") != null)
			{
				if (type.Assembly.FullName.IndexOf(OptionValue("a")) == -1)
				{
					return false;
				}
			}
			if (OptionValue("b") != null)
			{
				if (type.BaseType.FullName.IndexOf(OptionValue("b")) == -1)
				{
					return false;
				}
			}
			if (OptionValue("c") != null)
			{
				var exp = OptionValue("c").Split(';');
				foreach (var c in exp)
					if (!string.IsNullOrEmpty(c))
				{
					if (c == "!" + type.Name)
						return false;
				}
			}
			if (type.IsAbstract)
			{
				return false;
			}
			Entity obj = Activator.CreateInstance(type) as Entity;
			if (obj == null || obj.DataAccess != dt)
			{
				return false;
			}
			return true;
		}

		internal static string OptionValue(string option)
		{
			return CommandLineArguments.OptionValue(option, _args);
		}
	}
}