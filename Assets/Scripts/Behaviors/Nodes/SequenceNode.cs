// SequenceNode.cs
// by: Peter Bartosch <bartoschp@gmail.com>
//
using UnityEngine;
using System.Collections;
using Inkspot.Behaviors;
using Inkspot.Scheduling;
using Inkspot.Utility;

namespace Inkspot.Behaviors.Nodes
{
	/* xml sample
	 * 
	 * <node id=[int]>
	 *  <node_type>SequenceNode</node_type>
	 *  <children>
	 *   <child_id>[int]</child_id>
	 *   0..* child_id tags with the node id as a value
	 *  </children>
	 * </node>
	 * 
	 */
	/// <summary>
	/// The sequence node runs each of its children unless and until one returns
	/// <c>ReturnCode.Failure</c>.
	/// </summary>
	/// <remarks>
	/// Sample XML:
	/// <code>
	/// <node id=[int]>
	///  <node_type>SequenceNode</node_type>
	///  <children>
	///   <child_id>[int]</child_id>
	///   0..* child_id tags with the node id as a value
	///  </children>
	/// </node>
	/// </code>
	/// </remarks>
	public class SequenceNode : BehaviorNode
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
			int childIdx = 0;
			returnCode.Value = ReturnCode.Running;
			ReturnCodeWrapper retCode = new ReturnCodeWrapper(ReturnCode.Running);
			while(childIdx < Children.Count && retCode.Value != ReturnCode.Failure)
			{
				BehaviorNode child = Children[childIdx];
				Logger.LogMessage("SequenceNode("+Id+"): Adding instruction for child "+child.GetType().Name+"("+child.Id+")"+" ("+(childIdx+1)+"/"+Children.Count+")", @"logger.log", Logger.Verbosity.Low);
				yield return new AIInstruction(child, blackboard, retCode);
			
				//while(retCode.Value == ReturnCode.Running)
				Logger.LogMessage("SequenceNode("+Id+"): Before ContinueInstruction loop for child "+child.GetType().Name+"("+child.Id+")"+" ("+(childIdx+1)+"/"+Children.Count+")", @"logger.log", Logger.Verbosity.Low);
				while(!child.Executed)
					yield return new ContinueInstruction();
				Logger.LogMessage("SequenceNode("+Id+"): After ContinueInstruction loop for child "+child.GetType().Name+"("+child.Id+")"+" ("+(childIdx+1)+"/"+Children.Count+")", @"logger.log", Logger.Verbosity.Low);

				if(retCode.Value == ReturnCode.Success)
					Logger.LogMessage("SequenceNode("+Id+"): Child "+child.GetType().Name+"("+child.Id+")"+" successful, continuing.", @"logger.log", Logger.Verbosity.Low);
				else
					Logger.LogMessage("SequenceNode("+Id+"): Child "+child.GetType().Name+"("+child.Id+")"+" failure.  Finishing.", @"logger.log", Logger.Verbosity.Low);
				childIdx++;
			}
			returnCode.Value = retCode.Value;
			resetChildren();
			Executed = true;
			Logger.LogMessage("SequenceNode("+Id+"): Finished.", @"logger.log", Logger.Verbosity.Low);
		}
	}
}
