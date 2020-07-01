#region license
/* * * * *
 * A simple JSON Parser / builder
 * ------------------------------
 * Written by Bunny83
 * 2012-06-09
 *
 * Modified by zorgesho
 *
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2019 Markus Göbel (Bunny83)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * * * * */
#endregion

using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Common.Utils
{
	static class SimpleJSON
	{
		public static Node Parse(string json) => Node.Parse(json);

		public enum NodeType
		{
			Array = 1,
			Object = 2,
			String = 3,
			Number = 4,
			NullValue = 5,
			Boolean = 6,
			None = 7,
			Custom = 0xFF,
		}
		public enum TextMode
		{
			Compact,
			Indent
		}

		public abstract class Node
		{
			#region enumerators

			public struct Enumerator
			{
				enum Type { None, Array, Object }
				readonly Type type;

				Dictionary<string, Node>.Enumerator mObject;
				List<Node>.Enumerator mArray;

				public bool IsValid => type != Type.None;

				public Enumerator(List<Node>.Enumerator aArrayEnum)
				{
					type = Type.Array;
					mObject = default;
					mArray = aArrayEnum;
				}
				public Enumerator(Dictionary<string, Node>.Enumerator aDictEnum)
				{
					type = Type.Object;
					mObject = aDictEnum;
					mArray = default;
				}
				public KeyValuePair<string, Node> Current
				{
					get
					{
						if (type == Type.Array)
							return new KeyValuePair<string, Node>(string.Empty, mArray.Current);
						else if (type == Type.Object)
							return mObject.Current;
						return new KeyValuePair<string, Node>(string.Empty, null);
					}
				}
				public bool MoveNext()
				{
					if (type == Type.Array)
						return mArray.MoveNext();
					else if (type == Type.Object)
						return mObject.MoveNext();
					return false;
				}
			}

			public struct ValueEnumerator
			{
				Enumerator mEnumerator;
				public ValueEnumerator(List<Node>.Enumerator aArrayEnum): this(new Enumerator(aArrayEnum)) {}
				public ValueEnumerator(Dictionary<string, Node>.Enumerator aDictEnum): this(new Enumerator(aDictEnum)) {}
				public ValueEnumerator(Enumerator aEnumerator) { mEnumerator = aEnumerator; }

				public Node Current => mEnumerator.Current.Value;
				public bool MoveNext() => mEnumerator.MoveNext();
				public ValueEnumerator GetEnumerator() => this;
			}

			public struct KeyEnumerator
			{
				Enumerator mEnumerator;
				public KeyEnumerator(List<Node>.Enumerator aArrayEnum): this(new Enumerator(aArrayEnum)) {}
				public KeyEnumerator(Dictionary<string, Node>.Enumerator aDictEnum): this(new Enumerator(aDictEnum)) {}
				public KeyEnumerator(Enumerator aEnumerator) { mEnumerator = aEnumerator; }

				public string Current => mEnumerator.Current.Key;
				public bool MoveNext() => mEnumerator.MoveNext();
				public KeyEnumerator GetEnumerator() => this;
			}

			public class LinqEnumerator: IEnumerator<KeyValuePair<string, Node>>, IEnumerable<KeyValuePair<string, Node>>
			{
				Node mNode;
				Enumerator mEnumerator;
				public LinqEnumerator(Node aNode)
				{
					mNode = aNode;
					if (mNode != null)
						mEnumerator = mNode.GetEnumerator();
				}
				public KeyValuePair<string, Node> Current => mEnumerator.Current;
				object IEnumerator.Current => mEnumerator.Current;
				public bool MoveNext() => mEnumerator.MoveNext();

				public void Dispose()
				{
					mNode = null;
					mEnumerator = new Enumerator();
				}

				public IEnumerator<KeyValuePair<string, Node>> GetEnumerator() => new LinqEnumerator(mNode);

				IEnumerator IEnumerable.GetEnumerator() => new LinqEnumerator(mNode);

				public void Reset()
				{
					if (mNode != null)
						mEnumerator = mNode.GetEnumerator();
				}
			}
			#endregion enumerators

			#region common interface

			public static bool forceASCII = false; // Use Unicode by default
			public static bool longAsString = false; // lazy creator creates a String instead of Number
			public static bool allowLineComments = true; // allow "//"-style comments at the end of a line

			public abstract NodeType Tag { get; }

			public virtual Node this[int aIndex]  { get => null; set {} }
			public virtual Node this[string aKey] { get => null; set {} }

			public virtual string Value { get => ""; set {} }

			public virtual int Count => 0;

			public virtual bool IsNumber  => false;
			public virtual bool IsString  => false;
			public virtual bool IsBoolean => false;
			public virtual bool IsNull	  => false;
			public virtual bool IsArray	  => false;
			public virtual bool IsObject  => false;

			public virtual bool Inline { get => false; set {} }

			public virtual void Add(string aKey, Node aItem) {}
			public virtual void Add(Node aItem) => Add("", aItem);

			public virtual Node Remove(string aKey) => null;
			public virtual Node Remove(int aIndex)  => null;
			public virtual Node Remove(Node aNode) => aNode;

			public virtual Node Clone() => null;

			public virtual IEnumerable<Node> Children { get { yield break; } }

			public IEnumerable<Node> DeepChildren
			{
				get
				{
					foreach (var c in Children)
						foreach (var d in c.DeepChildren)
							yield return d;
				}
			}

			public virtual bool HasKey(string aKey) => false;

			public virtual Node GetValueOrDefault(string aKey, Node aDefault) => aDefault;

			public override string ToString()
			{
				var sb = new StringBuilder();
				WriteToStringBuilder(sb, 0, 0, TextMode.Compact);
				return sb.ToString();
			}

			public virtual string ToString(int aIndent)
			{
				var sb = new StringBuilder();
				WriteToStringBuilder(sb, 0, aIndent, TextMode.Indent);
				return sb.ToString();
			}

			public abstract void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, TextMode aMode);

			public abstract Enumerator GetEnumerator();

			public IEnumerable<KeyValuePair<string, Node>> Linq => new LinqEnumerator(this);
			public KeyEnumerator Keys => new KeyEnumerator(GetEnumerator());
			public ValueEnumerator Values => new ValueEnumerator(GetEnumerator());
			#endregion common interface

			#region typecasting properties

			public virtual double AsDouble
			{
				get => double.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double val)? val: 0.0D;
				set => Value = value.ToString(CultureInfo.InvariantCulture);
			}

			public virtual int AsInt
			{
				get => (int)AsDouble;
				set => AsDouble = value;
			}

			public virtual float AsFloat
			{
				get => (float)AsDouble;
				set => AsDouble = value;
			}

			public virtual bool AsBool
			{
				get => (bool.TryParse(Value, out bool val))? val: !string.IsNullOrEmpty(Value);
				set => Value = (value)? "true": "false";
			}

			public virtual long AsLong
			{
				get => long.TryParse(Value, out long val)? val: 0L;
				set => Value = value.ToString();
			}

			public virtual Array AsArray => this as Array;
			public virtual Object AsObject => this as Object;
			#endregion typecasting properties

			#region operators

			public static implicit operator Node(string s) => new String(s);
			public static implicit operator string(Node d) => d?.Value;

			public static implicit operator Node(double n) => new Number(n);
			public static implicit operator double(Node d) => (d == null)? 0D: d.AsDouble;

			public static implicit operator Node(float n) => new Number(n);
			public static implicit operator float(Node d) => (d == null)? 0F: d.AsFloat;

			public static implicit operator Node(int n) => new Number(n);
			public static implicit operator int(Node d) => (d == null)? 0: d.AsInt;

			public static implicit operator Node(long n) => longAsString? new String(n.ToString()): (Node)new Number(n);
			public static implicit operator long(Node d) => (d == null)? 0L: d.AsLong;

			public static implicit operator Node(bool b) => new Bool(b);
			public static implicit operator bool(Node d) => (d == null)? false: d.AsBool;

			public static implicit operator Node(KeyValuePair<string, Node> aKeyValue) => aKeyValue.Value;

			public static bool operator ==(Node a, object b)
			{
				if (ReferenceEquals(a, b))
					return true;

				bool aIsNull = a is Null || a is null || a is LazyCreator;
				bool bIsNull = b is Null || b is null || b is LazyCreator;

				return (aIsNull && bIsNull)? true: !aIsNull && a.Equals(b);
			}

			public static bool operator !=(Node a, object b) => !(a == b);

			public override bool Equals(object obj) => ReferenceEquals(this, obj);
			public override int GetHashCode() => base.GetHashCode();
			#endregion operators

			[ThreadStatic]
			static StringBuilder mEscapeBuilder;
			public static StringBuilder EscapeBuilder => mEscapeBuilder ??= new StringBuilder();

			public static string Escape(string aText)
			{
				var sb = EscapeBuilder;
				sb.Length = 0;
				sb.Capacity = Math.Max(sb.Capacity, aText.Length + aText.Length / 10);

				foreach (char c in aText)
				{
					switch (c)
					{
						case '\\': sb.Append("\\\\"); break;
						case '\"': sb.Append("\\\""); break;
						case '\n': sb.Append("\\n");  break;
						case '\r': sb.Append("\\r");  break;
						case '\t': sb.Append("\\t");  break;
						case '\b': sb.Append("\\b");  break;
						case '\f': sb.Append("\\f");  break;
						default:
							if (c < ' ' || (forceASCII && c > 127))
								sb.Append("\\u").Append(((ushort)c).ToString("X4"));
							else
								sb.Append(c);
							break;
					}
				}
				string result = sb.ToString();
				sb.Length = 0;
				return result;
			}

			static Node ParseElement(string token, bool quoted)
			{
				if (quoted)
					return token;

				string tmp = token.ToLower();
				if (tmp == "false" || tmp == "true")
					return tmp == "true";
				if (tmp == "null")
					return Null.CreateOrGet();

				if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
					return val;
				else
					return token;
			}

			public static Node Parse(string aJSON)
			{
				Stack<Node> stack = new Stack<Node>();
				Node ctx = null;
				int i = 0;
				StringBuilder Token = new StringBuilder();
				string TokenName = "";
				bool QuoteMode = false;
				bool TokenIsQuoted = false;

				while (i < aJSON.Length)
				{
					switch (aJSON[i])
					{
						case '{':
							if (QuoteMode)
							{
								Token.Append(aJSON[i]);
								break;
							}
							stack.Push(new Object());
							if (ctx != null)
								ctx.Add(TokenName, stack.Peek());

							TokenName = "";
							Token.Length = 0;
							ctx = stack.Peek();
							break;

						case '[':
							if (QuoteMode)
							{
								Token.Append(aJSON[i]);
								break;
							}

							stack.Push(new Array());
							if (ctx != null)
								ctx.Add(TokenName, stack.Peek());

							TokenName = "";
							Token.Length = 0;
							ctx = stack.Peek();
							break;

						case '}':
						case ']':
							if (QuoteMode)
							{

								Token.Append(aJSON[i]);
								break;
							}
							if (stack.Count == 0)
								throw new Exception("JSON Parse: Too many closing brackets");

							stack.Pop();
							if (Token.Length > 0 || TokenIsQuoted)
								ctx.Add(TokenName, ParseElement(Token.ToString(), TokenIsQuoted));

							TokenIsQuoted = false;
							TokenName = "";
							Token.Length = 0;
							if (stack.Count > 0)
								ctx = stack.Peek();
							break;

						case ':':
							if (QuoteMode)
							{
								Token.Append(aJSON[i]);
								break;
							}
							TokenName = Token.ToString();
							Token.Length = 0;
							TokenIsQuoted = false;
							break;

						case '"':
							QuoteMode ^= true;
							TokenIsQuoted |= QuoteMode;
							break;

						case ',':
							if (QuoteMode)
							{
								Token.Append(aJSON[i]);
								break;
							}
							if (Token.Length > 0 || TokenIsQuoted)
								ctx.Add(TokenName, ParseElement(Token.ToString(), TokenIsQuoted));

							TokenName = "";
							Token.Length = 0;
							TokenIsQuoted = false;
							break;

						case '\r':
						case '\n':
							break;

						case ' ':
						case '\t':
							if (QuoteMode)
								Token.Append(aJSON[i]);
							break;

						case '\\':
							++i;
							if (QuoteMode)
							{
								char C = aJSON[i];
								switch (C)
								{
									case 't': Token.Append('\t'); break;
									case 'r': Token.Append('\r'); break;
									case 'n': Token.Append('\n'); break;
									case 'b': Token.Append('\b'); break;
									case 'f': Token.Append('\f'); break;
									case 'u':
										string s = aJSON.Substring(i + 1, 4);
										Token.Append((char)int.Parse(s, NumberStyles.AllowHexSpecifier));
										i += 4;
										break;
									default: Token.Append(C); break;
								}
							}
							break;
						case '/':
							if (allowLineComments && !QuoteMode && i + 1 < aJSON.Length && aJSON[i + 1] == '/')
							{
								while (++i < aJSON.Length && aJSON[i] != '\n' && aJSON[i] != '\r');
								break;
							}
							Token.Append(aJSON[i]);
							break;
						case '\uFEFF': // remove / ignore BOM (Byte Order Mark)
							break;

						default:
							Token.Append(aJSON[i]);
							break;
					}
					++i;
				}
				if (QuoteMode)
					throw new Exception("JSON Parse: Quotation marks seems to be messed up.");

				return ctx ?? ParseElement(Token.ToString(), TokenIsQuoted);
			}
		} // End of Node

		public class Array: Node
		{
			readonly List<Node> mList = new List<Node>();

			public override bool Inline
			{
				get => inline;
				set => inline = value;
			}
			bool inline = false;

			public override NodeType Tag => NodeType.Array;
			public override bool IsArray => true;
			public override Enumerator GetEnumerator() => new Enumerator(mList.GetEnumerator());

			public override Node this[int aIndex]
			{
				get => (aIndex < 0 || aIndex >= mList.Count)? new LazyCreator(this): mList[aIndex];
				set
				{
					value ??= Null.CreateOrGet();

					if (aIndex < 0 || aIndex >= mList.Count)
						mList.Add(value);
					else
						mList[aIndex] = value;
				}
			}

			public override Node this[string aKey]
			{
				get => new LazyCreator(this);
				set => mList.Add(value ?? Null.CreateOrGet());
			}

			public override int Count => mList.Count;

			public override void Add(string _, Node aItem) => mList.Add(aItem ?? Null.CreateOrGet());

			public override Node Remove(int aIndex)
			{
				if (aIndex < 0 || aIndex >= mList.Count)
					return null;

				Node tmp = mList[aIndex];
				mList.RemoveAt(aIndex);
				return tmp;
			}

			public override Node Remove(Node aNode)
			{
				mList.Remove(aNode);
				return aNode;
			}

			public override Node Clone()
			{
				var node = new Array();
				node.mList.Capacity = mList.Capacity;
				mList.ForEach(n => node.Add(n?.Clone()));

				return node;
			}

			public override IEnumerable<Node> Children
			{
				get
				{
					foreach (Node n in mList)
						yield return n;
				}
			}

			public override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, TextMode aMode)
			{
				aSB.Append('[');
				int count = mList.Count;
				if (inline)
					aMode = TextMode.Compact;

				for (int i = 0; i < count; i++)
				{
					if (i > 0)
						aSB.Append(',');
					if (aMode == TextMode.Indent)
						aSB.AppendLine();

					if (aMode == TextMode.Indent)
						aSB.Append(' ', aIndent + aIndentInc);
					mList[i].WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
				}

				if (aMode == TextMode.Indent)
					aSB.AppendLine().Append(' ', aIndent);
				aSB.Append(']');
			}
		} // End of Array

		public class Object: Node
		{
			readonly Dictionary<string, Node> mDict = new Dictionary<string, Node>();

			public override bool Inline
			{
				get => inline;
				set => inline = value;
			}
			bool inline = false;

			public override NodeType Tag => NodeType.Object;
			public override bool IsObject => true;

			public override Enumerator GetEnumerator() => new Enumerator(mDict.GetEnumerator());

			public override Node this[string aKey]
			{
				get => mDict.TryGetValue(aKey, out Node node)? node: new LazyCreator(this, aKey);
				set => mDict[aKey] = value ?? Null.CreateOrGet();
			}

			public override Node this[int aIndex]
			{
				get => (aIndex < 0 || aIndex >= mDict.Count)? null: mDict.ElementAt(aIndex).Value;
				set
				{
					if (aIndex >= 0 && aIndex < mDict.Count)
						mDict[mDict.ElementAt(aIndex).Key] = value ?? Null.CreateOrGet();
				}
			}

			public override int Count => mDict.Count;

			public override void Add(string aKey, Node aItem) =>
				mDict[aKey ?? Guid.NewGuid().ToString()] = aItem ?? Null.CreateOrGet();

			public override Node Remove(string aKey)
			{
				if (!mDict.TryGetValue(aKey, out Node tmp))
					return null;

				mDict.Remove(aKey);
				return tmp;
			}

			public override Node Remove(int aIndex)
			{
				if (aIndex < 0 || aIndex >= mDict.Count)
					return null;

				var item = mDict.ElementAt(aIndex);
				mDict.Remove(item.Key);
				return item.Value;
			}

			public override Node Remove(Node aNode)
			{
				try
				{
					var item = mDict.Where(k => k.Value == aNode).First();
					mDict.Remove(item.Key);
					return aNode;
				}
				catch { return null; }
			}

			public override Node Clone()
			{
				var node = new Object();
				mDict.forEach(n => node.Add(n.Key, n.Value.Clone()));

				return node;
			}

			public override bool HasKey(string aKey) => mDict.ContainsKey(aKey);

			public override Node GetValueOrDefault(string aKey, Node aDefault) =>
				mDict.TryGetValue(aKey, out Node res)? res: aDefault;

			public override IEnumerable<Node> Children
			{
				get
				{
					foreach (var n in mDict)
						yield return n.Value;
				}
			}

			public override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, TextMode aMode)
			{
				aSB.Append('{');
				bool first = true;
				if (inline)
					aMode = TextMode.Compact;

				foreach (var k in mDict)
				{
					if (!first)
						aSB.Append(',');
					first = false;
					if (aMode == TextMode.Indent)
						aSB.AppendLine();
					if (aMode == TextMode.Indent)
						aSB.Append(' ', aIndent + aIndentInc);
					aSB.Append('\"').Append(Escape(k.Key)).Append('\"');
					if (aMode == TextMode.Compact)
						aSB.Append(':');
					else
						aSB.Append(" : ");
					k.Value.WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
				}

				if (aMode == TextMode.Indent)
					aSB.AppendLine().Append(' ', aIndent);
				aSB.Append('}');
			}
		} // End of Object

		class String: Node
		{
			string mData;

			public override NodeType Tag => NodeType.String;
			public override bool IsString => true;

			public override Enumerator GetEnumerator() => new Enumerator();

			public override string Value
			{
				get => mData;
				set => mData = value;
			}

			public String(string aData) => mData = aData;

			public override Node Clone() => new String(mData);

			public override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, TextMode aMode) =>
				aSB.Append('\"').Append(Escape(mData)).Append('\"');

			public override bool Equals(object obj)
			{
				if (base.Equals(obj))
					return true;
				if (obj is string s)
					return mData == s;
				if (obj is String s2)
					return mData == s2.mData;
				return false;
			}

			public override int GetHashCode() => mData.GetHashCode();
		} // End of String

		class Number: Node
		{
			double mData;

			public override NodeType Tag => NodeType.Number;
			public override bool IsNumber => true;
			public override Enumerator GetEnumerator() => new Enumerator();

			public override string Value
			{
				get => mData.ToString(CultureInfo.InvariantCulture);
				set
				{
					if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
						mData = val;
				}
			}

			public override double AsDouble
			{
				get => mData;
				set => mData = value;
			}
			public override long AsLong
			{
				get => (long)mData;
				set => mData = value;
			}

			public Number(double aData) => mData = aData;
			public Number(string aData) => Value = aData;

			public override Node Clone() => new Number(mData);

			public override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, TextMode aMode) =>
				aSB.Append(Value);

			static bool IsNumeric(object value) =>
				value is int || value is uint || value is float || value is double || value is decimal ||
				value is long || value is ulong || value is short || value is ushort || value is sbyte || value is byte;

			public override bool Equals(object obj)
			{
				if (obj == null)
					return false;
				if (base.Equals(obj))
					return true;

				if (obj is Number s2)
					return mData == s2.mData;

				if (IsNumeric(obj))
					return Convert.ToDouble(obj) == mData;

				return false;
			}

			public override int GetHashCode() => mData.GetHashCode();
		} // End of Number

		class Bool: Node
		{
			bool mData;

			public override NodeType Tag => NodeType.Boolean;
			public override bool IsBoolean => true;
			public override Enumerator GetEnumerator() =>new Enumerator();

			public override string Value
			{
				get => mData.ToString();
				set
				{
					if (bool.TryParse(value, out bool val))
						mData = val;
				}
			}

			public override bool AsBool
			{
				get => mData;
				set => mData = value;
			}

			public Bool(bool aData)   => mData = aData;
			public Bool(string aData) => Value = aData;

			public override Node Clone() => new Bool(mData);

			public override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, TextMode aMode) =>
				aSB.Append(mData? "true": "false");

			public override bool Equals(object obj) => (obj == null)? false: (obj is bool? mData == (bool)obj: false);

			public override int GetHashCode() => mData.GetHashCode();
		} // End of Bool

		class Null: Node
		{
			static readonly Null mStaticInstance = new Null();
			public static bool reuseSameInstance = true;

			Null() {}
			public static Null CreateOrGet() => reuseSameInstance? mStaticInstance: new Null();

			public override NodeType Tag => NodeType.NullValue;
			public override bool IsNull => true;
			public override Enumerator GetEnumerator() => new Enumerator();

			public override string Value => "null";
			public override bool AsBool => false;

			public override Node Clone() => CreateOrGet();

			public override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, TextMode aMode) =>
				aSB.Append("null");

			public override bool Equals(object obj) => ReferenceEquals(this, obj)? true: obj is Null;

			public override int GetHashCode() => 0;
		} // End of Null

		class LazyCreator: Node
		{
			Node mNode = null;
			readonly string mKey = null;

			public override NodeType Tag => NodeType.None;
			public override Enumerator GetEnumerator() => new Enumerator();

			public LazyCreator(Node aNode)
			{
				mNode = aNode;
				mKey = null;
			}

			public LazyCreator(Node aNode, string aKey)
			{
				mNode = aNode;
				mKey = aKey;
			}

			T Set<T>(T aVal) where T: Node
			{
				if (mKey == null)
					mNode.Add(aVal);
				else
					mNode.Add(mKey, aVal);

				mNode = null; // Be GC friendly.
				return aVal;
			}

			public override Node this[int aIndex]
			{
				get => new LazyCreator(this);
				set => Set(new Array()).Add(value);
			}

			public override Node this[string aKey]
			{
				get => new LazyCreator(this, aKey);
				set => Set(new Object()).Add(aKey, value);
			}

			public override void Add(Node aItem) => Set(new Array()).Add(aItem);
			public override void Add(string aKey, Node aItem) => Set(new Object()).Add(aKey, aItem);

			public static bool operator ==(LazyCreator a, object b) => b == null? true: ReferenceEquals(a, b);
			public static bool operator !=(LazyCreator a, object b) => !(a == b);

			public override bool Equals(object obj) => obj == null? true: ReferenceEquals(this, obj);

			public override int GetHashCode() => 0;

			public override int AsInt
			{
				get { Set(new Number(0)); return 0; }
				set { Set(new Number(value)); }
			}

			public override float AsFloat
			{
				get { Set(new Number(0.0f)); return 0.0f; }
				set { Set(new Number(value)); }
			}

			public override double AsDouble
			{
				get { Set(new Number(0.0)); return 0.0; }
				set { Set(new Number(value)); }
			}

			public override long AsLong
			{
				get
				{
					Set(longAsString? new String("0"): new Number(0.0) as Node);
					return 0L;
				}
				set
				{
					Set(longAsString? new String(value.ToString()): new Number(value) as Node);
				}
			}

			public override bool AsBool
			{
				get { Set(new Bool(false)); return false; }
				set { Set(new Bool(value)); }
			}

			public override Array  AsArray	=> Set(new Array());
			public override Object AsObject => Set(new Object());

			public override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, TextMode aMode) =>
				aSB.Append("null");
		} // End of LazyCreator
	}
}