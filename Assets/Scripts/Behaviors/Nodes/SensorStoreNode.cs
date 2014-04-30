// SensorStoreNode.cs
// by: pbartosch_sa <>
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Inkspot.Behaviors;
using Inkspot.Utility;

namespace Inkspot.Behaviors.Nodes
{
	/// <summary>
	/// Store the result of a check on <see cref="Sensor"/>s rather than just give <c>Success</c> or <c>Failure</c>.
	/// </summary>
	/// /// <remarks>
	/// <list>
	/// <item>
	/// To store for a single sensor:
	/// <code>
	/// <node id=[int]>
	///  <node_type>SensorStoreNode</node_type>
	///  <parameter>
	///   <name>sensorName</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>varName</name>
	///   <value>[string]</value>
	///  </parameter>
	/// </node>
	/// </code>
	/// </item>
	/// <item>
	/// <code>
	/// <node id=[int]>
	///  <node_type>SensorStoreNode</node_type>
	///  <parameter>
	///   <name>checkAll</name>
	///   <value>true</value>
	///  </parameter>
	///  <parameter>
	///   <name>sense</name>
	///   <value>[string from Senses enum]</value>
	///  </parameter>
	///  <parameter>
	///   <name>varName</name>
	///   <value>[string]</value>
	///  </parameter>
	/// </node>
	/// </code>
	/// </item>
	/// </list>
	/// </remarks>
	public class SensorStoreNode : BehaviorNode
	{
		#region fields
		private string _sensorName = null;
		private bool _storeAll = false;
		private Senses _sense = Senses.Sight;
		private string _varName = "lastSensed_";
		#endregion
		
		#region overrides
		public override bool Init(int id, List<ParameterInfo> parameters)
		{
			if(!base.Init(id, parameters)) return false;
			
			foreach(ParameterInfo pi in parameters)
			{
				if(pi.name.Equals("sensorName")) _sensorName = pi.value;
				
				if(pi.name.Equals("checkAll"))
				{
					bool res;
					if(!bool.TryParse(pi.value, out res))
						return false;
					
					_storeAll = res;
				}
				
				if(pi.name.Equals("sense"))
				{
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
						if(_storeAll) throw;
						
						//if(_storeAll) return false;
					}
				}

				if(pi.name.Equals("varName")) _varName = pi.value;
			}
			return true;
		}
		
		public override IEnumerator Run(Blackboard blackboard, ReturnCodeWrapper returnCode)
		{
			if(_storeAll)
			{
				Logger.LogMessage("SensorStoreNode("+Id+"): Starting for all sensors of type "+_sense+".", @"logger.log", Logger.Verbosity.Low);
				List<Sensable> res = new List<Sensable>();
				foreach(Sensor sensor in Sensor.AllSensors[_sense])
				{
					Logger.LogMessage("SensorStoreNode("+Id+"): Storing "+sensor.LastSensed.Count+" sensed objects from "+sensor.gameObject.name+".", @"logger.log", Logger.Verbosity.Low);
					res.AddRange(sensor.LastSensed);
					returnCode.Value = ReturnCode.Running;
					yield return new Inkspot.Scheduling.ContinueInstruction();
				}
				blackboard[_varName] = res;
				returnCode.Value = ReturnCode.Success;
			}
			else
			{
				//I think it works better to find the sensor every frame because this accounts
				//for checking a sensor that may enter/leave the scene at unpredictable (to the node)
				//times
				Sensor sensor = Sensor.FindSensor(_sensorName);
				Logger.LogMessage("SensorStoreNode("+Id+"): Storing "+sensor.LastSensed.Count+" sensed objects from "+sensor.gameObject.name+".", @"logger.log", Logger.Verbosity.Low);
				blackboard[_varName] = sensor.LastSensed;
				returnCode.Value = ReturnCode.Success;
			}
			Executed = true;
			Logger.LogMessage("SensorStoreNode("+Id+"): Finished with "+returnCode.Value.ToString()+".", @"logger.log", Logger.Verbosity.Low);
		}
		#endregion
	}
}
