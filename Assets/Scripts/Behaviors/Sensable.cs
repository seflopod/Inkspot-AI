using UnityEngine;
using System;

namespace Inkspot.Behaviors
{
	[Serializable]
	/// <summary>
	/// This attaches to an object in the scene to indicate that it can be sensed by a <see cref="Sensor"/> with the specified <see cref="Senses"/> value.
	/// </summary>
	public class Sensable : MonoBehaviour, IEquatable<Sensable>
	{
		[SerializeField]
		private readonly Guid _guid = Guid.NewGuid();
		public Senses _sense = Senses.Sight;

		#region monobehaviour
		protected void OnEnable()
		{
			Sensor.RegisterSensable(this);
		}

		protected void OnDisable()
		{
			Sensor.DeregsiterSensable(this);
		}
		#endregion

		#region IEquatable implementation
		public bool Equals(Sensable other)
		{
			if(other == null) return false;

			return _guid == other.GUID;
		}
		#endregion

		#region overrides
		public override bool Equals(System.Object obj)
		{
			return Equals((Sensable)obj);
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
		/// <description>
		/// Hash algorithm from http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
		/// I use GUID for the hash because it shouldn't change during execution (when this would be in a collection that depends on the hash).
		/// </description>
		public override int GetHashCode()
		{
			unchecked
			{
				int ret = 31;
				ret *= (43 + _guid.GetHashCode());
				return ret;
			}
		}
		#endregion

		public Guid GUID { get { return _guid; } }
		public Senses Sense { get { return _sense; }  }
	}
}