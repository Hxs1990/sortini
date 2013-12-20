//
// Copyright (C) 2004-2007 Kody Brown (kody@bricksoft.com).
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
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace Bricksoft.PowerCode
{
	/// <summary>Xml helper/utility class.</summary>
	public static class XmlExtensions
	{


		/* ------------------------------------------------------------------------------------------ */
		/* ----- XmlNode ---------------------------------------------------------------------------- */
		/* ------------------------------------------------------------------------------------------ */

		/* ----- GetOrCreateNodePath() -------------------------------------------------- */

		/// <summary>
		/// Creates a node for each missing element in <paramref name="XPath"/> and 
		/// returns the last node element.
		/// </summary>
		/// <param name="Document">The XmlDocument.</param>
		/// <param name="XPath">The XPath expression.</param>
		/// <returns></returns>
		public static XmlNode GetOrCreateNodePath( this XmlDocument Document, string XPath )
		{
			XmlNode node;
			XmlNode tempNode;
			List<string> parts;

			if (Document == null) {
				throw new ArgumentNullException("doc");
			}
			if (XPath == null) {
				throw new ArgumentNullException("nodePath");
			}

			if ((node = Document.SelectSingleNode(XPath)) != null) {
				return node;
			}

			if (XPath.IndexOf("/") == -1) {
				return Document.DocumentElement.AppendChild(
					Document.CreateNode(XmlNodeType.Element, XPath, ""));
			}

			node = Document.DocumentElement;

			parts = new List<string>(XPath.Split(new char[] { '/' }
					, StringSplitOptions.RemoveEmptyEntries));
			if (node.Name.Equals(parts[0], StringComparison.CurrentCultureIgnoreCase)) {
				parts.RemoveAt(0);
			}

			foreach (string el in parts) {
				if (node == null) {
					return null;
				}
				tempNode = node.SelectSingleNode(el);
				if (tempNode != null) {
					node = tempNode;
				} else {
					node = node.AppendChild(Document.CreateNode(XmlNodeType.Element, el, ""));
				}
			}

			return node;
		}

		/* ----- GetOrCreateChild() -------------------------------------------------- */

		/// <summary>
		/// Returns the child node named <paramref name="NodeName"/> or creates it then returns it. This method supports specifying basic XPath in the <paramref name="NodeName"/>.
		/// </summary>
		/// <remarks>Part of Kody's Xml library</remarks>
		/// <param name="ParentNode">The XmlNode object to start the search from.</param>
		/// <param name="NodeName">The name of the child name. The name can include (basic) XPath syntax specifying only the path (i.e.: Parent/Child).</param>
		/// <returns>The child as an XmlNode object. If the child node is not found, it will return null.</returns>
		public static XmlNode GetOrCreateChild( this XmlNode ParentNode, string NodeName )
		{
			XmlNode n;

			n = ParentNode.SelectSingleNode(NodeName);
			if (n != null) {
				return n;
			}

			n = ParentNode.OwnerDocument.CreateNode(XmlNodeType.Element, NodeName, "");
			ParentNode.AppendChild(n);

			return n;
		}


		/// <summary>
		/// Returns the child node named <paramref name="NodeName"/> or creates it then returns it. This method supports specifying basic XPath in the <paramref name="NodeName"/>.
		/// </summary>
		/// <remarks>Part of Kody's Xml library</remarks>
		/// <param name="doc">The XmlDocument object that <paramref name="ParentNode"/> belongs to.</param>
		/// <param name="ParentNode">The XmlNode object to start the search from.</param>
		/// <param name="NodeName">The name of the child name. The name can include (basic) XPath syntax specifying only the path (i.e.: Parent/Child).</param>
		/// <returns>The child as an XmlNode object. If the child node is not found, it will return null.</returns>
		public static XmlNode GetOrCreateChild( this XmlDocument doc, XmlNode ParentNode, string NodeName )
		{
			return GetOrCreateChild(doc, ParentNode, NodeName, "", "", false);
		}

		/// <summary>
		/// Returns the child node named <paramref name="NodeName"/> or creates it then returns it. This method supports specifying basic XPath in the <paramref name="NodeName"/>.
		/// </summary>
		/// <remarks>Part of Kody's Xml library</remarks>
		/// <param name="doc">The XmlDocument object that <paramref name="ParentNode"/> belongs to.</param>
		/// <param name="ParentNode">The XmlNode object to start the search from.</param>
		/// <param name="NodeName">The name of the child name. The name can include (basic) XPath syntax specifying only the path (i.e.: Parent/Child).</param>
		/// <param name="CreateDuplicate">Whether or not to force creating a new node regardless if one already exists. Default is false.</param>
		/// <returns>The child as an XmlNode object. If the child node is not found, it will return null.</returns>
		public static XmlNode GetOrCreateChild( this XmlDocument doc, XmlNode ParentNode, string NodeName, bool CreateDuplicate )
		{
			return GetOrCreateChild(doc, ParentNode, NodeName, "", "", CreateDuplicate);
		}

		/// <summary>
		/// Returns the child node named <paramref name="NodeName"/> or creates it then returns it. Creates (or updates) the nodes <paramref name="attributeName"/> attribute to <paramref name="attributeValue"/>. This method supports specifying basic XPath in the <paramref name="NodeName"/>.
		/// </summary>
		/// <remarks>Part of Kody's Xml library</remarks>
		/// <param name="doc">The XmlDocument object that <code>Parent</code> belongs to.</param>
		/// <param name="ParentNode">The XmlNode object to start the search from.</param>
		/// <param name="NodeName">The name of the child name. The name can include (basic) XPath syntax specifying only the path (i.e.: Parent/Child).</param>
		/// <param name="attributeName">The name of the attribute to create or update.</param>
		/// <param name="attributeValue">The value of the attribute to create or update.</param>
		/// <returns>The child as an XmlNode object. If the child node is not found, it will return null.</returns>
		public static XmlNode GetOrCreateChild( this XmlDocument doc, XmlNode ParentNode
				, string NodeName, string attributeName, string attributeValue )
		{

			return GetOrCreateChild(doc, ParentNode, NodeName, attributeName, attributeValue, false);
		}

		/// <summary>
		/// Returns the child node named <paramref name="NodeName"/> or creates it then returns it.
		/// Creates (or updates) the nodes <paramref name="attributeName"/> attribute to <paramref name="attributeValue"/>.
		/// This method supports specifying basic XPath in the <paramref name="NodeName"/>.
		/// </summary>
		/// <remarks>Part of Kody's Xml library</remarks>
		/// <param name="doc">The XmlDocument object that <code>Parent</code> belongs to.</param>
		/// <param name="ParentNode">The XmlNode object to start the search from.</param>
		/// <param name="NodeName">The name of the child name. The name can include (basic) XPath syntax specifying only the path (i.e.: Parent/Child).</param>
		/// <param name="attributeName">The name of the attribute to create or update.</param>
		/// <param name="attributeValue">The value of the attribute to create or update.</param>
		/// <param name="CreateDuplicate">Whether or not to force creating a new node regardless if one already exists. Default is false.</param>
		/// <returns>The child as an XmlNode object. If the child node is not found, it will return null.</returns>
		public static XmlNode GetOrCreateChild( this XmlDocument doc, XmlNode ParentNode, string NodeName, string attributeName, string attributeValue, bool CreateDuplicate )
		{
			XmlNode newNode;

			if (ParentNode == null) {
				throw new ArgumentNullException("ParentNode");
			}
			if (NodeName == null || (NodeName = NodeName.Trim()).Length == 0) {
				throw new ArgumentNullException("NodeName");
			}

			newNode = null;

			if (!CreateDuplicate) {
				XmlNodeList nodeList;
				nodeList = ParentNode.ChildNodes;
				foreach (XmlNode node in nodeList) {
					if (node.Name == NodeName) {
						if (null != attributeName && 0 < (attributeName = attributeName.Trim()).Length && null != attributeValue && 0 < (attributeValue = attributeValue.Trim()).Length) {
							if (node.Attributes[attributeName] != null && node.Attributes[attributeName].Value == attributeValue) {
								return node;
							}
						} else {
							return node;
						}
					}
				}
			}

			if (null == doc) {
				return null;
			}

			newNode = doc.CreateElement("", NodeName, "");
			if (!attributeName.Equals("")) { // && !attributeValue.Equals(""))
				XmlAttribute attrib;
				attrib = doc.CreateAttribute(attributeName);
				attrib.Value = attributeValue;
				newNode.Attributes.Append(attrib);
			}
			ParentNode.AppendChild(newNode);

			return newNode;
		}

		/* ----- RemoveChild() -------------------------------------------------- */

		/// <summary>
		/// Removes the child node that matches the specified XPath expression.
		/// </summary>
		/// <param name="me">The current instance.</param>
		/// <param name="XPath">The XPath expression.</param>
		/// <returns></returns>
		public static XmlNode RemoveChild( this XmlNode me, string XPath )
		{
			XmlNode node;

			if (me == null) {
				throw new ArgumentNullException("ParentNode");
			}
			if (XPath == null || (XPath = XPath.Trim()).Length == 0) {
				throw new ArgumentNullException("NodeName");
			}

			node = me.SelectSingleNode(XPath);

			if (node != null) {
				return node.ParentNode.RemoveChild(node);
			}

			return null;
		}

		/* ----- AppendOrReplaceChild() -------------------------------------------------- */

		/// <summary>
		/// Appends <paramref name="NewChild"/> to the end of the child nodes of the current instance.
		/// If a node by the same name already exists, it is replaced.
		/// </summary>
		/// <param name="me">The current instance.</param>
		/// <param name="NewChild"></param>
		public static XmlNode AppendOrReplaceChild( this XmlNode me, XmlNode NewChild )
		{
			XmlNode oldChild;

			if (me == null) {
				throw new ArgumentNullException("me");
			}
			if (NewChild == null || NewChild.Name.Trim().Length == 0) {
				throw new ArgumentNullException("ChildNode");
			}

			oldChild = me.SelectSingleNode(NewChild.Name);

			if (oldChild != null) {
				return me.ReplaceChild(NewChild, oldChild);
			} else {
				return me.AppendChild(NewChild);
			}
		}


		/* ------------------------------------------------------------------------------------------ */
		/* ----- XmlAttribute ----------------------------------------------------------------------- */
		/* ------------------------------------------------------------------------------------------ */

		/* ----- SelectSingleAttribute() -------------------------------------------------- */

		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		/// <param name="Name"></param>
		/// <returns></returns>
		public static XmlAttribute SelectSingleAttribute( this XmlNode node, string Name )
		{
			if (node == null) {
				throw new ArgumentNullException("node");
			}
			if (Name == null) {
				throw new ArgumentNullException("Name");
			}

			if (node.Attributes[Name] != null) {
				return node.Attributes[Name];
			}

			return null;
		}

		/* ----- CreateAttribute() -------------------------------------------------- */

		/// <summary>
		/// Creates or updates an attribute of the current XmlNode.
		/// </summary>
		/// <remarks>Part of Kody's Xml library</remarks>
		/// <param name="me">The XmlNode object to create the attribute in.</param>
		/// <param name="AttrName">The attribute name.</param>
		/// <param name="AttrValue">The attribute value.</param>
		/// <returns>The XmlAttribute object that was updated.</returns>
		public static XmlAttribute CreateAttribute( this XmlNode me, string AttrName, object AttrValue )
		{
			XmlAttribute newAttr;

			foreach (XmlAttribute attr in me.Attributes) {
				if (attr.Name == AttrName) {
					attr.Value = AttrValue.ToString();
					return attr;
				}
			}

			newAttr = me.OwnerDocument.CreateAttribute("", AttrName, "");
			if (newAttr == null) {
				return newAttr;
			}
			newAttr.Value = AttrValue.ToString();
			me.Attributes.Append(newAttr);

			return newAttr;
		}

		// ----- GetAttribute --------------------------------------------------

		public static int GetValue( this XmlAttribute me, int DefaultValue )
		{
			int result;

			if (me == null || me.Value.Length == 0) {
				return DefaultValue;
			}

			if (!int.TryParse(me.Value, out result)) {
				return DefaultValue;
			}

			return result;
		}

		// ----- GetAttribute --------------------------------------------------

		public static object GetAttrValue( this XmlNode me, string AttributeName, object DefaultValue ) { return GetAttrValue(me, AttributeName, StringComparison.CurrentCulture, DefaultValue); }

		public static object GetAttrValue( this XmlNode me, string AttributeName, StringComparison StringComparison, object DefaultValue )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}
			if (AttributeName == null) {
				throw new ArgumentNullException("AttributeName");
			}
			if ((AttributeName = AttributeName.Trim()).Length == 0) {
				throw new ArgumentException("AttributeName is required", "AttributeName");
			}

			if (me.Attributes != null && me.Attributes.Count > 0) {
				foreach (XmlAttribute attr in me.Attributes) {
					if (attr.Name.Equals(AttributeName, StringComparison)) {
						return attr.Value;
					}
				}
			}

			return DefaultValue;
		}

		public static T GetAttrValue<T>( this XmlNode me, string AttributeName, T DefaultValue ) { return GetAttrValue<T>(me, AttributeName, StringComparison.CurrentCulture, DefaultValue); }

		public static T GetAttrValue<T>( this XmlNode me, string AttributeName, StringComparison StringComparison, T DefaultValue )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}
			if (AttributeName == null) {
				throw new ArgumentNullException("AttributeName");
			}
			if ((AttributeName = AttributeName.Trim()).Length == 0) {
				throw new ArgumentException("AttributeName is required", "AttributeName");
			}

			if (me.Attributes != null && me.Attributes.Count > 0) {
				foreach (XmlAttribute attr in me.Attributes) {
					if (attr.Name.Equals(AttributeName, StringComparison)) {
						return (T)Convert.ChangeType(attr.Value, typeof(T));
					}
				}
			}

			return DefaultValue;
		}

		public static bool Contains( this XmlAttributeCollection me, string Name ) { return Contains(me, Name, StringComparison.CurrentCulture); }

		public static bool Contains( this XmlAttributeCollection me, string Name, StringComparison StringComparison )
		{
			foreach (XmlAttribute attr in me) {
				if (attr.Name.Equals(Name, StringComparison)) {
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// Returns an XmlAttribute specified by <paramref name="attributeName"/>.
		/// If the attribute does not exist it will create it.
		/// </summary>
		/// <remarks>Part of Kody's Xml library</remarks>
		/// <param name="doc">The XmlDocument object. This can be null if you are sure the attribute already exists.</param>
		/// <param name="parentNode">The XmlNode object to get the attribute from.</param>
		/// <param name="attributeName">The name of the attribute to create (or update).</param>
		//[Obsolete("Use XmlNode.GetAttribute(Name) instead.", true)]
		public static XmlAttribute GetAttribute( XmlDocument doc, XmlNode parentNode, string attributeName ) { return GetAttribute(doc, parentNode, attributeName, string.Empty); }

		/// <summary>
		/// Returns an XmlAttribute specified by <paramref name="attributeName"/>.
		/// If the attribute does not exist it will create it setting its value to <paramref name="defaultValue"/>.
		/// </summary>
		/// <remarks>Part of Kody's Xml library</remarks>
		/// <param name="doc">The XmlDocument object. This can be null if you are sure the attribute already exists.</param>
		/// <param name="parentNode">The XmlNode object to get the attribute from.</param>
		/// <param name="attributeName">The name of the attribute to create (or update).</param>
		/// <param name="defaultValue">The default value to set the attribute to if the attribute is created. This can be null.</param>
		//[Obsolete("Use XmlNode.GetAttribute(Name) instead.", true)]
		public static XmlAttribute GetAttribute( XmlDocument doc, XmlNode parentNode, string attributeName, string defaultValue )
		{
			if (null == parentNode || string.IsNullOrEmpty(attributeName)) {
				return null;
			}

			XmlAttribute temp = SelectSingleAttribute(parentNode, attributeName);
			if (null != temp) {
				return temp;
			} else {
				if (null == doc) {
					return null;
				}
				if (string.IsNullOrEmpty(defaultValue)) {
					defaultValue = string.Empty;
				}
				temp = doc.CreateAttribute(attributeName);
				temp.Value = defaultValue;
				return temp;
			}
		}

		/* ----- CopyAttributes() -------------------------------------------------- */

		/// <summary>
		/// </summary>
		/// <param name="xDoc"></param>
		/// <param name="FromNode"></param>
		/// <param name="ToNode"></param>
		public static void CopyAttributes( XmlDocument xDoc, XmlNode FromNode, XmlNode ToNode )
		{
			if (ToNode == null) {
				throw new ArgumentNullException("ToNode");
			}
			if (ToNode == null) {
				throw new ArgumentNullException("ToNode");
			}
			if (ToNode == null) {
				throw new ArgumentNullException("ToNode");
			}

			foreach (XmlAttribute attr in FromNode.Attributes) {
				ToNode.CreateAttribute(attr.Name, attr.Value);
			}
		}



		/* ------------------------------------------------------------------------------------------ */
		/* ----- Obsolete --------------------------------------------------------------------------- */
		/* ------------------------------------------------------------------------------------------ */

		/* ----- GetChildNode() -------------------------------------------------- */

		/// <summary>
		/// Finds and returns the child node specified by <paramref name="nodeName"/>. This method supports specifying basic XPath in the <paramref name="nodeName"/>.
		/// </summary>
		/// <remarks>Part of Kody's Xml library</remarks>
		/// <param name="parentNode">The XmlNode object to start the search from.</param>
		/// <param name="nodeName">The name of the child name. The name can include (basic) XPath syntax specifying only the path (i.e.: Parent/Child).</param>
		/// <returns>The child as an XmlNode object. If the child node is not found, it will return null.</returns>
		[Obsolete("Use XmlNode.SelectSingleNode(XPath) instead.", true)]
		public static XmlNode GetChildNode( XmlNode parentNode, string nodeName ) { return GetChildNode(null, parentNode, nodeName); }

		/// <summary>
		/// Finds and returns the child node specified by <paramref name="nodeName"/>. This method supports specifying basic XPath in the <paramref name="nodeName"/>.
		/// </summary>
		/// <remarks>Part of Kody's Xml library</remarks>
		/// <param name="doc">The XmlDocument object that <paramref name="parentNode"/> belongs to.</param>
		/// <param name="parentNode">The XmlNode object to start the search from.</param>
		/// <param name="nodeName">The name of the child name. The name can include (basic) XPath syntax specifying only the path (i.e.: Parent/Child).</param>
		/// <returns>The child as an XmlNode object. If the child node is not found, it will return null.</returns>
		[Obsolete("Use XmlNode.SelectSingleNode(XPath) instead.", true)]
		public static XmlNode GetChildNode( XmlDocument doc, XmlNode parentNode, string nodeName )
		{
			if ((null == doc && null == parentNode) || string.IsNullOrEmpty(nodeName)) {
				return null;
			}

			XmlNodeList nodeList = null;
			if (null != parentNode) {
				nodeList = parentNode.ChildNodes;
			} else {
				nodeList = doc.ChildNodes;
			}

			string tempName = nodeName;

			if (nodeName.IndexOf("/") > -1) {
				tempName = nodeName.Substring(0, nodeName.IndexOf("/"));
			}

			foreach (XmlNode node in nodeList) {
				if (node.NodeType == XmlNodeType.Comment || node.NodeType == XmlNodeType.Whitespace) {
					continue;
				}

				if (node.Name.Equals(tempName)) {
					if (nodeName.IndexOf("/") > -1) {
						XmlNode tmpNode = GetChildNode(doc, node, nodeName.Substring(nodeName.IndexOf("/") + 1)); // recursive
						if (null != tmpNode) {
							return tmpNode;
						}
					} else {
						return node;
					}
				}
			}

			return null;
		}

		/* ----- XmlNodeList Find() -------------------------------------------------- */

		[Obsolete("Use XmlNode.SelectSingleNode(XPath) instead.", true)]
		public static XmlNode Find( this XmlNodeList NodeList, XmlNode Node )
		{
			if (Node == null) {
				throw new ArgumentNullException("Node");
			}
			return Find(NodeList, Node.Name);
		}

		[Obsolete("Use XmlNode.SelectSingleNode(XPath) instead.", true)]
		public static XmlNode Find( this XmlNodeList NodeList, string Name ) { return Find(NodeList, Name, StringComparison.CurrentCulture); }

		[Obsolete("Use XmlNode.SelectSingleNode(XPath) instead.", true)]
		private static XmlNode Find( this XmlNodeList NodeList, string Name, StringComparison StringComparison )
		{
			if (NodeList == null) {
				throw new ArgumentNullException("NodeList");
			}
			if (Name == null || (Name = Name.Trim()).Length == 0) {
				throw new ArgumentNullException("Name");
			}

			foreach (XmlNode node in NodeList) {
				if (node.Name.Equals(Name, StringComparison)) {
					return node;
				}
			}

			return null;
		}

		/* ----- XmlNode SelectSingleNode() -------------------------------------------------- */

		[Obsolete("Use XmlNode.SelectSingleNode(XPath) instead.", true)]
		public static XmlNode SelectSingleNode( this XmlNodeList NodeList, string Name ) { return Find(NodeList, Name); }

		/* ----- GetRootElement() -------------------------------------------------- */

		//public static XmlNode GetRootElement(string xml) {
		//   XmlDocument myXmlDocument;

		//   myXmlDocument = new XmlDocument();
		//   myXmlDocument.PreserveWhitespace = false;
		//   myXmlDocument.LoadXml(xml);

		//   return myXmlDocument.DocumentElement;
		//}


		// ----- WriteExceptionToFile --------------------------------------------------

		/// <summary>
		/// Returns the exception specified in <paramref name="ex"/> as a XML-formatted string.
		/// </summary>
		/// <remarks>Part of Kody's Xml library</remarks>
		/// <param name="ex">The exception to write out as a XML-formatted string.</param>
		/// <returns>The exception formatted as XML.</returns>
		public static string WriteExceptionAsXml( Exception ex )
		{
			return string.Format(@"<Exception>
	<Source>{0}</Source>
	<Message>{1}</Message>
	<StackTrace>{2}</StackTrace>
	<TargetSite>{3}</TargetSite>
</Exception>", ex.Source, ex.Message, ex.StackTrace, ex.TargetSite);
		}

		/// <summary>
		/// Writes the exception specified in <paramref name="ex"/> as a XML-formatted string to a file specified in <paramref name="FileName"/>.
		/// </summary>
		/// <remarks>Part of Kody's Xml library</remarks>
		/// <param name="FileName">The name of the file to write to. An existing file will be overwritten.</param>
		/// <param name="ex">The exception to write out as a XML-formatted string.</param>
		public static void WriteExceptionToFile( string FileName, Exception ex )
		{
			using (StreamWriter fileWriter = File.AppendText(FileName)) {
				fileWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
				fileWriter.WriteLine(WriteExceptionAsXml(ex));
				fileWriter.Flush();
				fileWriter.Close();
			}
		}


		// ----- Transform --------------------------------------------------

		/// <summary>
		/// Returns a string of the result from transforming <paramref name="xmlContents"/> with <paramref name="xsltContents"/>.
		/// </summary>
		/// <remarks>Part of Kody's Xml library</remarks>
		/// <param name="xmlContents">The XML as a string.</param>
		/// <param name="xsltContents">The XSLT as a string.</param>
		public static string Transform( string xmlContents, string xsltContents )
		{
			XmlDocument xmlDocument = null;
			XslCompiledTransform xslTransform = null;

			try {
				xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(xmlContents);
				xmlDocument.PreserveWhitespace = false;
			} catch (Exception ex) {
				throw new Exception("Could not load the data elements.", ex);
			}
			if (null == xmlDocument)
				throw new Exception("the Xml Parser Could not read data file.");

			// xmlContents contains XML source generated from the elements passed into this method
			// xsltContents contains the template file's contents unchanged.

			NameTable nt = new NameTable();//Create the XmlNamespaceManager.
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
			nsmgr.AddNamespace("bk", "urn:sample");
			XmlParserContext context = new XmlParserContext(null, nsmgr, null, XmlSpace.None);//Create the XmlParserContext.
			XmlTextReader reader = new XmlTextReader(xsltContents, XmlNodeType.Document, context);//Create the reader. 

			try {
				xslTransform = new XslCompiledTransform(true);
				xslTransform.Load(reader);
			} catch (Exception ex) {
				throw new Exception("Could not transform the XML content.", ex);
			}
			if (null == xslTransform)
				throw new Exception("the Xslt Parser Could not read template file.");

			StringBuilder buffer = new StringBuilder();
			StringWriter writer = new StringWriter(buffer);
			xslTransform.Transform(xmlDocument, null, writer);

			return buffer.ToString();
		}


		// ----- Serialization of objects to and from XML --------------------------------------------------

		/// <summary>Method to convert a custom object to XML string</summary>
		/// <param name="pObject">Object that is to be serialized to XML</param>
		public static string SerializeObject( object pObject )
		{
			try {
				string XmlizedString = null;
				MemoryStream memoryStream = new MemoryStream();
				XmlSerializer xs = new XmlSerializer(pObject.GetType());
				XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
				xs.Serialize(xmlTextWriter, pObject);
				memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
				XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
				return XmlizedString.Substring("<?xml version=\"1.0\" encoding=\"utf-8\"?>".Length);
			} catch (Exception) {
				// todo: add error logging here
				return null;
			}
		}

		/// <summary>Method to reconstruct an object from XML string</summary>
		/// <param name="pXmlizedString"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static object DeserializeObject( string pXmlizedString, Type type )
		{
			XmlSerializer xs = new XmlSerializer(type);
			MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));
			XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
			return xs.Deserialize(memoryStream);
		}

		/// <summary>To convert a Byte Array of Unicode values (UTF-8 encoded) to a complete string.</summary>
		/// <param name="characters">Unicode Byte Array to be converted to String</param>
		/// <returns>String converted from Unicode Byte Array</returns>
		private static string UTF8ByteArrayToString( Byte[] characters )
		{
			UTF8Encoding encoding = new UTF8Encoding();
			string constructedString = encoding.GetString(characters);
			return (constructedString);
		}

		/// <summary>Converts the string to UTF8 Byte array and is used in De serialization</summary>
		/// <param name="pXmlString"></param>
		private static Byte[] StringToUTF8ByteArray( string pXmlString )
		{
			UTF8Encoding encoding = new UTF8Encoding();
			Byte[] byteArray = encoding.GetBytes(pXmlString);
			return byteArray;
		}

	}
}
