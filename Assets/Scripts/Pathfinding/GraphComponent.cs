// GraphComponent.cs
// by: Peter Bartosch <bartoschp@gmail.com>
//
// Defines the Graph used in pathfinding as a Component
//
// TODO
// * Determine if Name has any real value
// * Finish writing the custom inspector
//
using UnityEngine;
using System;
using System.Collections.Generic;
using Inkspot.Utility;

namespace Inkspot.Pathfinding
{
	[Serializable]
	[ExecuteInEditMode]
	/// <summary>
	/// Defines the component used for a pathfinding graph.
	/// </summary>
	public class GraphComponent : MonoBehaviour
	{
		[SerializeField]
		private List<NodeComponent> _nodes = new List<NodeComponent>();

		[SerializeField]
		private string _name = "";

		[SerializeField]
		private Color _color = Color.white;

		/// <summary>
		/// Add a node to the graph.
		/// </summary>
		/// <param name="node">The <see cref="NodeComponent"/> to add.</param>
		public void AddNode(NodeComponent node)
		{
			if(!_nodes.Contains(node))
			{
				node.DisplayColor = _color;
				_nodes.Add(node);
			}
		}

		/// <summary>
		/// Finds the node at or close to a given position.
		/// </summary>
		/// <returns><c>true</c>, if an exact position match was found, <c>false</c> otherwise.</returns>
		/// <param name="position">The position to findPosition.</param>
		/// <param name="bestNode">The <see cref="NodeComponent"/> for the node closest to <paramref name="position"/>.</param>
		public bool NodeAtPosition(Vector3 position, out NodeComponent bestNode)
		{
			float minDistSq = float.MaxValue;
			NodeComponent ret = null;
			foreach(NodeComponent node in _nodes)
			{
				if(node.transform.position == position)
				{
					bestNode = node;
					return true;
				}
				
				if((node.transform.position - position).sqrMagnitude < minDistSq)
				{
					minDistSq = (node.transform.position - position).sqrMagnitude;
					ret = node;
				}
			}
			bestNode = ret;
			return false;
		}

		/// <summary>
		/// Gets the nodes in the graph.
		/// </summary>
		/// <value>The list of nodes.</value>
		public List<NodeComponent> Nodes { get { return _nodes; } }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
			}
		}

		/// <summary>
		/// Gets or sets the color of the nodes in the graph.
		/// </summary>
		/// <value>The color of the nodes.</value>
		public Color NodeColor
		{
			get { return _color; }
			set
			{
				_color = value;

				//in addition to changing the stored color value,
				//change the display color of every node in the graph
				foreach(NodeComponent node in _nodes)
					node.DisplayColor = _color;
			}
		}
	}
}