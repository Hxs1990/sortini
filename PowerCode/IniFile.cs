//
// Copyright (C) 2005-2007 Kody Brown (kody@bricksoft.com).
//
// MIT License:
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.IO;
//using System.Windows.Forms;

namespace Bricksoft.PowerCode
{
   /// <summary>
   /// Utility class for reading from and writing to INI files.
   /// </summary>
   public class IniFile
   {
	  protected string _fileName = string.Empty;
	  protected IniSection _unnamedSection = new IniSection("#unnamed");
	  protected IniSections _sections = new IniSections();
	  protected bool _expand = false;

	  /// <summary>
	  /// Gets or sets the file name.
	  /// </summary>
	  public string FileName { get { return _fileName; } set { _fileName = value; } }

	  /// <summary>
	  /// Gets or sets the un-named section (any text in the file before the first section indicator).
	  /// </summary>
	  public IniSection UnnamedSection { get { return _unnamedSection; } set { _unnamedSection = value; } }

	  /// <summary>
	  /// Gets or sets the file sections.
	  /// </summary>
	  public IniSections Sections { get { return _sections; } set { _sections = value; } }

	  /// <summary>
	  /// Gets or sets whether to include empty lines between sections.
	  /// </summary>
	  public bool Expand { get { return _expand; } set { _expand = value; } }

	  /// <summary>
	  /// Creates an instance of the class.
	  /// </summary>
	  public IniFile() { }

	  /// <summary>
	  /// Creates an instance of the class.
	  /// </summary>
	  /// <param name="fileName"></param>
	  public IniFile( string fileName )
	  {
		 FileName = fileName;
	  }

	  /// <summary>
	  /// Loads the INI file from <seealso cref="FileName"/>.
	  /// </summary>
	  /// <returns></returns>
	  public bool Load()
	  {
		 if (FileName == null || (FileName = FileName.Trim()).Length == 0 || !File.Exists(FileName)) {
			return false;
		 }
		 return Load(FileName);
	  }

	  /// <summary>
	  /// Loads the INI file specified.
	  /// </summary>
	  /// <param name="fileName"></param>
	  /// <returns></returns>
	  public bool Load( string fileName )
	  {
		 List<string> lines;

		 if (fileName == null || (fileName = fileName.Trim()).Length == 0 || !File.Exists(fileName)) {
			return false;
		 }

		 FileName = fileName;
		 lines = new List<string>(File.ReadAllLines(FileName));

		 return Load(lines);
	  }

	  /// <summary>
	  /// Loads the INI settings from the <paramref name="lines"/> specified.
	  /// </summary>
	  /// <param name="lines"></param>
	  /// <returns></returns>
	  public bool Load( List<string> lines )
	  {
		 if (lines == null) {
			throw new ArgumentNullException("lines");
		 }

		 UnnamedSection = new IniSection("#unnamed");
		 Sections = new IniSections();
		 IniSection section = UnnamedSection;
		 int lineCount = 0;
		 foreach (string line in lines) {
			lineCount++;
			if (line.StartsWith(";") || line.StartsWith("#")) {
			   section.Entries.Add(new IniEntry(line, IniEntryType.Comment));
			} else if (line.StartsWith("[")) {
			   section = new IniSection(line);
			   Sections.Add(section);
			} else if (line.Trim().Length > 0) {
			   try {
				  section.Entries.Add(new IniEntry(line));
			   } catch (Exception ex) {
				  //MessageBox.Show(string.Format("Error on line #{0}\n\n{1}", lineCount, ex.Message));
				  throw new Exception(string.Format("Error on line #{0}\n\n{1}", lineCount, ex.Message), ex);
			   }
			}
		 }

		 return true;
	  }

	  /// <summary>
	  /// Saves the INI file as <seealso cref="FileName"/>.
	  /// </summary>
	  /// <returns></returns>
	  public bool Save()
	  {
		 return Save(FileName);
	  }

