using UnityEngine;
using System.Collections;

namespace Inkspot.Scheduling
{
	/// <summary>
	/// A sleep instruction does as its name implies, it puts the thread to
	/// sleep for a little while.
	/// </summary>
	public class SleepInstruction : IInstruction
	{
		private float _seconds;

		public SleepInstruction(float seconds)
		{
			_seconds = seconds;
		}

		#region IInstruction implementation
		/// <summary>
		/// Sleep for a certain amount of time, and then schedule the task.
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
			scheduler.StartCoroutine(sleep(scheduler, task));
			return false;
		}
		#endregion

		/// <summary>
		/// Sleep for a time and then schedule the task.
		/// </summary>
		/// <param name="scheduler">
		/// The scheduler that the instruction uses.
		/// </param>
		/// <param name="task">
		/// The task to use once the scheduler reaches it.
		/// </param>
		private IEnumerator sleep(SchedulerBase scheduler, IEnumerator task)
		{
			yield return new WaitForSeconds(_seconds);
			scheduler.ScheduleTask(task);
		}
	}
}
