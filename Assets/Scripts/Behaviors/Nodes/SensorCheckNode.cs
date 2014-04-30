// SensorCheckNode.cs
// by: Peter Bartosch <bartoschp@gmail.com>
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Inkspot.Behaviors;
using Inkspot.Utility;

namespace Inkspot.Behaviors.Nodes
{
	/// <summary>
	/// This node will check a single <see cref="Sensor"/> or all that have the same <see cref="Senses"/> value.  It will give a value of <c>ReturnCode.Success</c> if something is detected.
	/// </summary>
	/// <remarks>
	/// Sample XML:
	/// <list>
	/// <item>
	/// To check a single sensor:
	/// <code>
	/// <node id=[int]>
	///  <node_type>SensorCheckNode</node_type>
	///  <parameter>
	///   <name>sensorName</name>
	///   <value>[string]</value>
	///  </parameter>
	/// </node>
	/// </code>
	/// </item>
	/// <item>
	/// To check all sensors of a given sense:
	/// <code>
	/// <node id=[int]>
	///  <node_type>SensorCheckNode</node_type>
	///  <parameter>
	///   <name>checkAll</name>
	///   <value>true</value>
	///  </parameter>
	///  <parameter>
	///   <name>sense</name>
	///   <value>[string from Senses enum]</value>
	///  </parameter>
	/// </node>
	/// </code>
	/// </item>
	/// </list>
	/// </remarks>
	public class SensorCheckNode : BehaviorNode
	{
		#region fields
		private string _sensorName = null;
		private bool _checkAll = false;
		private Senses _sense = Senses.Sight;
		private bool _invert = false;
		#endregion

		#region overrides
		public override bool Init(int id, List<ParameterInfo> parameters)
		{
			if(!base.Init(id, parameters)) return false;

			bool ret = true;
			foreach(ParameterInfo pi in parameters)
			{
				switch(pi.name)
				{
				case "sensorName":
					_sensorName = pi.value;
					break;
				case "checkAll":
					bool resb1;
					if(!bool.TryParse(pi.value, out resb1))
						ret=false;
					else
						_checkAll = resb1;
					break;
				case "sense":
					try
					{
						_sense = (Senses)(Enum.Parse(typeof(Senses), pi.value, true));
					}
					catch
					{
						//not yet certain if I should rethrow or just return false in this case.
						
						//we only care about this failing if we are checking all
						//since we assume that all sensors are not being checked from the start
						//and leave it to the user to tell us otherwise, this will only
						//throw if the user as taken an action.
						ret = false;
						if(_checkAll) throw;
						
						//if(_checkAll) return false;
					}
					break;
				case "invertResult":
					bool resb2;
					if(!bool.TryParse(pi.value, out resb2))
						ret = false;
					else
						_invert = resb2;
					break;
				default:
					ret = false;
					break;
				}

				if(!ret) break;
			}

			return ret;
		}

		public override IEnumerator Run(Blackboard blackboard, ReturnCodeWrapper returnCode)
		{
			if(_checkAll)
			{
				Logger.LogMessage("SensorCheckNode(" + Id + "): Start checking all sensors for sense "+_sense, Logger.Verbosity.Low);
				bool success = false;
				LinkedList<Sensor> all = Sensor.AllSensors[_sense];
				if(all != null)
				{
					Logger.LogMessage("SensorCheckNode(" + Id + "): Found some sensors, now checking.", Logger.Verbosity.Low);
					foreach(Sensor sensor in all)
					{
						if(sensor.Check() && !success) success = true;
						returnCode.Value = ReturnCode.Running;
						yield return new Inkspot.Scheduling.ContinueInstruction();
					}

					if(!success && !_invert)
						Logger.LogMessage("SensorCheckNode(" + Id + "): Nothing detected; return Failure.", Logger.Verbosity.Low);
					if(!success && _invert)
					{
						success = true;
						Logger.LogMessage("SensorCheckNode("+ Id + "): Nothing detected and inverted; return Success", Logger.Verbosity.Low);
					}
				}
				else
				{
					Logger.LogMessage("SensorCheckNode(" + Id + "): Found no sensors; return Failure.", Logger.Verbosity.Low);
					success = false;
				}

				if(success) returnCode.Value = ReturnCode.Success;
				else returnCode.Value = ReturnCode.Failure;
			}
			else
			{
				Logger.LogMessage("SensorCheckNode(" + Id + "): Start for sensor " + _sensorName, Logger.Verbosity.Low);
				//I think it works better to find the sensor every frame because this accounts
				//for checking a sensor that may enter/leave the scene at unpredictable (to the node)
				//times
				Sensor sensor = Sensor.FindSensor(_sensorName);

				if(sensor != null)
				{
					bool res = sensor.Check();
					if(!res && !_invert) returnCode.Value = ReturnCode.Failure;
					if(res || _invert) returnCode.Value = ReturnCode.Success;
				}
				else
				{
					returnCode.Value = ReturnCode.Failure;
				}
			}
			Executed = true;
			Logger.LogMessage("SensorCheckNode(" + Id + "): Done.", Logger.Verbosity.Low);
		}
		#endregion
	}
}
	