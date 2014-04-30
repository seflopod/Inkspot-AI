// ConditionNode.cs
// by: Peter Bartosch <bartoschp@gmail.com>
//
// A comparision action for the behavior tree
//
// TODO
// * Implement the ability to compare "raw" values.
using UnityEngine;
using System.Collections;
using System;
using Inkspot.Behaviors;
using Inkspot.Utility;

namespace Inkspot.Behaviors.Nodes
{
	[System.Flags]
	/// <summary>
	/// Bitflags for the type of comparison to use in a
	/// <see cref="Inkspot.Behaviors.Nodes.ConditionNode"/>.
	/// </summary>
	public enum ComparisonType
	{
		EqualTo = 0x01,
		GreaterThan = 0x02,
		LessThan = 0x04
	}

	/// <summary>
	/// Condition node.  This is a
	/// <see cref="Inkspot.Behaviors.Nodes.BehaviorNode"/> that checks two
	/// values with a <see cref="Inkspot.Behaviors.Nodes.ComparisonType"/>.  If
	/// the check evaluates as true, then <c>ReturnCode.Success</c> is set,
	/// otherwise <c>ReturnCode.Failure</c> is set.  Ideally this would check
	/// objects both on the blackboard and by "raw" value, but for now I have
	/// this limited to blackboard-only variables.
	/// </summary>
	/// <remarks>
	/// Sample XML for usage:
	/// <list type="bullet">
	/// <item>
	/// To compare two variables on the blackboard:
	/// <code>
	/// <node id=[int]>
	///  <node_type>ConditionNode</node_type>
	///  <parameter>
	///   <name>varName1</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>varName2</name>
	///   <value>[string]</value>
	///  </parameter>
	///    * 
	///  <parameter>
	///   <name>comparisonType</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>comparisonType</name>
	///   <value>[string]</value>
	///  </parameter>
	/// </node>
	/// </code>
	/// </item>
	/// <item>
	/// To compare a variable on the blackboard against a raw bool, string, int, or float:
	/// <code>
	/// <node id=[int]>
	///  <node_type>ConditionNode</node_type>
	///  <parameter>
	///   <name>varName1</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>valueType</name>
	///   <value>[bool|string|int|float]</value>
	///  </parameter>
	///  <parameter>
	///   <name>value</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>useValue</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>comparisonType</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>comparisonType</name>
	///   <value>[string]</value>
	///  </parameter>
	/// </node>
	/// </code>
	/// </item>
	/// 
	/// <item>
	/// To compare a variable on the blackboard against a Vector3:
	/// <code>
	/// <node id=[int]>
	///  <node_type>ConditionNode</node_type>
	///  <parameter>
	///   <name>varName1</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>valueType</name>
	///   <value>Vector3</value>
	///  </parameter>
	///  <parameter>
	///   <name>value</name>
	///   <value>[float],[float],[float]</value>
	///  </parameter>
	///  <parameter>
	///   <name>useValue</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>comparisonType</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>comparisonType</name>
	///   <value>[string]</value>
	///  </parameter>
	/// </node>
	/// </code>
	/// </item>
	/// <item>
	/// To compare a variable on the blackboard against a Vector2:
	/// <code>
	/// <node id=[int]>
	///  <node_type>ConditionNode</node_type>
	///  <parameter>
	///   <name>varName1</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>valueType</name>
	///   <value>Vector3</value>
	///  </parameter>
	///  <parameter>
	///   <name>value</name>
	///   <value>[float],[float]</value>
	///  </parameter>
	///  <parameter>
	///   <name>useValue</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>comparisonType</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>comparisonType</name>
	///   <value>[string]</value>
	///  </parameter>
	/// </node>
	/// </code>
	/// </item>
	/// <item>
	/// To compare a variable on the blackboard against a Vector2:
	/// <code>
	/// <node id=[int]>
	///  <node_type>ConditionNode</node_type>
	///  <parameter>
	///   <name>varName1</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>valueType</name>
	///   <value>Vector3</value>
	///  </parameter>
	///  <parameter>
	///   <name>value</name>
	///   <value>[float],[float]</value>
	///  </parameter>
	///  <parameter>
	///   <name>useValue</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>comparisonType</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>comparisonType</name>
	///   <value>[string]</value>
	///  </parameter>
	/// </node>
	/// </code>
	/// </item>
	/// </list>
	/// </remarks>
	public class ConditionNode : BehaviorNode
	{
		private string _varName1 = "";
		private string _varName2 = "";
		private string _valueType = "";
		private string _valueStr = "";
		private System.Object _value;
		private bool _useValue = false;
		private ComparisonType _compareType = ComparisonType.EqualTo;

