using UnityEngine;
using System.Collections;

namespace Inkspot.Scheduling
{
	public class ContinueInstruction : IInstruction
	{
		#region IInstruction implementation
		/// <summary>
		/// A blank instruction to tell the scheduler to keep on truckin'.
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
			return true;
		}
		#endregion
	}
}