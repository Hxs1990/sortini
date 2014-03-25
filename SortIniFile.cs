/*!
	Copyright (C) 2008-2014 Kody Brown (kody@bricksoft.com).
	
	MIT License:
	
	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to
	deal in the Software without restriction, including without limitation the
	rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
	sell copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:
	
	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.
	
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
	FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
	DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using System.Text;
using Bricksoft.PowerCode;

namespace IniFileSorter
{
    public class sortini
    {
        // options
        static bool quiet = false,
                    verbose = false;

        public static int Main( string[] arguments )
        {
            if (0 == arguments.Length) {
                showUsage();
                return 0;
            }

            IniFile ini;
            string outfile = null;

            ini = new IniFile();

            for (int i = 0; i < arguments.Length; i++) {
                string a = arguments[i];

                if (a[0] == '-' || a[0] == '/' || a[0] == '!') {
                    while (a[0] == '-' || a[0] == '/') {
                        a = a.Substring(1);
                    }
                    string al = a.ToLower();

                    if (al.Equals("?") || al.StartsWith("help")) {
                        showUsage();
                        return 0;
                    } else if (al.StartsWith("q") || al.StartsWith("!q")) {
                        quiet = al.StartsWith("q");
                    } else if (al.StartsWith("v") || al.StartsWith("!v")) {
                        verbose = al.StartsWith("v");

                        //
                        // Output file..
                        //
                    } else if (al.StartsWith("o")) {
                        outfile = a;

                        //
                        // Expand output (adds space between sections and before/after the equal sign)..
                        //
                    } else if (al.StartsWith("expand") || al.StartsWith("!expand")) {
                        ini.Expand = al.StartsWith("expand");

                        //
                        // Case sensitivity..
                        //
                    } else if (al.Equals("i") || al.StartsWith("case-i") || al.StartsWith("casei")) {
                        ini.SectionsStringComparison = StringComparison.CurrentCultureIgnoreCase;
                        ini.EntriesStringComparison = StringComparison.CurrentCultureIgnoreCase;
                    } else if (al.StartsWith("!i") || al.StartsWith("case-s") || al.StartsWith("cases")) {
                        ini.SectionsStringComparison = StringComparison.CurrentCulture;
                        ini.EntriesStringComparison = StringComparison.CurrentCulture;

                    } else if (al.Equals("sections-is")) {
                        ini.SectionsStringComparison = StringComparison.CurrentCultureIgnoreCase;
                    } else if (al.Equals("sections-cs")) {
                        ini.SectionsStringComparison = StringComparison.CurrentCulture;

                    } else if (al.Equals("entries-is")) {
                        ini.EntriesStringComparison = StringComparison.CurrentCultureIgnoreCase;
                    } else if (al.Equals("entries-cs")) {
                        ini.EntriesStringComparison = StringComparison.CurrentCulture;

                        //
                        // Sort direction..
                        //
                    } else if (al.Equals("sort-asc") || al.Equals("sortasc")) {
                        ini.SectionsSortOption = SortOption.Ascending;
                        ini.EntriesSortOption = SortOption.Ascending;
                    } else if (al.Equals("sort-desc") || al.Equals("sortdesc")) {
                        ini.SectionsSortOption = SortOption.Descending;
                        ini.EntriesSortOption = SortOption.Descending;
                    } else if (al.Equals("!sort") || al.Equals("!sort")) {
                        ini.SectionsSortOption = SortOption.None;
                        ini.EntriesSortOption = SortOption.None;

                    } else if (al.Equals("sections") || al.Equals("sections-asc")) {
                        ini.SectionsSortOption = SortOption.Ascending;
                    } else if (al.Equals("sections-desc")) {
                        ini.SectionsSortOption = SortOption.Descending;
                    } else if (al.Equals("!sections")) {
                        ini.SectionsSortOption = SortOption.None;

                    } else if (al.Equals("entries") || al.Equals("entries-asc")) {
                        ini.EntriesSortOption = SortOption.Ascending;
                    } else if (al.Equals("entries-desc")) {
                        ini.EntriesSortOption = SortOption.Descending;
                    } else if (al.Equals("!entries")) {
                        ini.EntriesSortOption = SortOption.None;

                    } else {
                        if (!quiet) {
                            Console.WriteLine("Unknown option: " + a);
                        }
                    }
                } else {
                    if (ini.FileName == null || ini.FileName.Length == 0) {
                        ini.FileName = a;
                        while (ini.FileName.StartsWith("\"") && ini.FileName.EndsWith("\"")) {
                            ini.FileName = ini.FileName.Substring(1, ini.FileName.Length - 2);
                        }
                    } else if (outfile == null || outfile.Length == 0) {
                        outfile = a;
                    } else {
                        if (!quiet) {
                            Console.WriteLine("Unknown option: " + a);
                        }
                    }
                }
            }

            if (StdInEx.IsInputRedirected) {
                // Ignore the specified `file` (if exists)..
                ini.FileName = StdInEx.RedirectInputToFile();
            } else if (ini.FileName == null || (ini.FileName = ini.FileName.Trim()).Length == 0) {
                throw new ArgumentNullException("file");
            } else if (!File.Exists(ini.FileName)) {
                throw new ArgumentException("The specified file was not found (or is inaccessible)", "file");
            }

            try {
                // LOAD & SORT
                if (!ini.Load()) {
                    throw new Exception("Could not load specified file");
                }

                // SORT
                ini.Sort();

                // OUTPUT
                if (!StdInEx.IsOutputRedirected && (outfile == null || outfile.Length == 0)) {
                    // Write back to the input file..
                    if (!ini.Save()) {
                        throw new InvalidOperationException("Failed saving back to file");
                    }
                } else {
                    if (outfile != null && outfile.Length > 0) {
                        if (!ini.Save(outfile)) {
                            throw new InvalidOperationException("Failed saving to outfile");
                        }
                    }
                    if (StdInEx.IsOutputRedirected) {
                        string tmpOut;
                        DeleteFileWhenDone tmpDel;
                        if (outfile != null && outfile.Length > 0) {
                            tmpOut = outfile; // just output the new file..
                        } else {
                            tmpOut = Path.GetTempFileName();
                            tmpDel = new DeleteFileWhenDone(tmpOut);
                            if (!ini.Save(tmpOut)) {
                                throw new InvalidOperationException("Failed saving to temporary output (for stdout)");
                            }
                        }
                        Console.WriteLine(File.ReadAllText(tmpOut));
                    }
                }

            } catch (Exception ex) {
                StringBuilder s = new StringBuilder();
                s.AppendLine("**** ERROR OCURRED")
                 .AppendLine(ex.Message);
                Console.Error.WriteLine(s.ToString());
                if (outfile != null && outfile.Length > 0) {
                    // Write the error to the destination file..
                    File.WriteAllText(outfile, s.ToString());
                }
                return 1;
            }

            if (!quiet) {
                Console.WriteLine("SUCCESS");
            }

            return 0;
        }

        static void showUsage()
        {
            if (!quiet) {
                Console.WriteLine("Sorts the specified .ini file.");
                Console.WriteLine("Created 2009-2014 @wasatchwizard.");
                Console.WriteLine("No warranty expressed or implied. Use at your own risk.");
                Console.WriteLine();
                Console.WriteLine("USAGE: ");
                Console.WriteLine("  sortini filename");
            }
        }
    }
}