		public override bool Init(int id, System.Collections.Generic.List<ParameterInfo> parameters)
		{
			//if we can't even init the BehaviorNode, just bail
			if(!base.Init(id, parameters)) return false;

			bool ret = true;
			foreach(ParameterInfo pi in parameters)
			{
				switch(pi.name)
				{
				case "varName1":
					_varName1 = pi.value;
					break;
				case "varName2":
					_varName2 = pi.value;
					break;
				case "useValue":
					bool res;
					if(!bool.TryParse(pi.value, out res)) ret = false;
					else _useValue = res;
					break;
				case "valueType":
					_valueType = pi.value;
					break;
				case "value":
					_valueStr = pi.value;
					break;
				case "comparisonType":
					try
					{
						_compareType |= (ComparisonType)(Enum.Parse(typeof(ComparisonType), pi.value, true));
					}
					catch
					{
						//because _useValue defaults to false and I cannot guarantee the order
						//of parsing, I can't properly check on what should be done.
						//just return false
						ret = false;
					}
					break;
				default:
					//unknown parameter, throw error?
					ret = false;
					break;
				}

				if(!ret) break;
			}

			if(_useValue && ret) return setValue();

			return ret;
		}

		private bool setValue()
		{
			bool ret = true;
			switch(_valueType)
			{
			case "bool":
				bool resb;
				if(!bool.TryParse(_valueStr, out resb)) return false;
				_value = resb;
				break;
			case "int":
				int resi;
				if(!int.TryParse(_valueStr, out resi)) return false;
				_value = resi;
				break;
			case "string":
				_value = _valueStr;
				break;
			case "float":
				float resf;
				if(!float.TryParse(_valueStr, out resf)) return false;
				_value = resf;
				break;
			case "Vector3":
				string[] componentsv3 = _valueStr.Split(new char[]{','});
				if(componentsv3.Length != 3) return false;
				Vector3 vec3 = Vector3.zero;
				for(int i=0;i<3;++i)
				{
					float resv3;
					if(!float.TryParse(componentsv3[i], out resv3)) return false;
					vec3[i] = resv3;
				}
				break;
			case "Vector2":
				string[] componentsv2 = _valueStr.Split(new char[]{','});
				if(componentsv2.Length != 2) return false;
				Vector3 vec2 = Vector3.zero;
				for(int i=0;i<3;++i)
				{
					float resv2;
					if(!float.TryParse(componentsv2[i], out resv2)) return false;
					vec2[i] = resv2;
				}
				break;
			default:
				//unknown type, throw error?
				ret = false;
				break;
			}
			return ret;
		}

		/// <summary>
		/// Checks the two values associated with the node using the configured
		/// <see cref="Inkspot.Behaviors.Nodes.ComparisonType"/>.
		/// <seealso cref="Inkspot.Behaviors.Nodes.BehaviorNode"/>
		/// </summary>
		/// <param name="blackboard">The
		/// <see cref="Inkspot.Behaviors.Blackboard"/> used for data i/o in this
		/// behavior tree.
		/// </param>
		/// <param name="returnCode">The
		/// <see cref="Inkspot.Behaviors.ReturnCodeWrapper"/> for the parent of
		/// this behavior.  This is used to effectively pass
		/// <see cref="Inkspot.Behaviors.ReturnCode"/>s by reference.
		/// </param>
		public override IEnumerator Run(Blackboard blackboard, ReturnCodeWrapper returnCode)
		{
			Logger.LogMessage("ConditionNode("+Id+"): Starting ConditionNode", @"logger.log", Logger.Verbosity.Low);
			if(blackboard[_varName1] == null || (!_useValue && blackboard[_varName2] == null))
			{
				Logger.LogMessage("ConditionNode("+Id+"): Error finding variables from blackboard, return Failure.", @"logger.log", Logger.Verbosity.Low);
				returnCode.Value = ReturnCode.Failure;
				Executed = true;
				yield break;
			}

			System.Object value1 = blackboard[_varName1];
			System.Object value2;
			if(!_useValue) value2 = blackboard[_varName2];
			else value2 = _value;

			//I think I have the basics right, but I'm not sure how to deal with typing
			bool pass = false;
			if((_compareType & ComparisonType.EqualTo) == ComparisonType.EqualTo)
			{
				pass = value1.Equals(value2);
				Logger.LogMessage("ConditionNode("+Id+"): Checking for equality (result:"+pass.ToString()+")", @"logger.log", Logger.Verbosity.Low);
			}
			
			if(value1.GetType().Equals(value2.GetType()) && value1.GetType().GetInterface("IComparable") != null)
			{
				if((_compareType & ComparisonType.GreaterThan) == ComparisonType.GreaterThan)
				{
					pass |= (((IComparable)value1).CompareTo(value2) > 0);
					Logger.LogMessage("ConditionNode("+Id+"): Checking to see if value1 is greater than value2 (result:"+pass.ToString()+")", @"logger.log", Logger.Verbosity.Low);
				}
				if((_compareType & ComparisonType.LessThan) == ComparisonType.LessThan)
				{
					Logger.LogMessage("ConditionNode("+Id+"): Checking to see if value1 is less than value2 (result:"+pass.ToString()+")", @"logger.log", Logger.Verbosity.Low);
					pass |= (((IComparable)value1).CompareTo(value2) < 0);
				}
			}

			if(pass)
			{
				Logger.LogMessage("ConditionNode("+Id+"): TRUE, return Success", @"logger.log", Logger.Verbosity.Low);
				returnCode.Value = ReturnCode.Success;
			}
			else
			{
				Logger.LogMessage("ConditionNode("+Id+"): FALSE, return Failure", @"logger.log", Logger.Verbosity.Low);
				returnCode.Value = ReturnCode.Failure;
			}

			Executed = true;
			yield return null;
		}
	}
}