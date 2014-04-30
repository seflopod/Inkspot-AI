using System.Collections;

namespace Inkspot.Scheduling
{
	/// <summary>
	/// An interface for defining instructions to the scheduler.
	/// </summary>
	public interface IInstruction
	{
		/// <summary>
		/// Execute the specified task in the scheduler.
		/// </summary>
		/// <param name="scheduler">
		/// The scheduler that the instruction uses.
		/// </param>
		/// <param name="task">
		/// The task to use once the scheduler reaches it.
		/// </param>
		bool Execute(SchedulerBase scheduler, IEnumerator task);
	}
}
