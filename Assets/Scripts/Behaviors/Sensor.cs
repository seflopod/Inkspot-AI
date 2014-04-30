using UnityEngine;
using System;
using System.Collections.Generic;

namespace Inkspot.Behaviors
{
	/// <summary>
	/// Senses that can be used by <see cref="Sensor"/>s and <see cref="Sensable"/>s.
	/// </summary>
	public enum Senses
	{
		Sight,
		Hearing,
		Smell,
		Tactile,
		Taste,
		Sixth
	}

	[Serializable]
	/// <summary>
	/// A component that can find <see cref="Sensable"/>s with a similar <see cref="Senses"/> type.
	/// </summary>
	public class Sensor : MonoBehaviour, IEquatable<Sensor>
	{
		#region statics
		//Since most of the writing to the value of the dictionaries will likely
		//not involve the front or the back, use a LinkedList for a slight speed
		//gain.  See http://www.dotnetperls.com/linkedlist
		private static Dictionary<Senses, LinkedList<Sensor>> _allSensors = new Dictionary<Senses, LinkedList<Sensor>>();
		private static Dictionary<Senses, LinkedList<Sensable>> _allSensables = null;

		/// <summary>
		/// Registers a <see cref="Sensable"/> to avoid repeated calls to <c>GameObject.Find</c>.
		/// </summary>
		/// <param name="sensable">Sensable.</param>
		public static void RegisterSensable(Sensable sensable)
		{
			if(_allSensables == null)
				_allSensables = new Dictionary<Senses, LinkedList<Sensable>>();

			if(!_allSensables.ContainsKey(sensable.Sense))
				_allSensables.Add(sensable.Sense, (new LinkedList<Sensable>()));

			_allSensables[sensable.Sense].AddLast(sensable);
		}

		/// <summary>
		/// Deregsiters the <see cref="Sensable"/> so that it is no longer checked.
		/// </summary>
		/// <returns><c>true</c>, if <see cref="Sensable"/> was deregsitered, <c>false</c> otherwise.</returns>
		/// <param name="sensable">Object to remove.</param>
		public static bool DeregsiterSensable(Sensable sensable)
		{
			if(_allSensables == null || !_allSensables.ContainsKey(sensable.Sense))
				return false;
			return _allSensables[sensable.Sense].Remove(sensable);
		}

		/// <summary>
		/// Finds a sensor.
		/// </summary>
		/// <returns>The sensor if it is found;<c>null</c> otherwise.</returns>
		/// <param name="name">The name to search.</param>
		public static Sensor FindSensor(string name)
		{
			foreach(KeyValuePair<Senses, LinkedList<Sensor>> kvp in _allSensors)
			{
				foreach(Sensor sensor in kvp.Value)
				{
					if(sensor.Name.Equals(name))
						return sensor;
				}
			}
			return null;
		}

		/// <summary>
		/// Finds a sensor.
		/// </summary>
		/// <returns>The sensor if it is found;<c>null</c> otherwise.</returns>
		/// <param name="guid">The GUID to search.</param>
		public static Sensor FindSensor(Guid guid)
		{
			foreach(KeyValuePair<Senses, LinkedList<Sensor>> kvp in _allSensors)
			{
				foreach(Sensor sensor in kvp.Value)
				{
					if(sensor.GUID == guid)
						return sensor;
				}
			}

			return null;
		}

		/// <summary>
		/// Gives access to the dictionary containing sensors.
		/// </summary>
		/// <value>A dictionary of all sensors.</value>
		public static Dictionary<Senses, LinkedList<Sensor>> AllSensors
		{
			get { return _allSensors; }
			protected set { _allSensors = value; }
		}
		#endregion

		///<summary>
		/// The possible shapes for checking a sense.
		/// </summary>
		public enum Shapes
		{
			Box,
			Sphere
		}

		#region serialized fields
		[SerializeField]
		private Senses _sense = Senses.Sight;

		[SerializeField]
		private Shapes _shape = Shapes.Box;

		[SerializeField]
		private Vector3 _size = Vector3.one;

		[SerializeField]
		private float _radius = 1f;

		[SerializeField]
		private string _name = "";
		#endregion

		#region other fields
		private Bounds _boxBounds;
		private readonly Guid _guid = Guid.NewGuid();
		private List<Sensable> _lastSensed = null;
		#endregion

		#region monobehaviour
		protected void Awake()
		{
			if(AllSensors.Count == 0)
			{
				//AllSensors = new Dictionary<Senses, LinkedList<Sensor>>();
				foreach(var sense in Enum.GetValues(typeof(Senses)))
				{
					AllSensors[(Senses)sense] = new LinkedList<Sensor>();
				}
			}
		}

		protected void Start()
		{
			_boxBounds = new Bounds(transform.position, _size);
		}

		protected void OnDisable()
		{
			AllSensors[_sense].Remove(this);
		}

		protected void OnEnable()
		{
			if(!AllSensors.ContainsKey(_sense))
				AllSensors.Add(_sense, (new LinkedList<Sensor>()));
			
			AllSensors[_sense].AddLast(this);
		}
		#endregion

		#region IEquatable implementation
		/// <summary>
		/// Determines whether the specified <see cref="Inkspot.Behaviors.Sensor"/> is equal to the current <see cref="Inkspot.Behaviors.Sensor"/>.
		/// </summary>
		/// <param name="other">The <see cref="Inkspot.Behaviors.Sensor"/> to compare with the current <see cref="Inkspot.Behaviors.Sensor"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="Inkspot.Behaviors.Sensor"/> is equal to the current
		/// <see cref="Inkspot.Behaviors.Sensor"/>; otherwise, <c>false</c>.</returns>
		public bool Equals(Sensor other)
		{
			if(other == null) return false;
			
			return _guid == other.GUID;
		}
		#endregion
		