	  /// <summary>
	  /// Saves the INI file specified.
	  /// </summary>
	  /// <param name="fileName"></param>
	  /// <returns></returns>
	  public bool Save( string fileName )
	  {
		 if (null != fileName && 0 < (fileName = fileName.Trim()).Length) {
			if (File.Exists(fileName)) {
			   File.SetAttributes(fileName, FileAttributes.Normal);
			   File.Delete(fileName);
			}

			using (StreamWriter f = File.CreateText(fileName)) {
			   foreach (IniEntry entry in UnnamedSection.Entries) {
				  if (null != entry) {
					 if (entry.Type == IniEntryType.NameValue) {
						f.WriteLine("{0} = {1}", entry.Name.Trim(), entry.Value.Trim());
					 } else if (entry.Type == IniEntryType.Comment) {
						f.WriteLine("{0}", entry.Value.Trim());
					 } else if (entry.Type == IniEntryType.PlainText) {
						f.WriteLine("{0}", entry.Value);
					 }
				  }
			   }

			   foreach (IniSection section in Sections) {
				  if (null != section) {
					 if (Expand) {
						f.WriteLine(string.Empty);
					 }
					 f.WriteLine(section.ToString());

					 foreach (IniEntry entry in section.Entries) {
						if (null != entry) {
						   if (entry.Type == IniEntryType.NameValue) {
							  f.WriteLine("{0} = {1}", entry.Name.Trim(), entry.Value.Trim());
						   } else if (entry.Type == IniEntryType.Comment) {
							  f.WriteLine("{0}", entry.Value.Trim());
						   } else if (entry.Type == IniEntryType.PlainText) {
							  f.WriteLine("{0}", entry.Value);
						   }
						}
					 }
				  }
			   }

			   f.Flush();
			   f.Close();
			}
			return true;
		 } else {
			return false;
		 }
	  }

	  /// <summary>
	  /// Sorts the INI file sections.
	  /// Does not automatically save.
	  /// </summary>
	  /// <returns></returns>
	  public bool Sort()
	  {
		 try {
			Sections.Sort();
			return true;
		 } catch (Exception ex) {
			string m = ex.Message;
			return false;
		 }
	  }

	  /// <summary>
	  /// Adds a section to the INI file.
	  /// Does not automatically save.
	  /// </summary>
	  /// <param name="section"></param>
	  public void AddSection( IniSection section )
	  {
		 _sections.Add(section);
	  }
   }

   /// <summary>
   /// INI file sort methods.
   /// </summary>
   public enum IniSectionSort
   {
	  /// <summary>
	  /// Sort the INI sections alphabetically.
	  /// </summary>
	  Alphabetical,
	  /// <summary>
	  /// Sort the INI sections reverse-alphabetically.
	  /// </summary>
	  ReverseAlphabetical
   }

   /// <summary>
   ///  Simple wrapper for a List&lt;IniSection&gt; objects.
   /// </summary>
   public class IniSections : List<IniSection>
   {

	  public IniSection this[string name]
	  {
		 get
		 {
			foreach (IniSection section in this) {
			   if (section.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) {
				  return section;
			   }
			}
			return null;
		 }
		 set
		 {
			for (int i = this.Count - 1; i >= 0; i--) {
			   if (this[i].Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) {
				  Remove(this[i]);
				  Insert(i, value);
				  return;
			   }
			}
			Add(value);
		 }
	  }

	  /// <summary>
	  /// Determines whether and element is in the collection.
	  /// </summary>
	  /// <param name="name"></param>
	  /// <returns></returns>
	  public bool Contains( string name ) { return Contains(name, StringComparison.CurrentCultureIgnoreCase); }

	  /// <summary>
	  /// Determines whether and element is in the collection.
	  /// </summary>
	  /// <param name="name"></param>
	  /// <param name="StringComparison"></param>
	  /// <returns></returns>
	  public bool Contains( string name, StringComparison StringComparison )
	  {
		 foreach (IniSection section in this) {
			if (section.Name.Equals(name, StringComparison)) {
			   return true;
			}
		 }
		 return false;
	  }
   }

   /// <summary>
   /// INI file section.
   /// </summary>
   public class IniSection : System.IComparable
   {
	  private string _name = string.Empty;
	  private IniSectionSort _sortBy = 0;
	  private IniEntries _entries = new IniEntries();

	  /// <summary>
	  /// Gets or sets the section name.
	  /// </summary>
	  public string Name
	  {
		 get { return _name; }
		 set
		 {
			_name = value;
			if (_name.StartsWith("[")) {
			   _name = _name.Substring(1);
			}
			if (_name.EndsWith("]")) {
			   _name = _name.Substring(0, _name.Length - 1);
			}
		 }
	  }

	  /// <summary>
	  /// Gets or sets the sorting method.
	  /// </summary>
	  public IniSectionSort SortBy { get { return _sortBy; } set { _sortBy = value; } }

	  /// <summary>
	  /// Gets or sets the entries.
	  /// </summary>
	  public IniEntries Entries { get { return _entries; } set { _entries = value; } }

	  /// <summary>
	  /// Creates an instance of the class.
	  /// </summary>
	  /// <param name="name"></param>
	  public IniSection( string name ) { Name = name; }

	  /// <summary>
	  /// Compares the current instance with the specified object.
	  /// </summary>
	  /// <param name="obj"></param>
	  /// <returns></returns>
	  public int CompareTo( object obj )
	  {
		 if (!(obj is IniSection)) {
			throw new InvalidCastException("This object is not of type IniSection");
		 }
		 IniSection sect = (IniSection)obj;
		 switch (SortBy) {
			case IniSectionSort.Alphabetical:
			   return this.Name.CompareTo(sect.Name);
			default:
			   return sect.Name.CompareTo(this.Name);
		 }
	  }

