// ParallelNode.cs
// by: pbartosch_sa <>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Inkspot.Behaviors;
using Inkspot.Scheduling;
using Inkspot.Utility;

namespace Inkspot.Behaviors.Nodes
{
	/// <summary>
	/// A <see cref="BehaviorNode"/> for running its children in parallel.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This will add each child node to a list that is passed to a new <see cref="AIBranchInstruction"/>.  After this, it will wait until all children are done.  If ANY child returns Failure, this will also return Failure via <paramref name="returnCode"/>.
	/// </para>
	/// <para>
	/// Sample XML:
	/// <code>
	/// <node id="[int"]>
	///  <children>
	///   <child_id>[int]</child_id>
	///   <child_id>[int]</child_id>
	///   <child_id>[int]</child_id>
	///  </children>
	/// </node>
	/// </code>
	/// </para>
	/// </remarks>
	public class ParallelNode : BehaviorNode
	{
		public override IEnumerator Run(Blackboard blackboard, ReturnCodeWrapper returnCode)
		{
			Logger.LogMessage("ParallelNode("+Id+"): Starting to run "+Children.Count+" children in parallel.", Logger.Verbosity.Low);
			ReturnCodeWrapper[] retCodes = new ReturnCodeWrapper[Children.Count];
			Stack running = new Stack();
			List<KeyValuePair<BehaviorNode,ReturnCodeWrapper>> nodeInfo = new List<KeyValuePair<BehaviorNode, ReturnCodeWrapper>>();
			for(int i=0;i<Children.Count;++i)
			{
				retCodes[i] = new ReturnCodeWrapper(ReturnCode.Running);
				Logger.LogMessage("ParallelNode("+Id+"): Creating instruction information for child "+Children[i].GetType().Name+"("+Children[i].Id+").", Logger.Verbosity.High);
				nodeInfo.Add(new KeyValuePair<BehaviorNode, ReturnCodeWrapper>(Children[i], retCodes[i]));
				running.Push(retCodes[i]);
			}
			Logger.LogMessage("ParallelNode("+Id+"): Adding instruction for "+Children.Count+" children.", Logger.Verbosity.Low);
			yield return new AIBranchInstruction(nodeInfo, blackboard);

			//we have to wait for everything to finish
			//so this takes care of that
			//the stack is not necessarily the best data structure for this,
			//but it works.  If items lower in the stack finish earlier, then
			//it doesn't matter because the top isn't done.  If the top item
			//finishes first then it is popped and we keep waiting.
			//this is based on the idea that I'm passing the reference of the
			//ReturnCodeWrapper above, which may prove to be wrong.  It is entirely
			//possible that I am copying the value, which makes this unusable.
			bool haveFail = false;
			while(running.Count > 0)
			{
				ReturnCodeWrapper rc = (ReturnCodeWrapper)running.Peek();
				if(rc.Value != ReturnCode.Running)
				{
					Logger.LogMessage("ParallelNode("+Id+"): Child done.  Checking to see if it failed.", Logger.Verbosity.High);
					haveFail = (!haveFail) ? (rc.Value == ReturnCode.Failure) : true;
					running.Pop();
				}
				Logger.LogMessage("ParallelNode("+Id+"): Waiting for "+running.Count+" children to finish.", Logger.Verbosity.Low);
				yield return new ContinueInstruction();
			}

			returnCode.Value = (haveFail) ? ReturnCode.Failure : ReturnCode.Success;
			resetChildren();
			Executed = true;
		}
	}
}