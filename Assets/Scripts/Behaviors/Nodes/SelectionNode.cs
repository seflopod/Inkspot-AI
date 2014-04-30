// SelectionNode.cs
// by: Peter Bartosch <bartoschp@gmail.com>
//
using UnityEngine;
using System.Collections;
using Inkspot.Behaviors;
using Inkspot.Scheduling;
using Inkspot.Utility;

namespace Inkspot.Behaviors.Nodes
{
	/// <summary>
	/// A selection node runs each of its children until one returns
	/// <c>ReturnCode.Success</c> or it runs out of children.
	/// </summary>
	/// <remarks>
	/// Sample XML:
	/// <code>
	/// <node id=[int]>
	///  <node_type>SelectionNode</node_type>
	///  <children>
	///   <child_id>[int]</child_id>
	///   0..* child_id tags with the node id as a value
	///  </children>
	/// </node>
	/// </code>
	/// </remarks>
	public class SelectionNode : BehaviorNode
	{
		/// <summary>
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
			Logger.LogMessage("SelectionNode("+Id+"): Starting.", @"logger.log", Logger.Verbosity.Low);
			int childIdx = 0;
			returnCode.Value = ReturnCode.Running;
			ReturnCodeWrapper retCode = new ReturnCodeWrapper(ReturnCode.Running);
			while(childIdx < Children.Count && retCode.Value != ReturnCode.Success)
			{
				Logger.LogMessage("SelectionNode("+Id+"): Adding instruction for child "+Children[childIdx].GetType().Name+"("+Children[childIdx].Id+")"+" ("+(childIdx+1)+"/"+Children.Count+")", @"logger.log", Logger.Verbosity.Low);

				yield return new AIInstruction(Children[childIdx], blackboard, retCode);

				Logger.LogMessage("SelectionNode("+Id+"): Before ContinueInstruction loop for child "+Children[childIdx].GetType().Name+"("+Children[childIdx].Id+")"+" ("+(childIdx+1)+"/"+Children.Count+")", @"logger.log", Logger.Verbosity.Low);
				while(!Children[childIdx].Executed)
					yield return new ContinueInstruction();
				Logger.LogMessage("SelectionNode("+Id+"): After ContinueInstruction loop for child "+Children[childIdx].GetType().Name+"("+Children[childIdx].Id+")"+" ("+(childIdx+1)+"/"+Children.Count+")", @"logger.log", Logger.Verbosity.Low);

				if(retCode.Value == ReturnCode.Success)
					Logger.LogMessage("SelectionNode("+Id+"): Child "+Children[childIdx].GetType().Name+"("+Children[childIdx].Id+")"+" successful.  Finishing.", @"logger.log", Logger.Verbosity.Low);
				else
					Logger.LogMessage("SelectionNode("+Id+"): Child "+Children[childIdx].GetType().Name+"("+Children[childIdx].Id+")"+" failure, moving to next.", @"logger.log", Logger.Verbosity.Low);
				childIdx++;
			}
			returnCode.Value = retCode.Value;
			Logger.LogMessage("SelectionNode("+Id+"): Finished.", @"logger.log", Logger.Verbosity.Low);
			resetChildren();
			Executed = true;
		}
	}
}