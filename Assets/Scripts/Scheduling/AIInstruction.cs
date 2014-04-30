using UnityEngine;
using System.Collections;
using Inkspot.Behaviors;
using Inkspot.Behaviors.Nodes;
using Inkspot.Utility;

namespace Inkspot.Scheduling
{
	/// <summary>
	/// The instruction used for my behavior tree AI.
	/// </summary>
	public class AIInstruction : IInstruction
	{
		BehaviorNode _node;
		Blackboard _blackboard;
		ReturnCodeWrapper _returnCode;
	
		public AIInstruction(BehaviorNode node, Blackboard blackboard, ReturnCodeWrapper returnCode)
		{
			_node = node;
			_blackboard = blackboard;
			_returnCode = returnCode;
		}

		#region IInstruction implementation
		/// <summary>
		/// This will always return true, since it prevents the behavior tree
		/// from bailing early.
		/// <seealso cref="Inkspot.Scheduling.IInstruction"/>
		/// </summary>
		/// <param name="scheduler">
		/// The scheduler that the instruction uses.
		/// </param>
		/// <param name="task">
		/// The task to use once the scheduler reaches it.
		/// </param>
		public bool Execute(SchedulerBase scheduler, IEnumerator task)
		{
			Logger.LogMessage("AIInstruction: Adding "+_node.GetType().Name+"("+_node.Id+") to scheduler", @"logger.log", Inkspot.Utility.Logger.Verbosity.High);
			scheduler.ScheduleTask(_node.Run(_blackboard, _returnCode));
			Logger.LogMessage("AIInstruction: Added "+_node.name+"("+_node.Id+") to scheduler", @"logger.log", Inkspot.Utility.Logger.Verbosity.High);
			return true;
		}
		#endregion
	}
}