		#region overrides
		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Inkspot.Behaviors.Sensor"/>.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Inkspot.Behaviors.Sensor"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="Inkspot.Behaviors.Sensor"/>; otherwise, <c>false</c>.</returns>
		public override bool Equals(System.Object obj)
		{
			return Equals((Sensor)obj);
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
		/// <description>
		/// Hash algorithm from http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
		/// I changed the prime numbers for my implementation because I don't feel comfortable with all hashes being done the same.  I use GUID for the hash because it shouldn't change during execution (when this would be in a collection that depends on the hash).
		/// </description>
		public override int GetHashCode()
		{
			unchecked
			{
				int ret = 29;
				ret *= (13 + _guid.GetHashCode());
				return ret;
			}
		}
		#endregion

		#region sensing
		/// <summary>
		/// Check to see if there is at least one <see cref="Sensable"/> object withing range of this <see cref="Sensor"/>.  
		/// </summary>
		/// <returns>
		/// <c>true</c> if at least one <see cref="Sensable"/> is found; <c>false</c> otherwise.
		/// </returns>
		/// <description>
		/// As <see cref="Sensable"/>s are found they are added to <see cref="Sensor.LastSense"/>.  The list will be <c>null</c> if no objects are found in the bounding volume of this <see cref="Sensor"/>.
		/// </description>
		public bool Check()
		{
			_lastSensed = null;
			_boxBounds.size = _size;
			_boxBounds.center = transform.position;

			LinkedList<Sensable> toCheck = _allSensables[_sense];
			foreach(Sensable s in toCheck)
			{
				switch(_shape)
				{
				case Shapes.Box:
					checkBox(s);
					break;
				case Shapes.Sphere:
					checkSphere(s);
					break;
				default:
					Debug.LogError("WTF?  Fail on Shape in Sensor.Check()");
					break;
				}
			}

			return !(_lastSensed == null);
		}

		/// <summary>
		/// Checks the bounding box defined by the position of this <see cref="Sensor"/> and its <see cref="Sensor.Size"/> for a given <see cref="Sensable"/>.
		/// </summary>
		/// <param name="sensable">The <see cref="Sensable"/> to check.</param>
		private void checkBox(Sensable sensable)
		{
			if(_boxBounds.Contains(sensable.gameObject.transform.position))
			{
				if(_lastSensed == null) _lastSensed = new List<Sensable>();
				_lastSensed.Add(sensable);
			}
		}

		/// <summary>
		/// Checks to see if a <see cref="Sensable"/> is within the bounding sphere defined by <see cref="Sensor.Radius"/>.
		/// </summary>
		/// <param name="sensable">The <see cref="Sensable"/> to check.</param>
		private void checkSphere(Sensable sensable)
		{
			if((sensable.gameObject.transform.position - transform.position).sqrMagnitude <= _radius*_radius)
			{
				if(_lastSensed == null) _lastSensed = new List<Sensable>();
				_lastSensed.Add(sensable);
			}
		}
		#endregion

		#region accessor properties
		/// <summary>
		/// Gets or sets this <see cref="Sensor"/>'s bounding volume shape.
		/// </summary>
		/// <value>The <see cref="Sensor.Shapes"/> value of the <see cref="Sensor"/>'s bounding volume.</value>
		public Shapes Shape
		{
			get { return _shape; }
			set { _shape = value; }
		}

		/// <summary>
		/// Gets or sets the width (x) of the bounding box.
		/// </summary>
		/// <value>The width of the bounding box.</value>
		public float Width
		{
			get { return _size.x; }
			set { _size.x = value; }
		}

		/// <summary>
		/// Gets or sets the height (y) of the bounding box.
		/// </summary>
		/// <value>The height of the bounding box.</value>
		public float Height
		{
			get { return _size.y; }
			set { _size.y = value; }
		}

		/// <summary>
		/// Gets or sets the depth (z) of the bounding box.
		/// </summary>
		/// <value>The depth of the bounding box.</value>
		public float Depth
		{
			get { return _size.z; }
			set { _size.z = value; }
		}

		/// <summary>
		/// Gets or sets the size of this <see cref="Sensor"/>'s bounding box.
		/// </summary>
		/// <value>The size of the bounding box.</value>
		public Vector3 Size
		{
			get { return _size; }
			set { _size = value; }
		}

		/// <summary>
		/// Gets or sets the radius for <see cref="Sensable"/> checks.
		/// </summary>
		/// <value>The radius of the bounding sphere.</value>
		public float Radius
		{
			get { return _radius; }
			set { _radius = value; }
		}

		/// <summary>
		/// Gets the GUID of this <see cref="Sensor"/>.
		/// </summary>
		/// <value>The GUID of this <see cref="Sensor"/>.</value>
		public Guid GUID { get { return _guid; } }

		/// <summary>
		/// Gets or sets the name of this <see cref="Sensor"/>.
		/// </summary>
		/// <value>The name of this <see cref="Sensor"/>.</value>
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// Gets the last sensed game objects.
		/// </summary>
		/// <value>A list of all <see cref="Sensable"/>s that were sensed in the last <see cref="Sensor.Check"/>.</value>
		public List<Sensable> LastSensed { get { return _lastSensed; } }
		#endregion
	}
}