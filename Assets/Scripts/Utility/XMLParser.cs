// XMLParser.cs
// by: Peter Bartosch <bartoschp@gmail.com>
//
// TODO
// * Consider moving ParseState into XMLParser as a private enum
// * Comment Parse better
//
// MIT License for XMLParser:
// Copyright (c) 2010 Fraser McCormick
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;

namespace Inkspot.Utility
{
	/// <summary>
	/// XML node.  Tracks the name, text, attributes and children of an XML node.
	/// </summary>
	public class XMLNode
	{
		public XMLNode() : this("")
		{}
		
		public XMLNode(string name)
		{
			Name = name;
			Text = "";
			Children = new List<XMLNode>();
			Attributes = new Dictionary<string, string>();
		}
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="XMLParser.XMLNode"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="XMLParser.XMLNode"/>.
		/// </returns>
		public override string ToString()
		{
			/* This was for testing in a console.  When debugging in MonoDevelop this
			 * times out, so it is commented out.
			string ret = "Name: " + Name;
			ret += "\n Attributes:\n";
			
			if(Attributes.Count == 0)
				ret += "  None\n";
			else
				foreach(KeyValuePair<string, string> kvp in Attributes)
					ret+="  "+ kvp.Key + "=" + kvp.Value + "\n";
			ret+=" Children:\n";
			if(Children.Count == 0)
				ret += "  None\n";
			else
				foreach(XMLNode child in Children)
					ret += "  " + child.ToString() + "\n";
			return ret;
			 */
			return "Name: " + Name + " with " + Children.Count + " children.";
		}
		
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public string Name { get; set; }
		
		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <value>
		/// The text.
		/// </value>
		public string Text { get; set; }
		
		/// <summary>
		/// Gets or sets the children of the XMLNode.
		/// </summary>
		/// <value>
		/// The children of the XMLNode.
		/// </value>
		public List<XMLNode> Children { get; set; }
		
		/// <summary>
		/// Gets or sets the attributes of the XMLNode.
		/// </summary>
		/// <value>
		/// The attributes of the XMLNode.
		/// </value>
		public Dictionary<string, string> Attributes { get; set; }
	}
	
	[Flags]
	/// <summary>
	/// Bit flags for keeping track of the current parse state.
	/// </summary>
	internal enum ParseState
	{
		GetNodeName = 1,
		GetAttribName = 2,
		GetAttribValue = 4,
		InMetaTag = 8,
		InComment = 16,
		InDoctype = 32,
		InCDATA = 64,
		Parse = 128,
		InElement = 256
	};
	
	/// <summary>
	/// XML parser adapted from the XMLParser by Fraser McCormick at http://dev.grumpyferret.com/unity/
	///  using the MIT license.  This does no validation.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The main differences between that and this (aside from language) is that I use bitflags to
	/// keep track of the parsing state, where the original uses booleans.   Additionally, rather
	/// than wrap a dictionary for the XMLNode, I made a custom class.  Parent nodes are tracked with a
	/// generic Stack rather than a custom class.
	/// </para>
	/// </remarks>
	public static class XMLParser
	{
		//shortcut names for commonly used characters
		private static readonly char LT='<';
		private static readonly char GT='>';
		private static readonly char SPACE=' ';
		private static readonly char QUOTE='"';
		private static readonly char SLASH='/';
		private static readonly char QUEST_MARK='?';
		private static readonly char EQUALS='=';
		private static readonly char BANG='!';
		private static readonly char HYPHEN='-';
		private static readonly char SQR_RGT=']';

