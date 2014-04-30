// SetValueNode.cs
// by: Peter Bartosch <bartoschp@gmail.com>
//
// A set variable action for the behavior tree
using UnityEngine;
using System.Collections;
using System;
using Inkspot.Behaviors;
using Inkspot.Utility;

namespace Inkspot.Behaviors.Nodes
{
	/// <summary>
	/// Sets a value on a <see cref="Blackboard"/>.
	/// </summary>
	/// <remarks>
	/// Sample XML:
	/// <list>
	/// <item>
	/// To set a variable as a bool, string, int, or float
	/// <code>
	/// <node id=[int]>
	///  <node_type>SetValueNode</node_type>
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
	/// </node>
	/// </code>
	/// </item>
	/// <item>
	/// To set a variable on the blackboard as a Vector3:
	/// <code>
	/// <node id=[int]>
	///  <node_type>SetValueNode</node_type>
	///  <parameter>
	///   <name>varName</name>
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
	/// </node>
	/// </code>
	/// </item>
	/// <item>
	/// To set a variable on the blackboard as a Vector2:
	/// <code>
	/// <node id=[int]>
	///  <node_type>SetValueNode</node_type>
	///  <parameter>
	///   <name>varName</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>valueType</name>
	///   <value>Vector2</value>
	///  </parameter>
	///  <parameter>
	///   <name>value</name>
	///   <value>[float],[float]</value>
	///  </parameter>
	/// </node>
	/// </code>
	/// </item>
	/// </list>
	/// </remarks>
	public class SetValueNode : BehaviorNode
	{
		private string _varName = "";
		private string _valueType = "";
		private string _valueStr = "";
		private System.Object _value;

		public override bool Init(int id, System.Collections.Generic.List<ParameterInfo> parameters)
		{
			//if we can't even init the BehaviorNode, just bail
			if(!base.Init(id, parameters)) return false;

			bool ret = true;
			foreach(ParameterInfo pi in parameters)
			{
				switch(pi.name)
				{
				case "varName":
					_varName = pi.value;
					break;
				case "valueType":
					_valueType = pi.value;
					break;
				case "value":
					_valueStr = pi.value;
					break;
				default:
					//unknown parameter, throw error?
					ret = false;
					break;
				}
				//bail if something isn't right
				if(!ret) break;
			}

			return ret;
		}
		
		public override IEnumerator Run(Blackboard blackboard, ReturnCodeWrapper returnCode)
		{
			Logger.LogMessage("SetValueNode("+Id+"): Starting SetValueNode.", @"logger.log", Logger.Verbosity.Low);
			if(setValue(blackboard))
			{
				Logger.LogMessage("SetValueNode("+Id+"): Set "+_varName+"="+_valueStr+" as " + _valueType+" in blackboard okay, return Success", @"logger.log", Logger.Verbosity.Low);
				returnCode.Value = ReturnCode.Success;
			}
			else
			{
				Logger.LogMessage("SetValueNode("+Id+"): Failed to set "+_varName+"="+_valueStr+" as " + _valueType+" in blackboard, return Failure", @"logger.log", Logger.Verbosity.Low);
				returnCode.Value = ReturnCode.Failure;
			}
			
			Executed = true;
			yield return null;
		}
		
		private bool setValue(Blackboard blackboard)
		{
			bool ret = true;
			switch(_valueType)
			{
			case "int":
				int resi;
				if(!int.TryParse(_valueStr, out resi)) return false;
				Logger.LogMessage("SetValueNode("+Id+"): Able to parse "+_valueStr+" as int.", @"logger.log", Logger.Verbosity.Low);
				_value = resi;
				break;
			case "string":
				_value = _valueStr;
				Logger.LogMessage("SetValueNode("+Id+"): Able to parse "+_valueStr+" as string.", @"logger.log", Logger.Verbosity.Low);
				break;
			case "float":
				float resf;
				if(!float.TryParse(_valueStr, out resf)) return false;
				Logger.LogMessage("SetValueNode("+Id+"): Able to parse "+_valueStr+" as float.", @"logger.log", Logger.Verbosity.Low);
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
				Logger.LogMessage("SetValueNode("+Id+"): Able to parse "+_valueStr+" as Vector3.", @"logger.log", Logger.Verbosity.Low);
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
				Logger.LogMessage("SetValueNode("+Id+"): Able to parse "+_valueStr+" as Vector2.", @"logger.log", Logger.Verbosity.Low);
				break;
			case "bool":
				bool resb;
				if(!bool.TryParse(_valueStr, out resb)) return false;
				Logger.LogMessage("SetValueNode("+Id+"): Able to parse "+_valueStr+" as bool.", @"logger.log", Logger.Verbosity.Low);
				_value = resb;
				break;
			default:
				//unknown type, throw error?
				ret = false;
				break;
			}
			
			if(ret) blackboard[_varName] = _value;
			return ret;
		}
	}
}
