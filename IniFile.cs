/*!
	Copyright (C) 2005-2007 Kody Brown (kody@bricksoft.com).
	
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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bricksoft.PowerCode
{
    /// <summary>
    /// Utility class for reading from and writing to INI files.
    /// </summary>
    public class IniFile
    {
        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }
        protected string _fileName = "";

        /// <summary>
        /// Gets or sets the un-named section (any text in the file before the first section indicator).
        /// </summary>
        public IniSection UnnamedSection
        {
            get { return _unnamedSection; }
            set { _unnamedSection = value; }
        }
        protected IniSection _unnamedSection = new IniSection("#unnamed");

        /// <summary>
        /// Gets or sets the file sections.
        /// </summary>
        public IniSections Sections
        {
            get { return _sections; }
            set { _sections = value; }
        }
        protected IniSections _sections = new IniSections();

        /// <summary>
        /// Gets or sets whether to include empty lines between sections 
        /// and a space before and after the equal sign in entries.
        /// </summary>
        public bool Expand
        {
            get { return _expand; }
            set { _expand = value; }
        }
        protected bool _expand = true;

        public bool ExpandExtraEntriesWithComments
        {
            get { return _expandExtraEntriesWithComments; }
            set { _expandExtraEntriesWithComments = value; }
        }
        protected bool _expandExtraEntriesWithComments = false;

        /// <summary>
        /// Gets or sets the <seealso cref="System.StringComparison"/> used when sorting Sections.
        /// </summary>
        public StringComparison SectionsStringComparison
        {
            get { return _sectionsStringComparison; }
            set { _sectionsStringComparison = value; }
        }
        public StringComparison _sectionsStringComparison = StringComparison.CurrentCultureIgnoreCase;

        /// <summary>
        /// Gets or sets the <seealso cref="Bricksoft.PowerCode.SortOption"/> used when sorting Sections.
        /// </summary>
        public SortOption SectionsSortOption
        {
            get { return _sectionsSortOption; }
            set { _sectionsSortOption = value; }
        }
        public SortOption _sectionsSortOption = SortOption.Ascending;

        /// <summary>
        /// Gets or sets the <seealso cref="System.StringComparison"/> used when sorting Entries.
        /// </summary>
        public StringComparison EntriesStringComparison
        {
            get { return _entriesStringComparison; }
            set { _entriesStringComparison = value; }
        }
        public StringComparison _entriesStringComparison = StringComparison.CurrentCultureIgnoreCase;

        /// <summary>
        /// Gets or sets the <seealso cref="Bricksoft.PowerCode.SortOption"/> used when sorting Entries.
        /// </summary>
        public SortOption EntriesSortOption
        {
            get { return _entriesSortOption; }
            set { _entriesSortOption = value; }
        }
        public SortOption _entriesSortOption = SortOption.Ascending;

        public bool TreatPlainTextAsComment
        {
            get { return _treatPlainTextAsComment; }
            set { _treatPlainTextAsComment = value; }
        }
        public bool _treatPlainTextAsComment = true;

        public bool NaturalSort
        {
            get { return _naturalSort; }
            set { _naturalSort = value; }
        }
        public bool _naturalSort = true;

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
                //return false;
                throw new InvalidOperationException("FileName is null or empty");
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

            IniSection section = null;
            string l;
            int lineCount;
            char lc;
            StringBuilder comments = new StringBuilder();

            Sections = new IniSections();
            lineCount = 0;

            UnnamedSection = new IniSection("#unnamed");
            section = UnnamedSection;
            bool inheader = true;

            for (int i = 0; i < lines.Count; i++) {
                l = lines[i].Trim();
                lineCount++;

                if (inheader) {
                    if (l.Length == 0) {
                        comments.AppendLine("");
                        continue;
                    }
                    lc = l[0];
                    if (lc == '#') {
                        comments.AppendLine(l);
                        continue;
                    } else {
                        UnnamedSection.Comments += comments.ToString();
                        comments.Clear();
                        inheader = false;
                    }
                }

                if (l.Length == 0) {
                    continue;
                }

                lc = l[0];

                if (lc == ';' || lc == '#') {
                    if (l.StartsWith("; ") || lc == '#') {
                        // Comment
                        comments.AppendLine(l);
                    } else {
                        // Commented-out entry..
                        IniEntry entry = new IniEntry(l, "", IniEntryType.Comment, comments.ToString());
                        comments.Clear();
                        if (section == null) {
                            UnnamedSection.Entries.Add(entry);
                        } else {
                            section.Entries.Add(entry);
                        }
                    }
                } else if (lc == '[') {
                    // handle the un-named items..
                    section = new IniSection(l, comments.ToString());
                    Sections.Add(section);
                    comments.Clear();
                } else if (l.Length > 0) {
                    if (l.IndexOf('=') == -1 && TreatPlainTextAsComment) {
                        // plaintext
                        comments.Append("; **").AppendLine(l);
                    } else {
                        IniEntry entry = new IniEntry(l, comments.ToString(), TreatPlainTextAsComment);
                        comments.Clear();
                        if (section == null) {
                            UnnamedSection.Entries.Add(entry);
                        } else {
                            section.Entries.Add(entry);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Saves the INI file as <seealso cref="FileName"/>.
        /// </summary>
        /// <returns></returns>
        public bool Save() { return Save(FileName); }

        /// <summary>
        /// Saves the INI file specified.
        /// DOES NOT change the FileName property.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool Save( string fileName )
        {
            if (fileName == null || (fileName = fileName.Trim()).Length == 0) {
                return false;
            }

            if (File.Exists(fileName)) {
                File.SetAttributes(fileName, FileAttributes.Normal);
                File.Delete(fileName);
            }

            using (StreamWriter f = File.CreateText(fileName)) {
                // UnnamedSection
                if (UnnamedSection.Comments.Length > 0) {
                    f.WriteLine(UnnamedSection.Comments.Trim());
                }
                if (UnnamedSection.Entries.Count > 0) {
                    foreach (IniEntry entry in UnnamedSection.Entries) {
                        if (entry != null) {
                            f.Write(entry.Export(Expand, ExpandExtraEntriesWithComments));
                        }
                    }
                    f.WriteLine();
                }

                // Sections
                foreach (IniSection section in Sections) {
                    if (section != null) {
                        f.Write(section.Export(Expand, ExpandExtraEntriesWithComments));
                    }
                }

                f.Flush();
                f.Close();
            }
            return true;
        }

        /// <summary>
        /// Sorts the INI file sections.
        /// Does not automatically save.
        /// </summary>
        /// <returns></returns>
        public void Sort()
        {
            if (SectionsSortOption != SortOption.None) {
                if (SectionsSortOption == SortOption.Ascending) {
                    Sections.Sort(delegate( IniSection a, IniSection b )
                    {
                        return string.Compare(a.Name, b.Name, SectionsStringComparison);
                    });
                } else {
                    Sections.Sort(delegate( IniSection a, IniSection b )
                    {
                        return string.Compare(b.Name, a.Name, SectionsStringComparison);
                    });
                }
            }

            if (EntriesSortOption != SortOption.None) {
                if (EntriesSortOption == SortOption.Ascending) {
                    foreach (IniSection section in Sections) {
                        section.Entries.Sort(SortEntriesDelegate);
                    }
                } else {
                    foreach (IniSection section in Sections) {
                        section.Entries.Sort(SortEntriesDelegateRev);
                    }
                }
            }
        }

        private int SortEntriesDelegate( IniEntry a, IniEntry b )
        {
            int i;
            long al, bl;

            if (NaturalSort && long.TryParse(a.Name, out al) && long.TryParse(b.Name, out bl)) {
                i = al.CompareTo(bl);
            } else {
                i = string.Compare(a.Name, b.Name, EntriesStringComparison);
            }
            if (i == 0) {
                if (NaturalSort && long.TryParse(a.Value, out al) && long.TryParse(b.Value, out bl)) {
                    i = al.CompareTo(bl);
                } else {
                    i = string.Compare(a.Value, b.Value, EntriesStringComparison);
                }
            }

            return i;
        }

        private int SortEntriesDelegateRev( IniEntry a, IniEntry b )
        {
            return SortEntriesDelegate(b, a);
        }
    }

    /// <summary>
    /// INI file sort methods.
    /// </summary>
    public enum SortOption
    {
        None = 0,
        /// <summary>
        /// Sort the INI sections alphabetically.
        /// </summary>
        Ascending,
        /// <summary>
        /// Sort the INI sections reverse-alphabetically.
        /// </summary>
        Descending
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
        private string _name = "";

        /// <summary>
        /// Gets or sets the section's comments.
        /// </summary>
        public string Comments { get { return _comments; } set { _comments = (value != null) ? value.Trim() : ""; } }
        private string _comments = "";

        /// <summary>
        /// Gets or sets the sorting method.
        /// </summary>
        public SortOption SortBy { get { return _sortBy; } set { _sortBy = value; } }
        private SortOption _sortBy = 0;

        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        public IniEntries Entries { get { return _entries; } set { _entries = value; } }
        private IniEntries _entries = new IniEntries();

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="comments"></param>
        public IniSection( string name, string comments = "" )
        {
            Name = name;
            Comments = comments;
        }

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
                case SortOption.Ascending:
                    return this.Name.CompareTo(sect.Name);
                default:
                    return sect.Name.CompareTo(this.Name);
            }
        }

        /// <summary>
        /// Returns the section name as it would appear in the file.
        /// </summary>
        /// <returns></returns>
        public new string ToString() { return string.Format("[{0}]", Name); }

        /// <summary>
        /// Returns the section as it should appear in the file.
        /// The <paramref name="Expand"/> flag indicates whether or not to output a space before each Section.
        /// This flag also indicates whether to insert a space before and after the equal sign for name/value pairs.
        /// </summary>
        /// <param name="Expand"></param>
        /// <returns></returns>
        public string Export( bool Expand, bool expandExtraEntriesWithComments = false )
        {
            StringBuilder s = new StringBuilder();

            if (Expand) {
                s.AppendLine();
            }
            if (Comments.Length > 0) {
                s.AppendLine(Comments.Trim());
            }

            s.AppendLine(ToString());

            foreach (IniEntry entry in Entries) {
                if (entry != null) {
                    s.Append(entry.Export(Expand, expandExtraEntriesWithComments));
                }
            }

            s = s.Replace(Environment.NewLine + Environment.NewLine + Environment.NewLine, Environment.NewLine + Environment.NewLine);

            return s.ToString();
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
        /// <summary>
        /// Gets or sets the entry type.
        /// </summary>
        public IniEntryType Type
        {
            get { return _type; }
            set { _type = value; }
        }
        private IniEntryType _type = IniEntryType.PlainText;

        /// <summary>
        /// Gets or sets the entry name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = (value != null) ? value.Trim() : ""; }
        }
        private string _name = "";

        /// <summary>
        /// Gets or sets the entry value.
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = (value != null) ? value.Trim() : ""; }
        }
        private string _value = "";

        /// <summary>
        /// Gets or sets the entry's comments.
        /// </summary>
        public string Comments
        {
            get { return _comments; }
            set { _comments = (value != null) ? value.Trim() : ""; }
        }
        private string _comments = "";

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="comments"></param>
        public IniEntry( string expression, string comments = "", bool treatPlainTextAsComment = false )
        {
            expression = expression.Trim();

            if (comments != null) {
                Comments = comments.Trim();
            } else {
                Comments = "";
            }

            int pos = expression.IndexOf("=");

            if (expression.StartsWith(";") || expression.StartsWith("#")
                    || (pos == -1 && treatPlainTextAsComment)) {
                Type = IniEntryType.Comment;
                Name = expression;
                while (Name.StartsWith(";") || Name.StartsWith("#")) {
                    Name = Name.Substring(1).Trim();
                }
                Value = "";
            } else {
                if (pos == -1) {
                    //throw new Exception("IniEntry expression is not valid");
                    Type = IniEntryType.PlainText;
                    Name = "****ERROR";
                    Value = expression;
                } else {
                    Type = IniEntryType.NameValue;
                    Name = expression.Substring(0, pos).Trim();
                    Value = expression.Substring(pos + 1).Trim();
                }
            }
        }

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="comments"></param>
        public IniEntry( string name, string value = "", IniEntryType type = IniEntryType.NameValue, string comments = "" )
        {
            Type = type;

            Name = name.Trim();
            if (value != null) {
                Value = value.Trim();
            } else {
                Value = "";
            }
            Comments = comments.Trim();

            if (Type == IniEntryType.PlainText) {
                Name = "****ERROR";
                Value = name.Trim();
            } else if (Type == IniEntryType.Comment) {
                Name = name.Trim();
                while (Name.StartsWith(";") || Name.StartsWith("#")) {
                    Name = Name.Substring(1).Trim();
                }
            }
        }

        /// <summary>
        /// Returns the details of this entry.
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

        /// <summary>
        /// Returns the entry as it should appear in the file.
        /// The <paramref name="Expand"/> flag indicates whether or not to output a space before and after the equal sign for name/value pairs.
        /// </summary>
        /// <param name="Expand"></param>
        /// <returns></returns>
        public string Export( bool Expand, bool expandExtraEntriesWithComments = false )
        {
            StringBuilder s = new StringBuilder();

            if (Comments.Length > 0) {
                if (expandExtraEntriesWithComments) {
                    s.AppendLine();
                }
                s.AppendLine(Comments);
            }

            if (Type.Equals(IniEntryType.Comment)) {
                s.AppendLine(";" + Name);
                //} else if (Type.Equals(IniEntryType.PlainText)) {
                //    s.AppendLineFormat("****ERROR{1}={1}{0}", Name, Expand ? " " : "");
            } else {
                string n;
                if (Name.Length == 0) {
                    n = "****NO_KEY";
                } else {
                    n = Name;
                }
                s.AppendLineFormat("{0}{2}={2}{1}", n, Value, Expand ? " " : "");
            }

            if (Comments.Length > 0 && expandExtraEntriesWithComments) {
                s.AppendLine();
            }

            return s.ToString();
        }
    }
}





