	  /// <summary>
	  /// Returns the section name as it would appear in the file.
	  /// </summary>
	  /// <returns></returns>
	  public new string ToString()
	  {
		 return string.Format("[{0}]", Name);
	  }

	  /// <summary>
	  /// Adds an entry to the collection.
	  /// </summary>
	  /// <param name="entry"></param>
	  public void AddEntry( IniEntry entry )
	  {
		 _entries.Add(entry);
	  }
   }

   /// <summary>
   /// Entry types for INI files.
   /// </summary>
   public enum IniEntryType
   {
	  /// <summary>
	  /// Entry is a comment.
	  /// </summary>
	  Comment,
	  /// <summary>
	  /// Entry is a name/value pair.
	  /// </summary>
	  NameValue,
	  /// <summary>
	  /// Entry is plain text.
	  /// </summary>
	  PlainText
   }

   /// <summary>
   ///  Simple wrapper for a List&lt;IniEntry&gt; objects.
   /// </summary>
   public class IniEntries : List<IniEntry>
   {
	  public IniEntry this[string name]
	  {
		 get
		 {
			foreach (IniEntry section in this) {
			   if (section.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) {
				  return section;
			   }
			}
			return null;
		 }
		 set
		 {
			for (int i = this.Count - 1; i >= 0; i--) {
			   if (this[i].Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) {
				  Remove(this[i]);
				  Insert(i, value);
				  return;
			   }
			}
			Add(value);
		 }
	  }

	  /// <summary>
	  /// Determines whether and element is in the collection.
	  /// </summary>
	  /// <param name="name"></param>
	  /// <returns></returns>
	  public bool Contains( string name ) { return Contains(name, StringComparison.CurrentCultureIgnoreCase); }

	  /// <summary>
	  /// Determines whether and element is in the collection.
	  /// </summary>
	  /// <param name="name"></param>
	  /// <param name="StringComparison"></param>
	  /// <returns></returns>
	  public bool Contains( string name, StringComparison StringComparison )
	  {
		 foreach (IniEntry section in this) {
			if (section.Name.Equals(name, StringComparison)) {
			   return true;
			}
		 }
		 return false;
	  }

   }

   /// <summary>
   /// INI file entry.
   /// </summary>
   public class IniEntry
   {
	  private IniEntryType _type = 0;
	  private string _name = string.Empty;
	  private string _value = string.Empty;

	  /// <summary>
	  /// Gets or sets the entry type.
	  /// </summary>
	  public IniEntryType Type { get { return _type; } set { _type = value; } }

	  /// <summary>
	  /// Gets or sets the entry name.
	  /// </summary>
	  public string Name { get { return _name; } set { _name = value; } }

	  /// <summary>
	  /// Gets or sets the entry value.
	  /// </summary>
	  public string Value { get { return _value; } set { _value = value; } }

	  /// <summary>
	  /// Creates an instance of the class.
	  /// </summary>
	  /// <param name="expression"></param>
	  public IniEntry( string expression )
	  {
		 if (expression.StartsWith(";") || expression.StartsWith("#")) {
			Type = IniEntryType.Comment;
			Name = "#comment";
			Value = expression;
		 } else {
			int pos = expression.IndexOf("=");
			if (pos == -1) {
			   //throw new Exception("IniEntry expression is not valid");
			   Type = IniEntryType.PlainText;
			   Name = "#plaintext";
			   Value = expression;
			} else {
			   Type = IniEntryType.NameValue;
			   Name = expression.Substring(0, pos);
			   Value = expression.Substring(pos + 1);
			}
		 }
	  }

	  /// <summary>
	  /// Creates an instance of the class.
	  /// </summary>
	  /// <param name="name"></param>
	  /// <param name="value"></param>
	  public IniEntry( string name, string value )
	  {
		 Type = IniEntryType.NameValue;
		 Name = name;
		 Value = value;
	  }

	  /// <summary>
	  /// Creates an instance of the class.
	  /// </summary>
	  /// <param name="value"></param>
	  /// <param name="type"></param>
	  public IniEntry( string value, IniEntryType type )
	  {
		 Type = type;
		 Value = value;
		 if (Type == IniEntryType.Comment) {
			Name = "#comment";
		 } else if (Type == IniEntryType.PlainText) {
			Name = "#plaintext";
		 }
	  }

	  /// <summary>
	  /// Returns the entry as it should appear in the file.
	  /// </summary>
	  /// <returns></returns>
	  public override string ToString()
	  {
		 if (Type.Equals(IniEntryType.Comment)) {
			return string.Format(";{0}", Value);
		 } else if (Type.Equals(IniEntryType.PlainText)) {
			return Value;
		 } else {
			return string.Format("{0}={1}", Name, Value);
		 }
	  }
   }
}





















