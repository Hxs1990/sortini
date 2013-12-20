//
// Copyright (C) 2003-2013 Kody Brown (kody@bricksoft.com).
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

#define BRICKSOFT_CMDLINE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;

namespace Bricksoft.PowerCode
{
	/// <summary>
	/// Provides a simple way to get the command-line arguments.
	/// <remarks>
	/// See CommandLineArguments.cs.txt for details on how to use this class.
	/// </remarks>
	/// </summary>
	public class CommandLine
	{
		private OrderedDictionary data = new OrderedDictionary();


		/// <summary>
		/// Represents an un-named command-line argument.
		/// Unnamed items in the collection have their index appended, matching the order entered on the command-line.
		/// Unnamed items are one-based.
		/// <remarks>These arguments do not begin with a - nor /, and do not have a named item preceding them.</remarks>
		/// </summary>
		public const string UnnamedItem = "UnnamedItem";

		public StringComparison stringComparison { get; set; }


		private object Caller { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string[] OriginalCmdLine { get { return _originalCmdLine; } }
		private string[] _originalCmdLine;

		/// <summary>
		/// Gets the first un-named item if it exists, otherwise 
		/// returns an empty string.
		/// </summary>
		public string UnnamedItem1 { get { return attr<string>("UnnamedItem1"); } }

		/// <summary>
		/// 
		/// </summary>
		public List<string> UnnamedItems
		{
			get
			{
				List<string> l;
				IDictionaryEnumerator enumerator;

				l = new List<string>();
				enumerator = data.GetEnumerator();

				while (enumerator.MoveNext()) {
					if (enumerator.Key.ToString().StartsWith(UnnamedItem, StringComparison.InvariantCulture)) {
						l.Add(enumerator.Value as string);
					}
				}

				return l;
			}
		}