		/// <summary>
		/// Parse the specified string.
		/// </summary>
		/// <param name="text">The text string of assumed XML to parse.</param>
		/// <returns>An <see cref="XMLNode"/> representing the root node of the XML document.</returns>
		/// <remarks>
		/// <para>
		/// It should first be noted that this does no validation.  It just assumes that what is
		/// passed in is valid XML.  This means that the XML needs to validated elsewhere, or you need to
		/// be aware that garbage output can occur.
		/// </para>
		/// <para>
		/// This does a character by character parse, keeping track of the <see cref="ParseState"/> to deterimine
		/// what needs to be done with the current character.  As needed an <see cref="XMLNode"/> will be created
		/// </para>
		/// </remarks>
		public static XMLNode Parse(string text)
		{
			XMLNode root = new XMLNode();
			root.Name = "DOCROOT";
			bool quoted = false;
			string attribName = "";
			string attribValue = "";
			string nodeName = "";
			string textVal = "";
			
			Stack<XMLNode> parents = new Stack<XMLNode>();
			XMLNode curNode = root;
			
			ParseState state = ParseState.Parse;
			ParseState special = (ParseState.InCDATA | ParseState.InComment | ParseState.InDoctype | ParseState.InMetaTag);


			for(int i=0;i<text.Length;++i)
			{
				char c = text[i];
				char cn = (i+1 < text.Length) ? text[i+1] : (char)0;
				char cnn = (i+2 < text.Length) ? text[i+2] : (char)0;
				char cp = (i-1 >= 0) ? text[i-1] : (char)0;
				
				//parse for special tags
				int delta = parseSpecial(text, c, cn, cnn, cp, i, quoted, ref state, ref textVal);
				i+=delta;
				if((state & special) != 0)
					continue;
				
				//if we aren't dealing with a special tag, then just parse as normal
				if((state & ParseState.InElement) != 0)
				{
					if((state & ParseState.GetNodeName) != 0)
					{
						if(c == SPACE)
						{
							//hit a space in the element name, we are
							//no longer getting its name
							state = state & ~ParseState.GetNodeName;
						}
						else if(c == GT)
						{
							//at the end of the element tag, set flags appropriately
							state = state & ~ParseState.GetNodeName;
							state = state & ~ParseState.InElement;
						}
										
						//if we have a full node name, then check it out
						if((state & ParseState.GetNodeName) == 0 && nodeName.Length > 0)
						{
							//tag is closing tag
							if(nodeName[0] == SLASH)
							{
								if(textVal.Length > 0)
									curNode.Text+=textVal;
								
								//reset values for nodes and move up one level
								textVal = "";
								nodeName = "";
								curNode = parents.Pop();
							}
							else
							{
								//assign whatever text value we have to the current node
								if(textVal.Length > 0)
									curNode.Text += textVal;
								
								//reset textVal since we've consumed it
								textVal = "";
								
								//make a new node using the name we've collected
								XMLNode newNode = new XMLNode();
								newNode.Name = nodeName;
								curNode.Children.Add(newNode);
								
								//add the current node the stack of parents and
								//descend a level
								parents.Push(curNode);
								curNode = newNode;
								
								//reset nodeName
								nodeName = "";
							}
						}
						else if((state & ParseState.GetNodeName) != 0)
						{
							nodeName += c;
						}
					}
					else
					{
						//we're in an element tag but not getting the node name
						//we must be dealing with attributes
						
						//end of self-closing tag element
						if(!quoted && c==SLASH && cn==GT)
						{
							//clear flags for element and getting attributes
							state = state & ~ParseState.InElement;
							state = state & ~ParseState.GetAttribName;
							state = state & ~ParseState.GetAttribValue;
							
							if(attribName != "")
							{
								if(attribValue != "")
								{
									curNode.Attributes[attribName] = attribValue;
								}
								else
								{
									//no value, but need to note that the attribute exists
									curNode.Attributes[attribName] = null;
								}
								
								//consume cn
								i++;
								
								//move up one level since this tag is closed
								curNode = parents.Pop();
								
								//reset attribute tracking
								attribName = "";
								attribValue = "";
							}
						}
						else if(!quoted && c==GT)
						{
							//end of a tag, but not closed
							//clear flags for element and getting attributes
							state = state & ~ParseState.InElement;
							state = state & ~ParseState.GetAttribName;
							state = state & ~ParseState.GetAttribValue;
							
							if(attribName != "")
							{
								curNode.Attributes[attribName] = attribValue;
								
								//reset attribute tracking
								attribName = "";
								attribValue = "";
							}
						}
						else
						{
							//still in a tag, collect attribute data
							
							if((state & ParseState.GetAttribName) != 0)
							{
								if(c==SPACE || c==EQUALS)
								{
									//done with the name, move on to collecting the value
									state = state & ~ParseState.GetAttribName;
									state = state | ParseState.GetAttribValue;
								}
								else
								{
									attribName += c;
								}
							}
							else if((state & ParseState.GetAttribValue) != 0)
							{
								if(c==QUOTE)
								{
									if(quoted)
									{
										//at the end of the attrib value
										//assign it and reset
										state = state & ~ParseState.GetAttribValue;
										curNode.Attributes[attribName] = attribValue;
										attribName = "";
										attribValue = "";
										quoted = false;
									}
									else
									{
										quoted = true;
									}
								}
								else
								{
									if(quoted)
									{
										//in the middle of attribute, just add to data
										attribValue += c;
									}
									else if(c == SPACE)
									{
										//at the end of the attrib value
										//assign it and reset
										state = state & ~ParseState.GetAttribValue;
										curNode.Attributes[attribName] = attribValue;
										attribName = "";
										attribValue = "";
									}
								}
							}
							else if(c == SPACE)
							{
								//left blank in source doc, not sure why
								//I assume this is to essentially make an != SPACE else if
								//but for now I will leave as I found it.
							}
							else
							{
								state = state | ParseState.GetAttribName;
								attribName = "" + c;
								attribValue = "";
								quoted = false;
							}
						}
					}
					
				} //end of if(state & ParseState.InElement != 0)
				else
				{
					if(c == LT)
					{
						//beginning of tag
						state = state | ParseState.InElement;
						state = state | ParseState.GetNodeName;
					}
					else
					{
						//collect the value of the tag
						textVal += c;
					}
				}
			}
			
			//done with parsing, return the root node
			return root;
		}

