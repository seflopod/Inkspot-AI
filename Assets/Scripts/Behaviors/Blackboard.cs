// Blackboard.cs
// by: Peter Bartosch <bartoschp@gmail.com>
//
using UnityEngine;
using System.Collections.Generic;

namespace Inkspot.Behaviors
{
	/// <summary>
	/// The blackboard here is used as a wrapper to the .NET <c>Dictionary</c>
	/// object for storing data.  Data is stored with <c>string</c> keys and
	/// <see cref="System.Object"/>s for values.
	/// </summary>
	public class Blackboard
	{
		private Dictionary<string, System.Object> _values = new Dictionary<string, System.Object>();

		/// <summary>
		/// Add the specified name and value.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="value">Value.</param>
		public void Add(string name, System.Object value)
		{
			try
			{
				_values.Add(name, value);
			}
			catch
			{
				throw;
			}
		}

		/// <summary>
		/// Remove the specified name.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <returns>
		/// <c>true</c> if the removal works, <c>false</c> otherwise.
		/// </returns>
		public bool Remove(string name)
		{
			if(_values.ContainsKey(name))
				return _values.Remove(name);
			return false;
		}

		/// <summary>
		/// Checks to see if the specified variable is in the <c>Dictionary</c>
		/// </summary>
		/// <param name="name">Name.</param>
		/// <returns><c>true</c> if the variable is found; <c>false</c> otherwise.</returns>
		public bool Contains(string name)
		{
			return _values.ContainsKey(name);
		}

		/// <summary>
		/// Gets or sets the
		/// <see cref="Inkspot.Behaviors.System.Object"/> using the specified
		/// name as the key.
		/// </summary>
		/// <param name="name">Name.</param>
		public System.Object this[string name]
		{
			get
			{
				if(_values.ContainsKey(name))
					return _values[name];
				else
					return null;
			}

			set { _values[name] = value; }
		}
	}
}
