// ReturnCodeWrapper.cs
// by: Peter Bartosch <bartoschp@gmail.com>
//
namespace Inkspot.Behaviors
{
	/// <summary>
	/// A return code for a <see cref="Inkspot.Behaviors.Nodes.BehaviorNode"/>.
	/// </summary>
	public enum ReturnCode
	{
		Success,
		Failure,
		Running
	}

	/// <summary>
	/// A class used to wrap <see cref="Inkspot.Behaviors.ReturnCode"/>s so that
	/// I can pass them as reference types.
	/// </summary>
	public class ReturnCodeWrapper
	{
		private ReturnCode _code;

		/// <summary>
		/// Initializes a new instance of the <see cref="Inkspot.Behaviors.ReturnCodeWrapper"/> class.
		/// </summary>
		/// <param name="code">The <see cref="ReturnCode"/> to wrap.</param>
		public ReturnCodeWrapper(ReturnCode code)
		{
			_code = code;
		}

		/// <summary>
		/// Gets or sets the value of the return code.
		/// </summary>
		/// <value>The value.</value>
		public ReturnCode Value
		{
			get { return _code; }
			set { _code = value; }
		}
	}
}
