// RootNode.cs
// by: Peter Bartosch <bartoschp@gmail.com>
//
// The root node of a behavior tree.
//
using UnityEngine;
using System.Collections;
using Inkspot.Behaviors;
using Inkspot.Scheduling;

namespace Inkspot.Behaviors.Nodes
{
	/// <summary>
	/// The root node is the top node of any behavior tree.  It runs every one
	/// of its children, regardless of their success.
	/// </summary>
	/// <remarks>
	/// Sample XML:
	/// <code>
	/// <node id=[int]>
	///  <node_type>RootNode</node_type>
	///  <children>
	///   <child_id>[int]</child_id>
	///	  0..* child_id tags with the node id as a value
	///	 </children>
	/// </node>
	/// </code>
	/// </remarks>
	public class RootNode : BehaviorNode
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
			Inkspot.Utility.Logger.LogMessage("RootNode("+Id+"): Starting to parse children.", @"logger.log", System.IO.FileMode.Append, true, Inkspot.Utility.Logger.Verbosity.High);
			int childIdx = 0;
			while(childIdx < Children.Count)
			{
				Inkspot.Utility.Logger.LogMessage("RootNode("+Id+"): Adding instruction for child "+Children[childIdx].GetType().Name+"("+Children[childIdx].Id+")"+" ("+(childIdx+1)+"/"+Children.Count+")", @"logger.log", System.IO.FileMode.Append, true, Inkspot.Utility.Logger.Verbosity.High);
				yield return new AIInstruction(Children[childIdx], blackboard, returnCode);

				while(!Children[childIdx].Executed)
					yield return new ContinueInstruction();

				childIdx++;
			}
			Inkspot.Utility.Logger.LogMessage("RootNode("+Id+"): Finished parsing children.", @"logger.log", System.IO.FileMode.Append, true, Inkspot.Utility.Logger.Verbosity.High);
			resetChildren();
			Executed = true;
		}
	}
}