		/// <summary>
		/// Parses the XML string for any special tags.
		/// </summary>
		/// <returns>The amount to move the character index forward.</returns>
		/// <param name="text">The text we're currently parsing.</param>
		/// <param name="c">The current character.</param>
		/// <param name="cn">The next character.</param>
		/// <param name="cnn">The character after the next character.</param>
		/// <param name="cp">The previous character.</param>
		/// <param name="i">The index we're parsing at..</param>
		/// <param name="quoted">If set to <c>true</c> we're using quoted text.</param>
		/// <param name="state">The current parsing state.</param>
		/// <param name="textVal">The accumulated text value for setting the value of an attribute or node.</param>
		private static int parseSpecial(string text, char c, char cn, char cnn, char cp, int i, bool quoted, ref ParseState state, ref string textVal)
		{
			int ret = 0;
			//first step, see if we are in an xml metatag
			//if we are currently in one, check to see if it has ended and unflag
			//if this is so
			//if we are not in one, check to see if we are entering one, and flag
			//as needed
			if(state == ParseState.InMetaTag)
			{
				//end of a meta tag is ?>
				if(c == QUEST_MARK && cn == GT)
				{
					state = ParseState.Parse;
					ret = 1; //since we consumed cn in this, we can skip it
				}
				return ret; //no matter what go back to beginning
			}
			else
			{
				//if we are being quoted then the check doesn't matter
				//otherwise beginning of metatag is <?
				if(!quoted && c == LT && cn == QUEST_MARK)
				{
					state = ParseState.InMetaTag;
					ret = 1; //different from original, but I assume that the consume matters here too
					return ret;
				}
			}
			
			switch(state)
			{
			case ParseState.InDoctype:
				//end of a !DOCTYPE declaration is >
				if(cn == GT)
				{
					state = ParseState.Parse;
					ret = 1;
				}
				break;
			case ParseState.InComment:
				//check to see if the comment is done
				//if it is not, we don't care about the contents
				//and just continue
				//end ofa comment is -->
				if(cp == HYPHEN && c == HYPHEN && cn == GT)
				{
					state = ParseState.Parse;
					ret = 1; //consume the next character
				}
				break;
			case ParseState.InCDATA:
				//first are we done?
				//end of cdata is ]]>
				if(c == SQR_RGT && cn == SQR_RGT && cnn == GT)
				{
					state = ParseState.Parse;
					ret = 2;
					return ret;
				}
				textVal+=c;
				break;
			default:
				//check to see if we are in any special tags
				//the special tags we're checking here all begin
				// with <!
				if(!quoted && c==LT && cn==BANG)
				{
					if(text.Length > i+9 && text.Substring(i,9).Equals("<![CDATA["))
					{
						state = ParseState.InCDATA;
						ret = 8;
					}
					else if(text.Length > i+9 && text.Substring(i,9).Equals("<!DOCTYPE"))
					{
						state = ParseState.InDoctype;
						ret = 8;
					}
					else if(cn == HYPHEN && cnn == HYPHEN)
					{
						state = ParseState.InComment;
						ret = 2;
					}
				}
				break;
			}
			
			return ret;
		}
	}
}