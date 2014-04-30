// NodeComponent.cs
// by: Peter Bartosch <bartoschp@gmail.com>
//
// Defines the NodeComponent class for pathfinding.
//
// TODO
// * Consider changing the Id to GUID.  This would make unique ids easier, I think.
// * Write a custom inspector.
//
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Inkspot.Pathfinding
{
	[ExecuteInEditMode]
	/// <summary>
	/// The Node component for pathfinding.  Since this inherits from <see cref="UnityEngine.MonoBehaviour"/> it
	/// must be attached to a <see cref="UnityEngine.GameObject"/> in the scene.
	/// </summary>
	public class NodeComponent : MonoBehaviour, IComparable<NodeComponent>, IComparable
	{
		private static int _nextId = 1;
		
		[SerializeField]
		private Color _color = Color.white;

		[SerializeField]
		private List<NodeComponent> _neighbors = new List<NodeComponent>();

		[SerializeField]
		private int _id = _nextId++;

		#region IComparable implementation
		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <returns>-1 if less than, 0 if equal to, 1 if greater than.</returns>
		/// <param name="other">The other <see cref="NodeComponent"/>.</param>
		public int CompareTo(NodeComponent other)
		{
			return _id.CompareTo(other.Id);
		}
		#endregion

		#region IComparable implementation
		/// <summary>
		/// Compares the current object with another object.
		/// </summary>
		/// <returns>-1 if less than, 0 if equal to, 1 if greater than.</returns>
		/// <param name="obj">The other <see cref="System.Object"/>.</param>
		int IComparable.CompareTo(System.Object obj)
		{
			if(obj == null) return 1;
			
			NodeComponent other = obj as NodeComponent;
			if(other != null) return this.CompareTo(other);
			else throw new ArgumentException("Trying to compare a non-NodeComponent to a NodeComponent");
		}
		#endregion

		/// <summary>
		/// The cost to the adjacent node, using Euclidean distance.
		/// </summary>
		/// <returns>The absolute value of the Euclidean distance to the neighbor if it is found in the adjacency list.
		/// If the passed neighbor id is not found, then -1 is returned.  I'm not sure yet if it should be -1 or the max
		/// float value.
		/// </returns>
		/// <param name="id">The id of the neighbor to check.</param>
		public float CostToNeighbor(int id)
		{
			//just a simple linear search for the neighbor
			foreach(NodeComponent neighbor in _neighbors)
				if(neighbor.Id == id)
					return Mathf.Abs((neighbor.transform.position - transform.position).magnitude);

			return -1f; //not sure if it should be this or max float
		}

		/// <summary>
		/// The cost to the adjacent node, using Euclidean distance.
		/// </summary>
		/// <returns>The absolute value of the Euclidean distance to the neighbor if it is found in the adjacency list.
		/// If the passed neighbor id is not found, then -1 is returned.  I'm not sure yet if it should be -1 or the max
		/// float value.
		/// </returns>
		/// <param name="neighbor">A <see cref="NodeComponent"/> representing the neighbor to check.</param>
		public float CostToNeighbor(NodeComponent neighbor)
		{
			return CostToNeighbor(neighbor.Id);
		}

		/// <summary>
		/// Gets or sets the display color.  This is used for the Unity Editor scene view.
		/// </summary>
		/// <value>The display color for the node.</value>
		public Color DisplayColor
		{
			get { return _color; }
			set { _color = value; }
		}

		/// <summary>
		/// Gets or sets the neighbors.
		/// </summary>
		/// <value>The neighbors.</value>
		public List<NodeComponent> Neighbors
		{
			get { return _neighbors; }
			set { _neighbors = value; }
		}

		/// <summary>
		/// Gets or sets the node identifier.
		/// </summary>
		/// <value>The node identifier.</value>
		public int Id
		{
			get { return _id; }
			set
			{
				_id = value;

				//just to make sure auto-assignment of ids does not
				//overlap, push _nextId forward.
				if(_id > _nextId)
					_nextId = _id+1;
			}
		}
	}
}