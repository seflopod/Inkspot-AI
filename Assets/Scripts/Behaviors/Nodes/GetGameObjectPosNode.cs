using UnityEngine;
using System.Collections;
using System;
using Inkspot.Behaviors;
using Inkspot.Utility;

namespace Inkspot.Behaviors.Nodes
{
	/// <summary>
	/// This is used to get the position of a given <c>GameObject</c>.
	/// </summary>
	/// <remarks>
	/// Sample XML:
	/// <code>
	/// <node id=[int]>
	///  <node_type>GetGameObjectPosNode</node_type>
	///  <parameter>
	///   <name>varName</name>
	///   <value>[string]</value>
	///  </parameter>
	///  <parameter>
	///   <name>gameObjectName</name>
	///   <value>[string]</value>
	///  </parameter>
	/// </node>
	/// </code>
	/// </remarks>
	public class GetGameObjectPosNode : BehaviorNode
	{
		private string _gameObjName;
		private string _varName;
		
		public override bool Init(int id, System.Collections.Generic.List<ParameterInfo> parameters)
		{
			//if the base init, bail
			if(!base.Init(id, parameters)) return false;
			
			bool ret = true;
			foreach(ParameterInfo pi in parameters)
			{
				switch(pi.name)
				{
				case "gameObjectName":
					_gameObjName = pi.value;
					break;
				case "varName":
					_varName = pi.value;
					break;
				default:
					ret = false;
					break;
				}
				
				//if something isn't right, bail
				if(!ret) break;
			}

			return ret;
		}

		/// <summary>
		/// Using <see cref="_gameObjName"/>, search the scene for a <c>GameObject</c>.
		/// </summary>
		/// <param name="blackboard">Blackboard.</param>
		/// <param name="returnCode">Return code.</param>
		/// <remarks>
		/// This will access the position of the found <c>GameObject</c> and store it in on the <paramref name="blackboard"/> specified as <see cref="_varName"/>.
		/// </remarks>
		public override IEnumerator Run(Blackboard blackboard, ReturnCodeWrapper returnCode)
		{
			Logger.LogMessage("GetGameObjectPosNode("+Id+"): Starting.", @"logger.log", Logger.Verbosity.Low);
			GameObject go;
			if(!getGameObject(out go))
			{
				Logger.LogMessage("GetGameObjectPosNode("+Id+"): Failed to find GameObject named "+_gameObjName+" in scene; returning Failure.", @"logger.log", Logger.Verbosity.Low);
				returnCode.Value = ReturnCode.Failure;
				yield break;
			}
			Logger.LogMessage("GetGameObjectPosNode("+Id+"): Found GameObject named "+_gameObjName+" in scene and added its position of "+go.transform.position+" to blackboard as "+_varName+"; returning Success.", @"logger.log", Logger.Verbosity.Low);
			returnCode.Value = ReturnCode.Success;
			blackboard[_varName] = go.transform.position;
			Executed = true;
		}

		/// <summary>
		/// Gets the game object.
		/// </summary>
		/// <returns><c>true</c>, if game object was gotten, <c>false</c> otherwise.</returns>
		/// <param name="go">GameObject output variable.</param>
		private bool getGameObject(out GameObject go)
		{
			go = GameObject.Find(_gameObjName);
			return go != null;
		}
	}
}