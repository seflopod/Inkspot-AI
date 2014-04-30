// UnsetValueNode.cs
// by: Peter Bartosch <bartoschp@gmail.com>
using UnityEngine;
using System.Collections;
using Inkspot.Behaviors;
using Inkspot.Utility;

namespace Inkspot.Behaviors.Nodes
{
	/// <summary>
	/// Remove a variable from a <see cref="Blackboard"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This will always set its returnCode to Success.  If the variable does not
	/// exist on the <see cref="Blackboard"/> then it is simply ignored.
	/// </para>
	/// <para>
	/// Sample XML:
	/// <code>
	/// <node id="[int]">
	///  <node_type>UnsetValueNode</node_type>
	///  <parameter>
	///   <name>varName</name>
	///   <value>[string]</value>
	///  </parameter>
	/// </node>
	/// </code>
	/// </para>
	/// </remarks>
	public class UnsetValueNode : BehaviorNode
	{
		private string varName;

		public override bool Init(int id, System.Collections.Generic.List<ParameterInfo> parameters)
		{
			if(!base.Init(id, parameters)) return false;

			bool ret = true;
			foreach(ParameterInfo pi in parameters)
			{
				switch(pi.name)
				{
				case "varName":
					varName = pi.value;
					break;
				default:
					ret = false;
					break;
				}
				if(!ret) break;
			}

			return ret;
		}

		/// <summary>
		/// Run the specified blackboard and returnCode.
		/// </summary>
		/// <param name="blackboard">Blackboard.</param>
		/// <param name="returnCode">Return code.</param>
		/// <remarks>
		/// <para>
		/// This will always set its returnCode to Success.  If the variable does not
		/// exist on the <see cref="Blackboard"/> then it is simply ignored.
		/// </para>
		/// </remarks>
		public override IEnumerator Run(Blackboard blackboard, ReturnCodeWrapper returnCode)
		{
			Logger.LogMessage("UnsetValueNode("+Id+"): Removing variable named "+varName+" from blackboard.", Logger.Verbosity.High);
			blackboard.Remove(varName);
			Logger.LogMessage("UnsetValueNode("+Id+"): Removed variable named "+varName+" from blackboard; return Success.", Logger.Verbosity.Low);
			returnCode.Value = ReturnCode.Success;
			Executed = true;
			yield return null;
		}
	}
}