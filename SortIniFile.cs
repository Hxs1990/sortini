using System;
using System.IO;
using Bricksoft.PowerCode;

namespace IniFileSorter
{
   public class sortini
   {
	  string[] args;

	  // options
	  bool quiet = false;

	  public static int Main( string[] args )
	  {
		 return new sortini(args).Run();
	  }

	  public sortini( string[] args )
	  {
		 this.args = args;
	  }

	  public int Run()
	  {
		 if (0 == args.Length) {
			return showUsage(0);
		 }

		 string a, al;
		 string fileName = "";
		 IniFile ini = new IniFile();
		 //string order;

		 for (int argidx = 0; argidx < args.Length; argidx++) {
			a = args[argidx];
			al = a.ToLowerInvariant();

			if (al.Equals("/?") || al.Equals("--help")) {
			   return showUsage(0);

			} else if (al.Equals("/quiet") || al.Equals("--quiet")) {
			   quiet = true;
			} else if (al.Equals("/!quiet") || al.Equals("!quiet") || al.Equals("--!quiet")) {
			   quiet = false;

			} else {
			   fileName = a;
			}
		 }

		 if (fileName.StartsWith("\"") && fileName.EndsWith("\"")) {
			fileName = fileName.Substring(1, fileName.Length - 2);
		 }
		 if (null == fileName || 0 == (fileName = fileName.Trim()).Length) {
			return showUsage(-2);
		 } else if (!File.Exists(fileName)) {
			return showUsage(-3);
		 }


		 if (!ini.Load(fileName)) {
			return showUsage(-5);
		 }

		 //if (cmdLine.Contains("expand")) {
		 //   ini.Expand = cmdLine.attr<bool>("expand");
		 //} else {
		 //   ini.Expand = false;
		 //}
		 ini.Expand = true;

		 //if (cmdLine.Contains("sections")) {
		 //   order = cmdLine["sections"];
		 //   if (!order.Equals("asc", StringComparison.CurrentCultureIgnoreCase)) {
		 //	  ini.Sections.Sort(delegate( IniSection i1, IniSection i2 )
		 //	  {
		 //		 return string.Compare(i2.Name, i1.Name, StringComparison.CurrentCultureIgnoreCase);
		 //	  });
		 //   } else {
		 //	  ini.Sections.Sort(delegate( IniSection i1, IniSection i2 )
		 //	  {
		 //		 return string.Compare(i1.Name, i2.Name, StringComparison.CurrentCultureIgnoreCase);
		 //	  });
		 //   }
		 //}
		 ini.Sections.Sort(delegate( IniSection i1, IniSection i2 )
		 {
			return string.Compare(i2.Name, i1.Name, StringComparison.CurrentCultureIgnoreCase);
		 });

		 //if (cmdLine.Contains("entries")) {
		 //   order = cmdLine["entries"];
		 //   if (!order.Equals("asc", StringComparison.CurrentCultureIgnoreCase)) {
		 //	  foreach (IniSection section in ini.Sections) {
		 //		 section.Entries.Sort(delegate( IniEntry ie1, IniEntry ie2 )
		 //		 {
		 //			return string.Compare(ie2.Name, ie1.Name, StringComparison.CurrentCultureIgnoreCase);
		 //		 });
		 //	  }
		 //   } else {
		 //	  foreach (IniSection section in ini.Sections) {
		 //		 section.Entries.Sort(delegate( IniEntry ie1, IniEntry ie2 )
		 //		 {
		 //			return string.Compare(ie1.Name + ie1.Value, ie2.Name + ie2.Value, StringComparison.CurrentCultureIgnoreCase);
		 //		 });
		 //	  }
		 //   }
		 foreach (IniSection section in ini.Sections) {
			section.Entries.Sort(delegate( IniEntry ie1, IniEntry ie2 )
			{
			   return string.Compare(ie2.Name, ie1.Name, StringComparison.CurrentCultureIgnoreCase);
			});
		 }

		 if (!ini.Save()) {
			return showUsage(-10);
		 }

		 ConsoleColor backupColor = Console.ForegroundColor;
		 Console.ForegroundColor = ConsoleColor.Cyan;

		 Console.WriteLine("Successfully sorted and saved file");

		 Console.ForegroundColor = backupColor;

		 return 0;
	  }

	  public int showUsage( int error )
	  {
		 if (!quiet || error > 0) {
			Console.WriteLine("Sorts the specified .ini file.");
			Console.WriteLine("Created (C) 2009 @wasatchwizard.");
			Console.WriteLine("No warranty expressed or implied. Use at your own risk.");
			Console.WriteLine();
			Console.WriteLine("USAGE: ");
			Console.WriteLine("  sortini filename");
		 }

		 ConsoleColor backupColor = Console.ForegroundColor;
		 Console.ForegroundColor = ConsoleColor.Red;

		 if (error != 0) {
			Console.WriteLine();
			switch (error) {
			   case -1:
				  Console.WriteLine("Invalid arguments");
				  break;
			   case -2:
				  Console.WriteLine("Missing or incorrect format for the filename");
				  break;
			   case -3:
				  Console.WriteLine("The file specified was not found");
				  break;
			   case -5:
				  Console.WriteLine("Could not load the INI file specified");
				  break;
			   case -10:
				  Console.WriteLine("Could not save the sorted file");
				  break;
			}
		 }

		 Console.ForegroundColor = backupColor;

		 return error;
	  }
   }
}