		// **** Indexer ---------------------------------------------------------------------------

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Index"></param>
		/// <returns></returns>
		public object this[int index]
		{
			get { return data[index]; }
			set { data[index] = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Key"></param>
		/// <returns></returns>
		public object this[string key]
		{
			get
			{
				int index;

				index = getIndexOfArgument(key);
				if (index == -1) {
					throw new ArgumentException("key");
				}

				return data[index];
			}
			set
			{
				int index;

				index = getIndexOfArgument(key);
				if (index == -1) {
					throw new ArgumentException("key");
				}

				data[index] = value;
			}
		}

		//public object this[string key, StringComparison stringComparison]
		//{
		//	get
		//	{
		//		int index;

		//		index = getIndexOfArgument(key, stringComparison);
		//		if (index == -1) {
		//			throw new ArgumentException("key");
		//		}

		//		return data[index];
		//	}
		//	set
		//	{
		//		int index;

		//		index = getIndexOfArgument(key, stringComparison);
		//		if (index == -1) {
		//			throw new ArgumentException("key");
		//		}

		//		data[index] = value;
		//	}
		//}

		// **** Constructor(s) --------------------------------------------------

		/// <summary>
		/// Creates a new instance of the class.
		/// </summary>
		/// <param name="arguments"></param>
		public CommandLine( string[] arguments )
		{
			stringComparison = Path.DirectorySeparatorChar == '\\' ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;

			_originalCmdLine = arguments;
			parseCommandLine(arguments);
		}

		// **** Parsers ---------------------------------------------------------------------------

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arguments"></param>
		private void parseCommandLine( string[] arguments )
		{
			char[] anyOf = new char[] { '=', ':' };
			int pos = -1;

			#region Supported arguments:

			// the / indicates a single parameter
			// the - and -- indicate a parameter with a trailing value and are interchangeable..
			// 
			// /name                    ( name = true )
			// /"name"                  ( name = true )
			// /"name one"              ( name one = true )
			// "/name one"              ( name = true )
			// 
			// /name=value              ( name=value = true )
			// /name:value              ( name = value )
			// /name="value here"       ( name = value here )
			// 
			// -name value              ( name = value )
			// -name "-value"           ( name = -value )
			// -name -value             ( name = -value )
			// -"name 4" "value"        ( name 4 = value )
			// "-name 4" "value"        ( name 4 = value )
			// "-name 4" "value one"    ( name 4 = value one )
			// 
			// -name1 -name2            ( name1 = name2 )
			// 
			// -name=value              ( name = value )
			// -"name"=value            ( name = value )
			// -name="value"            ( name = value )
			// -"name"="value"          ( name = value )
			// -"name=value"            ( name = value )
			// "-name=value"            ( name = value )
			// 
			// -name="value one"        ( name = value one )
			// -"name=value one"        ( name = value one )
			// "-name=value one"        ( name = value one )
			// 
			// 
			// 
			// /name "value"            ( name = true ) and ( value = true )  <-- notice the /
			// -name "value"            ( name = value )  <-- notice the -
			// 
			// 
			// -"name 1"                
			// 

			#endregion

			string arg;
			string name;
			string value;
			bool needsValue;
			int unnamedItemCount;

			name = string.Empty;
			value = string.Empty;
			needsValue = false;
			unnamedItemCount = 0;

			if (arguments == null || arguments.Length == 0) {
				return;
			}

			for (int i = 0; i < arguments.Length; i++) {
				arg = arguments[i];

				if (needsValue && name != null && name.Length > 0) {

					// Get the value for a NameValueArg argument.
					value = arg.Trim();
					while (value.StartsWith("\"") && value.EndsWith("\"")) {
						value = value.Substring(1, value.Length - 2);
					}

					add(name, value);
					needsValue = false;

				} else if (arg.StartsWith("-")) {

					// NameValueOptional | NameValueRequired
					name = arg.Trim();
					while (name.StartsWith("-") || (name.StartsWith("\"") && name.EndsWith("\""))) {
						name = name.TrimStart('-');
						if (name.StartsWith("\"") && name.EndsWith("\"")) {
							name = name.Substring(1, name.Length - 2);
						}
					}

					pos = name.IndexOfAny(anyOf);
					if (pos > -1) {
						value = name.Substring(pos + 1);
						if (value.StartsWith("\"") && value.EndsWith("\"")) {
							value = value.Substring(1, value.Length - 2);
						}
						name = name.Substring(0, pos);
						add(name, value);
						needsValue = false;
					} else {
						needsValue = true;
					}

				} else if (arg.StartsWith("/")) {

					// NameOnly
					name = arg.Trim();
					while (name.StartsWith("/") || (name.StartsWith("\"") && name.EndsWith("\""))) {
						name = name.TrimStart('/');
						if (name.StartsWith("\"") && name.EndsWith("\"")) {
							name = name.Substring(1, name.Length - 2);
						}
					}

					pos = name.IndexOfAny(anyOf);
					if (pos > -1) {
						value = name.Substring(pos + 1);
						if (value.StartsWith("\"") && value.EndsWith("\"")) {
							value = value.Substring(1, value.Length - 2);
						}
						name = name.Substring(0, pos - 1);
						add(name, value);
					} else {
						add(name, null);
					}
					needsValue = false;

				} else {

					// UnnamedItem
					add(UnnamedItem + (++unnamedItemCount), arg);

				}

			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public static CommandLine parse( string[] arguments ) { return new CommandLine(arguments); }

		// **** add() ---------------------------------------------------------------------------

		/// <summary>
		/// Adds an argument with the specified <paramref name="key"/> and <paramref name="value"/> 
		/// into the collection with the lowest available index.
		/// <remarks>If an existing item exists in the collection, it will be overwritten.</remarks>
		/// </summary>
		/// <param name="key">The key of the entry to add.</param>
		/// <param name="value">The value of the entry to add. This value can be null.</param>
		public void add( string key, object value )
		{
			data.Add(key, value);
		}

		//// **** remove() ---------------------------------------------------------------------------

		///// <summary>
		///// Removes the entry with the specified key from the System.Collections.Specialized.OrderedDictionary collection.
		///// </summary>
		///// <param name="key">The key of the entry to remove.</param>
		///// <exception cref="System.NotSupportedException">The System.Collections.Specialized.OrderedDictionary collection is read-only.</exception>
		///// <exception cref="System.ArgumentNullException">key is null.</exception>
		//public void remove( string key ) { remove(key); }

		//public void remove( string key, StringComparison stringComparison ) { remove(key, stringComparison ); }

		///// <summary>
		///// Removes the entry with the specified key from the System.Collections.Specialized.OrderedDictionary collection.
		///// </summary>
		///// <param name="keys">The keys of the entries to remove.</param>
		///// <exception cref="System.NotSupportedException">The System.Collections.Specialized.OrderedDictionary collection is read-only.</exception>
		///// <exception cref="System.ArgumentNullException">key is null.</exception>
		//public void remove( params string[] keys )
		//{
		//	foreach (string p in keys) {
		//		data.Remove(p);
		//	}
		//}

		// **** ToArray() ---------------------------------------------------------------------------

		/// <summary>
		/// Output all arguments as it would be entered on the command-line.
		/// </summary>
		/// <returns></returns>
		public string[] toArray()
		{
			List<string> result;
			IDictionaryEnumerator enumerator;
			string name;
			string value;
			int pos;

			result = new List<string>();
			enumerator = data.GetEnumerator();

			while (enumerator.MoveNext()) {
				name = (string)enumerator.Key;
				value = enumerator.Value as string;

				if (name.StartsWith(UnnamedItem, StringComparison.InvariantCulture)) {

					// UnnamedItem
					pos = value.IndexOf(' ');
					if (pos > -1) {
						result.Add("\"" + name + "\"");
					} else {
						result.Add(name);
					}

				} else if (value == null) {

					// StandAloneArg (/arg)
					pos = name.IndexOf(' ');
					if (pos > -1) {
						result.Add("/\"" + name + "\"");
					} else {
						result.Add("/" + name);
					}

				} else {

					// NameValueArg (-name value)
					pos = name.IndexOf(' ');
					if (pos > -1) {
						result.Add("-\"" + name + "\"");
					} else {
						result.Add("-" + name);
					}

					pos = value.IndexOf(' ');
					if (pos > -1) {
						result.Add("\"" + name + "\"");
					} else {
						result.Add(name);
					}

				}
			}

			return result.ToArray();
		}

		// **** ToString() ---------------------------------------------------------------------------

		/// <summary>
		/// Output all arguments as it would be entered on the command-line.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder result;
			IDictionaryEnumerator enumerator;
			string name;
			string value;
			int pos;

			result = new StringBuilder();
			enumerator = data.GetEnumerator();

			while (enumerator.MoveNext()) {
				name = (string)enumerator.Key;
				value = enumerator.Value as string;

				if (result.Length > 0) {
					result.Append(' ');
				}

				if (name.StartsWith(UnnamedItem, StringComparison.InvariantCulture)) {

					// UnnamedItem
					pos = value.IndexOf(' ');
					if (pos > -1) {
						result.Append('"').Append(value).Append('"');
					} else {
						result.Append(value);
					}

				} else if (value == null) {

					// StandAloneArg (/arg)
					result.Append('/');
					pos = name.IndexOf(' ');
					if (pos > -1) {
						result.Append('"').Append(name).Append('"');
					} else {
						result.Append(name);
					}

				} else {

					// NameValueArg (-name value)
					result.Append('-');
					pos = name.IndexOf(' ');
					if (pos > -1) {
						result.Append('"').Append(name).Append('"');
					} else {
						result.Append(name);
					}
					result.Append(' ');
					pos = value.IndexOf(' ');
					if (pos > -1) {
						result.Append('"').Append(value).Append('"');
					} else {
						result.Append(value);
					}

				}
			}

			return result.ToString();
		}

		// **** contains() ---------------------------------------------------------------------------

		/// <summary>
		/// Returns whether any of the items in <paramref name="keys"/> exists on the command-line.
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		public bool contains( params string[] keys )
		{
			IDictionaryEnumerator enumerator;
			string Key;

			if (keys == null || keys.Length == 0) {
				throw new ArgumentNullException("keys");
			}

			enumerator = data.GetEnumerator();

			while (enumerator.MoveNext()) {
				Key = (string)enumerator.Key;
				foreach (string k in keys) {
					if (Key.Equals(k, stringComparison)) {
						return true;
					}
				}
			}

			return false;
		}

		// **** containsAllOf() ---------------------------------------------------------------------------

		/// <summary>
		/// Returns whether all items in <paramref name="keys"/> exist on the command-line.
		/// </summary>
		/// <param name="keys">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public bool containsAllOf( params string[] keys )
		{
			if (keys == null) {
				throw new ArgumentNullException("keys");
			}

			foreach (string arg in keys) {
				if (arg == null) {
					throw new ArgumentNullException("keys", "An element in keys is null");
				}
				if (!contains(arg)) {
					return false;
				}
			}

			return true;
		}

		// **** isUnnamedItem() --------------------------------------------------

		/// <summary>
		/// Returns whether <paramref name="index"/> is an un-named argument.
		/// </summary>
		/// <param name="index">The index of the argument to check whether it is an un-named argument.</param>
		/// <returns></returns>
		public bool isUnnamedItem( int index )
		{
			int i;

			if (data.Keys.Count <= index) {
				return false;
				//throw new IndexOutOfRangeException("index cannot exceed collection count.");
			}

			i = 0;

			foreach (string key in data.Keys) {
				if (i++ == index) {
					if (key.StartsWith(UnnamedItem, StringComparison.InvariantCulture)) {
						return true;
					} else {
						return false;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Returns the numeric index of the <paramref name="key"/> specified.
		/// Performs comparison ignoring the case.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public int getIndexOfArgument( string key )
		{
			IDictionaryEnumerator enumerator;
			string name;
			int index;
			bool foundIt;

			enumerator = data.GetEnumerator();
			index = 0;
			foundIt = false;

			while (enumerator.MoveNext()) {
				name = (string)enumerator.Key;
				//value = enumerator.Value as string;
				if (name.Equals(key, stringComparison)) {
					foundIt = true;
					break;
				}
				index++;
			}

			if (foundIt) {
				return index;
			} else {
				return -1;
			}
		}

		// **** getRemainingString() --------------------------------------------------

		/// <summary>
		/// Returns the value of the first command-line argument found in <paramref name="keys"/> as a string
		/// INCLUDING everything on the command-line that followed, otherwise returns <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public string getRemainingString( string key, string defaultValue )
		{
			if (hasValue(key)) {
				StringBuilder val = new StringBuilder();
				val.Append((string)this[key]).Append(' ');
				for (int i = getIndexOfArgument(key) + 1; i < data.Count; i++) {
					val.Append((string)this[i]).Append(' ');
				}
				return val.ToString().Trim();
			}
			return defaultValue;
		}

		/// <summary>
		/// Returns the value of the command-line argument as a string, found at position <paramref name="index"/>,
		/// INCLUDING everything on the command-line that followed, otherwise returns an empty string.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public string getRemainingString( int index ) { return getRemainingString(index, string.Empty); }

		/// <summary>
		/// Returns the value of the command-line argument as a string, found at position <paramref name="index"/>,
		/// INCLUDING everything on the command-line that followed, otherwise returns <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public string getRemainingString( int index, string defaultValue )
		{
			StringBuilder val;

			if (this[index] != null) {
				val = new StringBuilder();
				val.Append((string)this[index]).Append(' ');
				for (int i = index + 1; i < data.Count; i++) {
					val.Append((string)this[i]).Append(' ');
				}
				return val.ToString().Trim();
			}

			return defaultValue;
		}

		// **** getEverythingAfter() --------------------------------------------------

		/// <summary>
		/// Finds the first command-line argument found in <paramref name="key"/> and returns
		/// everything AFTER it on the command-line that followed, otherwise returns <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public string getEverythingAfter( string key, string defaultValue )
		{
			if (contains(key) && this[key] != null) {
				StringBuilder val = new StringBuilder();
				//val.Append((string)this[key]).Append(' ');
				for (int i = getIndexOfArgument(key) + 1; i < data.Count; i++) {
					val.Append((string)this[i]).Append(' ');
				}
				return val.ToString().Trim();
			}
			return defaultValue;
		}

		///// <summary>
		///// Finds the first command-line argument found in <paramref name="keys"/> and returns
		///// everything AFTER it on the command-line that followed, otherwise returns <paramref name="defaultValue"/>.
		///// </summary>
		///// <param name="defaultValue"></param>
		///// <param name="keys"></param>
		///// <returns></returns>
		//public string[] getEverythingAfter( string[] defaultValue, params string[] arguments )
		//{
		//	List<string> result = new List<string>();

		//	foreach (string argument in arguments) {
		//		if (contains(argument) && this[argument] != null) {
		//			StringBuilder val = new StringBuilder();
		//			//val.Append((string)this[key]).Append(' ');
		//			for (int i = getIndexOfArgument(argument) + 1; i < this.Count; i++) {
		//				result.Add(this.GetString(i));
		//			}
		//			return result.ToArray();
		//		}
		//	}

		//	return defaultValue;
		//}

		// **** exists() --------------------------------------------------

		public bool exists( string key )
		{
			if (key == null || key.Length == 0) {
				throw new ArgumentNullException("key");
			}

			if (contains(key)) {
				return true;
			}

			return false;
		}

		// **** hasValue() --------------------------------------------------

		/// <summary>
		/// Returns whether the argument(s) contain a non-null and non-empty value.
		/// If no arguments are found it returns false.
		/// Performs comparison ignoring case.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool hasValue( string key )
		{
			if (key == null || key.Length == 0) {
				throw new ArgumentNullException("keys");
			}

			if (contains(key) && attr<string>(key).Length > 0) {
				return true;
			}

			return false;
		}

		// **** readValuesFromFile() --------------------------------------------------

		/// <summary>
		/// Returns a collection of values loaded from the specified file.
		/// One item in collection for each line in file.
		/// Lines starting with a semi-colon (;) are ignored.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		static public bool readValuesFromFile( string fileName, out List<string> values )
		{
			if (fileName == null || fileName.Length == 0) {
				throw new ArgumentNullException("fileName");
			}
			if (!File.Exists(fileName)) {
				values = null;
				return false;
			}

			values = new List<string>();

			try {
				foreach (string line in File.ReadAllLines(fileName)) {
					// Ignore comments in file, just in case!
					if (line.StartsWith(";")) {
						continue;
					}
					values.Add(line);
				}
			} catch (Exception ex) {
				//values = string.Empty;
				values.Clear();
				values.Add(ex.Message);
				return false;
			}

			return true;
		}

		// **** attr<T>() --------------------------------------------------

		public T attr<T>( params string[] keys )
		{
			if (keys == null || keys.Length == 0) {
				throw new InvalidOperationException("keys is required");
			}

			foreach (string key in keys) {
				if (contains(key)) {
					if (typeof(T) == typeof(bool) || typeof(T).IsSubclassOf(typeof(bool))) {
						if ((object)this[key] != null) {
							return (T)(object)(this[key].ToString().StartsWith("t", StringComparison.CurrentCultureIgnoreCase));
						} else {
							return (T)(object)true;
						}
					} else if (typeof(T) == typeof(DateTime) || typeof(T).IsSubclassOf(typeof(DateTime))) {
						DateTime dt;
						if ((object)this[key] != null && DateTime.TryParse(this[key].ToString(), out dt)) {
							return (T)(object)dt;
						}
					} else if (typeof(T) == typeof(short) || typeof(T).IsSubclassOf(typeof(short))) {
						short i;
						if ((object)this[key] != null && short.TryParse(this[key].ToString(), out i)) {
							return (T)(object)i;
						}
					} else if (typeof(T) == typeof(int) || typeof(T).IsSubclassOf(typeof(int))) {
						int i;
						if ((object)this[key] != null && int.TryParse(this[key].ToString(), out i)) {
							return (T)(object)i;
						}
					} else if (typeof(T) == typeof(long) || typeof(T).IsSubclassOf(typeof(long))) {
						long i;
						if ((object)this[key] != null && long.TryParse(this[key].ToString(), out i)) {
							return (T)(object)i;
						}
					} else if (typeof(T) == typeof(ulong) || typeof(T).IsSubclassOf(typeof(ulong))) {
						ulong i;
						if ((object)this[key] != null && ulong.TryParse(this[key].ToString(), out i)) {
							return (T)(object)i;
						}
					} else if (typeof(T) == typeof(string) || typeof(T).IsSubclassOf(typeof(string))) {
						// string
						if ((object)this[key] != null) {
							return (T)(object)(this[key]).ToString();
						}
					} else if (typeof(T) == typeof(string[]) || typeof(T).IsSubclassOf(typeof(string[]))) {
						// string[]
						if ((object)this[key] != null) {
							// string array data is ALWAYS saved to the file as a string[] (even List<string>)..
							return (T)(object)this[key];
						}
					} else if (typeof(T) == typeof(List<string>) || typeof(T).IsSubclassOf(typeof(List<string>))) {
						// List<string>
						if ((object)this[key] != null) {
							// string array data is ALWAYS saved to the file as a string[] (even List<string>)..
							return (T)(object)new List<string>((string[])this[key]);
						}
					} else {
						throw new InvalidOperationException("unknown or unsupported data type was requested");
					}
				}
			}

			return default(T);
		}

	}

	public class CommandLineArg
	{
		/// <summary>
		/// 
		/// </summary>
		public const int DEFAULT_INDEX = (int.MaxValue / 2);

		/// <summary>
		/// 
		/// </summary>
		public const int DEFAULT_GROUP = (int.MaxValue / 2);

		/// <summary>
		/// 
		/// </summary>
		public int SortIndex { get { return _sortIndex; } set { _sortIndex = value; } }
		private int _sortIndex = DEFAULT_INDEX;

		/// <summary>
		/// 
		/// </summary>
		public int Group { get { return _group; } set { _group = value; } }
		private int _group = DEFAULT_GROUP;

		/// <summary>
		/// 
		/// </summary>
		public string Name { get { return _name ?? (_name = string.Empty); } set { _name = value ?? string.Empty; } }
		private string _name = string.Empty;

		/// <summary>
		/// Gets or sets the summary description displayed in the usage.
		/// </summary>
		public string Description { get { return _description ?? (_description = string.Empty); } set { _description = value ?? string.Empty; } }
		private string _description = string.Empty;

		/// <summary>
		/// Gets or sets the additional help content that is displayed when you call '--help cmd', where cmd is the current CommandLineArg.
		/// </summary>
		public string HelpContent { get { return _helpContent ?? (_helpContent = string.Empty); } set { _helpContent = value ?? string.Empty; } }
		private string _helpContent = string.Empty;

		/// <summary>
		/// Gets or sets the error text when the command-line argument was not provided and was set to <seealso cref="Required"/>.
		/// </summary>
		/// <remarks>
		/// If not specified the <seealso name="AppDescription"/> is used in the error message.
		/// </remarks>
		public string MissingText { get { return _missingText ?? (_missingText = string.Empty); } set { _missingText = value ?? string.Empty; } }
		private string _missingText = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public bool Required { get { return _required; } set { _required = value; } }
		private bool _required = false;

		/// <summary>
		/// 
		/// </summary>
		public string[] Keys { get { return _keys ?? (_keys = new string[] { }); } set { _keys = value ?? new string[] { }; } }
		private string[] _keys = new string[] { };

		/// <summary>
		/// 
		/// </summary>
		public DisplayMode DisplayMode { get { return _displayMode; } set { _displayMode = value; } }
		private DisplayMode _displayMode = DisplayMode.Always;

		/// <summary>
		/// 
		/// </summary>
		public CommandLineArgumentOptions Options { get { return _options; } set { _options = value; } }
		private CommandLineArgumentOptions _options = CommandLineArgumentOptions.NameValueOptional;

		/// <summary>
		/// 
		/// </summary>
		public Type InteractiveClass { get { return _interactiveClass; } set { _interactiveClass = value; } }
		private Type _interactiveClass = null;

		/// <summary>
		/// The name of the value 'cmd'.
		/// usage: blah
		///   -arg 'cmd'
		/// </summary>
		public string ExpressionLabel { get { return _expressionLabel ?? (_expressionLabel = string.Empty); } set { _expressionLabel = value ?? string.Empty; } }
		private string _expressionLabel = string.Empty;

		/// <summary>
		/// The list of allowed values of 'cmd'.
		/// usage: blah
		///   -arg 'cmd'   allowed values for 'cmd' include: ExpressionsAllowed.. 
		/// </summary>
		public Dictionary<string, string> ExpressionsAllowed { get { return _expressionsAllowed ?? (_expressionsAllowed = new Dictionary<string, string>()); } set { _expressionsAllowed = value ?? new Dictionary<string, string>(); } }
		private Dictionary<string, string> _expressionsAllowed = new Dictionary<string, string>();
		//public string[] ExpressionsAllowed { get { return _expressionsAllowed ?? (_expressionsAllowed = new string[] { }); } set { _expressionsAllowed = value ?? new string[] { }; } }
		//private string[] _expressionsAllowed = new string[] { };

		[Obsolete("use ExpressionsAllowed instead.", true)]
		public string[] AllowedExpressions { get; set; }

		/// <summary>
		/// Gets or sets whether the current command line argument is enabled or not.
		/// </summary>
		public bool Enabled { get { return _enabled; } set { _enabled = value; } }
		private bool _enabled = true;

		/// <summary>
		/// Gets whether the argument is present on the command-line or in the environment variables.
		/// <seealso cref="Exists"/> tells you whether or not the argument exists on the command-line,
		/// where <seealso cref="hasValue"/> tells you that it exists on the command-line and has a (non-empty) value.
		/// </summary>
		public bool Exists { get { return IsArgument || IsEnvironmentVariable; } }

		/// <summary>
		/// Gets whether the argument has been set.
		/// <seealso cref="Exists"/> tells you whether or not the argument exists on the command-line,
		/// where <seealso cref="hasValue"/> tells you that it exists on the command-line and has a (non-empty) value.
		/// </summary>
		public bool HasValue { get { return _hasValue; } protected internal set { _hasValue = value; } }
		private bool _hasValue = false;

		/// <summary>
		/// Gets or sets whether the value was read from the command-line arguments.
		/// </summary>
		public bool IsArgument { get { return _isArgument; } set { _isArgument = value; } }
		private bool _isArgument = false;

		/// <summary>
		/// 
		/// </summary>
		public EventHandler Handler { get; set; }

		/// <summary>
		/// Gets or sets whether the value can be read from the environment variables.
		/// A value specified on the command-line will ALWAYS take precedence over the environment variable.
		/// </summary>
		public bool AllowEnvironmentVariable { get { return _allowEnvironmentVariable; } set { _allowEnvironmentVariable = value; } }
		private bool _allowEnvironmentVariable = true;

		/// <summary>
		/// Gets or sets whether the value was read from the environment variable.
		/// </summary>
		public bool IsEnvironmentVariable { get { return _isEnvironmentVariable; } set { _isEnvironmentVariable = value; } }
		private bool _isEnvironmentVariable = false;

		/// <summary>
		/// Gets or sets whether the value is the default value (and not found in the command-line arguments, nor in the environment variables).
		/// </summary>
		public bool IsDefault { get { return _isDefault; } set { _isDefault = value; } }
		private bool _isDefault = false;

		/// <summary>
		/// Gets or sets whether the value can be read and written to the config (settings) file.
		/// </summary>
		public bool AllowConfig { get { return _allowConfig; } set { _allowConfig = value; } }
		private bool _allowConfig = false;

		/// <summary>
		/// Gets or sets whether the value was read from config (the settings file).
		/// </summary>
		public bool IsConfigItem { get { return _isConfigItem; } set { _isConfigItem = value; } }
		private bool _isConfigItem = false;
	}

	public class CommandLineArg<T> : CommandLineArg
	{
		/// <summary>
		/// 
		/// </summary>
		public delegate Result ValidateEventHandler( CommandLine CmdLine, CommandLineArg<T> arg );

#pragma warning disable 67

		/// <summary>
		/// Provides a method to ensure the validity of the command-line argument.
		/// </summary>
		public event ValidateEventHandler Validate;

#pragma warning restore 67

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>
		public Result OnValidate( CommandLine cmdLine, CommandLineArg<T> arg )
		{
			if (Validate != null) {
				return Validate(cmdLine, arg);
			}
			return new Result(Result.Okay, "");
		}

		/// <summary>
		/// Gets or sets the value of this command-line argument.
		/// </summary>
		public T Value { get { return _value != null ? _value : (_value = default(T)); } set { _value = value != null ? value : default(T); } }
		private T _value = default(T);

		/// <summary>
		/// Gets or sets the default value used if the Value was not set.
		/// </summary>
		public T Default { get { return _default != null ? _default : (_default = default(T)); } set { _default = value != null ? value : default(T); } }
		private T _default = default(T);

		#region Operator overloads

		// relational operators

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj1"></param>
		/// <param name="obj2"></param>
		/// <returns></returns>
		public static bool operator ==( CommandLineArg<T> obj1, CommandLineArg<T> obj2 )
		{
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(obj1, obj2)) {
				return true;
			}

			// If one is null, but not both, return false.
			// The ^ is an exclusive-or.
			if ((object)obj1 == null ^ (object)obj2 == null) {
				return false;
			}

			return obj1.Equals(obj2);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj1"></param>
		/// <param name="obj2"></param>
		/// <returns></returns>
		public static bool operator !=( CommandLineArg<T> obj1, CommandLineArg<T> obj2 ) { return !(obj1 == obj2); }

		//public static bool operator <( CommandLineArg<short> val1, CommandLineArg<short> val2 ) { return val1.Value < val2.Value; }
		//public static bool operator <( CommandLineArg<int> val1, CommandLineArg<int> val2 ) { return val1.Value < val2.Value; }
		//public static bool operator <( CommandLineArg<long> val1, CommandLineArg<long> val2 ) { return val1.Value < val2.Value; }

		//public static bool operator <=( CommandLineArg<short> val1, CommandLineArg<short> val2 ) { return val1.Value <= val2.Value; }
		//public static bool operator <=( CommandLineArg<int> val1, CommandLineArg<int> val2 ) { return val1.Value <= val2.Value; }
		//public static bool operator <=( CommandLineArg<long> val1, CommandLineArg<long> val2 ) { return val1.Value <= val2.Value; }

		//public static bool operator >( CommandLineArg<short> val1, CommandLineArg<short> val2 ) { return val1.Value > val2.Value; }
		//public static bool operator >( CommandLineArg<int> val1, CommandLineArg<int> val2 ) { return val1.Value > val2.Value; }
		//public static bool operator >( CommandLineArg<long> val1, CommandLineArg<long> val2 ) { return val1.Value > val2.Value; }

		//public static bool operator >=( CommandLineArg<short> val1, CommandLineArg<short> val2 ) { return val1.Value >= val2.Value; }
		//public static bool operator >=( CommandLineArg<int> val1, CommandLineArg<int> val2 ) { return val1.Value >= val2.Value; }
		//public static bool operator >=( CommandLineArg<long> val1, CommandLineArg<long> val2 ) { return val1.Value >= val2.Value; }

		// assignment and cast operators

		//public static explicit operator CommandLineArg<T>( T Value ) { return new CommandLineArg<T>(Value); }

		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator string( CommandLineArg<T> value )
		//{
		//	//if (value.Value is string && typeof(T) == typeof(string)) {
		//	return Convert.ToString(value.Value);
		//	//}
		//	//throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator bool( CommandLineArg<T> value )
		//{
		//	if (value.Value is bool && typeof(T) == typeof(bool)) {
		//		if (value.Value != null) {
		//			return Convert.ToBoolean(value.Value);
		//		} else {
		//			return false;
		//		}
		//	}
		//	throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator DateTime( CommandLineArg<T> value )
		//{
		//	if (value.Value is DateTime && typeof(T) == typeof(DateTime)) {
		//		return Convert.ToDateTime(value.Value);
		//	}
		//	throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static explicit operator Int16( CommandLineArg<T> value )
		//{
		//	if (value.Value is Int16 && typeof(T) == typeof(Int16)) {
		//		return Convert.ToInt16(value.Value);
		//	}
		//	throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator Int32( CommandLineArg<T> value )
		//{
		//	if (value.Value is Int32 && typeof(T) == typeof(Int32)) {
		//		return Convert.ToInt32(value.Value);
		//	}
		//	throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator Int64( CommandLineArg<T> value )
		//{
		//	if (value.Value is Int64 && typeof(T) == typeof(Int64)) {
		//		return Convert.ToInt64(value.Value);
		//	}
		//	throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator UInt16( CommandLineArg<T> value )
		//{
		//	if (value.Value is UInt16 && typeof(T) == typeof(UInt16)) {
		//		return Convert.ToUInt16(value.Value);
		//	}
		//	throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator UInt32( CommandLineArg<T> value )
		//{
		//	if (value.Value is UInt32 && typeof(T) == typeof(UInt32)) {
		//		return Convert.ToUInt32(value.Value);
		//	}
		//	throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator UInt64( CommandLineArg<T> value )
		//{
		//	if (value.Value is UInt64 && typeof(T) == typeof(UInt64)) {
		//		return Convert.ToUInt64(value.Value);
		//	}
		//	throw new InvalidCastException();
		//}
		//public static explicit operator int[]( CommandLineArg<int[]> value )
		//{
		//   if (value.Value is int[]) {
		//      return value.Value;
		//   }
		//   throw new InvalidCastException();
		//}

		#endregion

		/// <summary>
		/// 
		/// </summary>
		public CommandLineArg() : this(default(T)) { }

		/// <summary>
		/// 
		/// </summary>
		public CommandLineArg( T DefaultValue ) { Default = DefaultValue; }

		/// <summary>
		/// 
		/// </summary>
		public override int GetHashCode() { return Value.GetHashCode(); }

		/// <summary>
		/// 
		/// </summary>
		public override bool Equals( object value )
		{
			CommandLineArg<T> tmp;

			// If parameter is null return false.
			if (value == null) {
				return false;
			}

			// If parameter cannot be cast to CommandLineArg<T>, return false.
			tmp = value as CommandLineArg<T>;
			if ((System.Object)tmp == null) {
				return false;
			}

			return base.Equals(tmp);
		}

		/// <summary>
		/// 
		/// </summary>
		public bool Equals( CommandLineArg<T> value )
		{
			// TODO 
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		public override string ToString() { return Value.ToString(); }
	}

	/// <summary>
	/// Indicates what to expect and what is allowed for the command-line argument's values.
	/// </summary>
	[Flags]
	public enum CommandLineArgumentOptions
	{
		///// <summary>
		///// 
		///// </summary>
		//NotSet = 0,
		/// <summary>
		/// There will not be any value specified. (No value allowed.)
		/// </summary>
		NameOnly = 1,
		/// <summary>
		/// There may or may not be a value specified. (A value is optional.)
		/// </summary>
		NameValueOptional = 2,
		/// <summary>
		/// There will be a value specified. (A value is required.)
		/// </summary>
		NameValueRequired = 4,
		/// <summary>
		/// There may be multiple values specified. (At least one value is required.)
		/// </summary>
		NameRemainingValues = 8,
		/// <summary>
		/// There is no name. The value is the first argument without a prefix. (The name is optional.)
		/// </summary>
		UnnamedItem = 16,
		/// <summary>
		/// There is no name. The value is the first argument without a prefix. (The name is required.)
		/// </summary>
		UnnamedItemRequired = 32,
		/// <summary>
		/// 
		/// </summary>
		NameOnlyInteractive = 64
	}

	/// <summary>
	/// Indicates when (if ever) the command-line argument should be displayed in the usage content.
	/// </summary>
	public enum DisplayMode
	{
		/// <summary>
		/// Always displays the command-line argument.
		/// </summary>
		Always,
		/// <summary>
		/// Displays the command-line argument only when the /hidden flag was specified.
		/// </summary>
		Hidden,
		/// <summary>
		/// Will never display the command-line argument as even existing.
		/// </summary>
		Never
	}

	/// <summary>
	/// 
	/// </summary>
	public class Result
	{
		/// <summary>
		/// 
		/// </summary>
		public const int Okay = 0;

		/// <summary>
		/// 
		/// </summary>
		public const int Success = 0;

		/// <summary>
		/// 
		/// </summary>
		public const int Error = 1;

		//public readonly Result Success = new Result(0);
		//public readonly Result Error = new Result(1);

		/// <summary>
		/// 
		/// </summary>
		public int code { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		public Result Code( int code )
		{
			this.code = code;
			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		public string message { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public Result Message( string message )
		{
			this.message = message;
			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		public Result() : this(0, string.Empty) { }

		/// <summary>
		/// 
		/// </summary>
		public Result( int code ) : this(code, string.Empty) { }

		/// <summary>
		/// 
		/// </summary>
		public Result( int code, string message )
		{
			this.code = code;
			this.message = message;
		}
	}
}
