// AIBranchInstruction.cs
// by: pbartosch_sa <>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Inkspot.Behaviors;
using Inkspot.Behaviors.Nodes;
using Inkspot.Utility;

namespace Inkspot.Scheduling
{
	/// <summary>
	/// AI branch instruction for creating parallel tasks.
	/// </summary>
	public class AIBranchInstruction : IInstruction
	{
		//private BranchInstruction _branchInst;
		private ICollection<KeyValuePair<BehaviorNode,ReturnCodeWrapper>> _nodeInfo;
		private Blackboard _blackboard;

		/// <summary>
		/// Initializes a new instance of the <see cref="Inkspot.Scheduling.AIBranchInstruction"/> class.
		/// </summary>
		/// <param name="nodeInfo">A collection containing pairs of <see cref="BehaviorNode"/>s and their associated <see cref="ReturnCodeWrapper"/>s</param>
		/// <param name="blackboard">The <see cref="Blackboard"/> to pass to each node.</param>
		public AIBranchInstruction(ICollection<KeyValuePair<BehaviorNode,ReturnCodeWrapper>> nodeInfo, Blackboard blackboard)
		{
			_nodeInfo = nodeInfo;
			_blackboard = blackboard;
		}

		/// <summary>
		/// Execute the specified task in the scheduler.
		/// </summary>
		/// <param name="scheduler">The scheduler that the instruction uses.</param>
		/// <param name="task">The task to use once the scheduler reaches it.</param>
		/// <remarks>
		/// This use <c>ThreadPool</c> to queue each node for running.  This way
		/// we don't have to actually manage the threads ourselves.
		/// </remarks>
		public bool Execute(SchedulerBase scheduler, IEnumerator task)
		{
			foreach(KeyValuePair<BehaviorNode, ReturnCodeWrapper> kvp in _nodeInfo)
			{
				ThreadPool.QueueUserWorkItem(delegate(object state) {
					Logger.LogMessage("AIBranchInstruction: Scheduling " + kvp.Key.GetType().Name + "("+kvp.Key.Id+") in a new thread.", Logger.Verbosity.High);
					scheduler.ScheduleTask(kvp.Key.Run(_blackboard, kvp.Value));
				});
				Logger.LogMessage("AIBranchInstruction: Scheduled " + kvp.Key.GetType().Name + "("+kvp.Key.Id+") in a new thread.", Logger.Verbosity.High);
			}
			Logger.LogMessage("AIBranchInstruction: Scheduled "+_nodeInfo.Count+" nodes in new threads.", Logger.Verbosity.High);
			return true;
		}
	}
}
