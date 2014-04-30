// Pathfinder.cs
// by: pbartosch_sa <>
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Inkspot.Utility;

namespace Inkspot.Pathfinding
{
	/// <summary>
	/// Used to find a path over a graph.
	/// </summary>
	public class Pathfinder
	{
		#region statics
		/// <summary>
		/// The default heurstic for estimating cost in the path finding.  This is simple Manhattan distance.
		/// </summary>
		/// <returns>The Manhattan distance between <paramref name="position"/> and <paramref name="destination"/>.</returns>
		/// <param name="position">The current position.</param>
		/// <param name="destination">The destination position.</param>
		private static float defaultHeuristic(Vector3 position, Vector3 destination)
		{
			float dist = Mathf.Abs(position.x - destination.x) +
				Mathf.Abs(position.y - destination.y) +
					Mathf.Abs(position.z - destination.z);
			return dist;
		}
		#endregion
		
		#region NodeRecord class
		/// <summary>
		/// A private class used to keep track of costs and such
		/// while pathfinding.
		/// </summary>
		private class NodeRecord : IComparable<NodeRecord>, IComparable
		{
			/// <summary>
			/// The node this record tracks.
			/// </summary>
			public NodeComponent node;
			
			/// <summary>
			/// A <see cref="NodeRecord"/> for the preceding node.
			/// </summary>
			public NodeRecord connection;
			
			/// <summary>
			/// The cost so far.
			/// </summary>
			public float currentCost;
			
			/// <summary>
			/// An estimate of the cost remaining to the end.
			/// </summary>
			public float estCost;
			
			/// <summary>
			/// Initializes a new instance of the <see cref="Inkspot.Pathfinding.GraphComponent+NodeRecord"/> class.
			/// </summary>
			/// <param name="node">The node to make a record for.</param>
			/// <param name="curCost">Current cost.</param>
			/// <param name="estCost">Estimated remaining cost.</param>
			public NodeRecord(NodeComponent node, float curCost, float estCost)
			{
				this.node = node;
				currentCost = curCost;
				this.estCost = estCost;
				connection = null;
			}
			
			#region IComparable implementation
			/// <summary>
			/// Compares the current object with another object of the same type.
			/// </summary>
			/// <returns>-1 if less than, 0 if equal to, and 1 if greater than.</returns>
			/// <param name="other">The other <see cref="NodeRecord"/>.</param>
			public int CompareTo (NodeRecord other)
			{
				if(other == null) return 1;
				return this.node.CompareTo(other.node);
			}
			
			#endregion
			
			#region IComparable implementation
			/// <summary>
			/// Compares the current object with another object.
			/// </summary>
			/// <returns>-1 if less than, 0 if equal to, and 1 if greater than.</returns>
			/// <param name="obj">The other <see cref="System.Object"/>.</param>
			int IComparable.CompareTo(System.Object obj)
			{
				if(obj == null) return 1;
				
				NodeRecord other = obj as NodeRecord;
				return this.CompareTo(other);
			}
			#endregion
		}
		#endregion

		///<summary>
		/// A function pointer defining what makes an estimating function for the A* pathfinding
		/// </summary>
		public delegate float Heuristic(Vector3 position, Vector3 destination);

		#region private fields
		//private Vector3 startPos;
		//private Vector3 endPos;
		private List<NodeComponent> _path;
		private Heuristic _h;
		
		private bool _hitsEndNode;
		private bool _done;
		#endregion
		
		#region ctors
		public Pathfinder() : this(new Heuristic(Pathfinder.defaultHeuristic))
		{}
		
		public Pathfinder(Heuristic h)
		{
			_h = h;
			_path = null;
			_hitsEndNode = false;
		}
		#endregion
		
		/// <summary>
		/// A coroutine method for finding a path.
		/// </summary>
		/// <returns>An <c>IEnumerator</c> so it can be used as a coroutine.</returns>
		/// <param name="graph">The graph on which the search will take place.</param>
		/// <param name="startPos">The start position.</param>
		/// <param name="endPos">The end position.</param>
		public IEnumerator FindPath(GraphComponent graph, Vector3 startPos, Vector3 endPos)
		{
			Logger.LogMessage("Pathfinder: Starting to find path.", @"logger.log", Logger.Verbosity.Medium);
			_done = false;
			NodeComponent start, end;
			graph.NodeAtPosition(startPos, out start);
			_hitsEndNode = graph.NodeAtPosition(endPos, out end);

			PriorityQueue<NodeRecord> open = new PriorityQueue<NodeRecord>(graph.Nodes.Count);
			PriorityQueue<NodeRecord> closed = new PriorityQueue<NodeRecord>(graph.Nodes.Count);

			NodeRecord startRec = new NodeRecord(start, 0f, _h(startPos, endPos));
			open.Add(startRec);
			NodeRecord current = null;
			int cnt = 0;
			while(open.Count > 0)
			{
				cnt++;
				current = open.Peek();
				Logger.LogMessage("Pathfinder: Starting with node number " + cnt + " (not an id).", @"logger.log", Logger.Verbosity.High);
				//if the current node is the end goal, then we're done
				//break out
				if(current.node.CompareTo(end) == 0)
				{
					Logger.LogMessage("Pathfinder: Current node is target node.  Done.", @"logger.log", Logger.Verbosity.Medium);
					break;
				}
				
				List<NodeComponent> neighbors = current.node.Neighbors;
				Logger.LogMessage("Pathfinder: Looking through "+neighbors.Count+" neighbors for current node.", @"logger.log", Logger.Verbosity.High);
				foreach(NodeComponent neighbor in neighbors)
				{
					float cost = current.currentCost + current.node.CostToNeighbor(neighbor.Id);
					
					//Since the PQ checks deal with records, we make a dummy record
					//for the neighbor.  Note the estCost is not actually estimated
					//yet.
					NodeRecord tmpRec = new NodeRecord(neighbor, cost, cost);
					tmpRec.connection = current;
					float hCost = 0f;
					
					//if the node is in the closed PQ, it is either skipped
					//or removed for re-analysis
					if(closed.Contains(tmpRec))
					{
						Logger.LogMessage("Pathfinder: tmpRec in closed list.  Checking to see if info needs to be updated.", @"logger.log", Logger.Verbosity.High);
						//this is a little weird, but I like it that way
						//because of how I wrote Find for PQ and implemented IComparable for
						//NodeRecord finding the tmpRec does not actually return tmpRec,
						//but it instead returns the element in the internal PQ array that
						//has the same Node as tmpRec.
						NodeRecord curRec = closed.Find(tmpRec);
						
						//if the curRec cost is lower than tmpRec, just skip out
						if(tmpRec.estCost > curRec.currentCost)
						{
							Logger.LogMessage("Pathfinder: tmpRec.estCost > curRec.currentCost so move on to next neighbor.", @"logger.log", Logger.Verbosity.High);
							continue;
						}

						Logger.LogMessage("Pathfinder: tmpRec needs to be updated, remove from closed list.", @"logger.log", Logger.Verbosity.High);
						//well, now we have to remove the current record from the PQ
						closed.Remove(curRec);
						
						//cheat for finding the heursitic cost, subtraction
						hCost = curRec.estCost - curRec.currentCost;
					}
					else if(open.Contains(tmpRec)) //if the node is already opened, we have some info we can use
					{
						Logger.LogMessage("Pathfinder: tmpRec in open list.  Checking to see if info needs to be updated.", @"logger.log", Logger.Verbosity.High);
						//same idea as with the closed PQ here, check to see if we
						//have a shorter path to the found record, and use that if
						//we do.
						NodeRecord curRec = open.Find(tmpRec);
						
						//if the curRec cost is lower than tmpRec, just skip out
						if(tmpRec.estCost > curRec.currentCost)
						{
							Logger.LogMessage("Pathfinder: tmpRec.estCost > curRec.currentCost so move on to next neighbor.", @"logger.log", Logger.Verbosity.High);
							continue;
						}
						
						//at this point we can just use the data from the
						//current record.

						Logger.LogMessage("Pathfinder: tmpRec needs to be updated, so updating.", @"logger.log", Logger.Verbosity.High);

						//cheat for finding the heursitic cost, subtraction
						hCost = curRec.estCost - curRec.currentCost;
					}
					else //need to make a new record and entry in the open PQ
					{
						//for now that just means calculating the hCost
						Logger.LogMessage("Pathfinder: tmpRec not in a list yet.  Find hCost and then add to open list.", @"logger.log", Logger.Verbosity.High);
						hCost = _h(neighbor.transform.position, endPos);
					}
					
					//at this point the data for the tmpRec needs to be updated on the
					//hCost, and then put into the open list
					tmpRec.estCost = tmpRec.currentCost + hCost;
					
					if(!open.Contains(tmpRec))
					{
						Logger.LogMessage("Pathfinder: Adding tmpRec to open list.", @"logger.log", Logger.Verbosity.High);
						open.Add(tmpRec);
					}

					Logger.LogMessage("Pathfinder: Yielding to avoid slowing framerate.", @"logger.log", Logger.Verbosity.High);
					//yield to allow for long execution via coroutine
					yield return null;
				}
				
				//we're done analyzing things for the current node, so put it in the closed list
				Logger.LogMessage("Pathfinder: Done analyzing current node, adding to closed list and removing from open list.", @"logger.log", Logger.Verbosity.High);
				open.Remove(current);
				closed.Add(current);
			}
			
			//now we either compile the path, or we return null for no path found
			//if we wanted to do a best guess path, I think this would be unnecessary
			if(current.node.CompareTo(end) != 0)
			{
				Logger.LogMessage("Pathfinder: Could not find path to end.  Setting path to null and noting that this does not hit the end node.", @"logger.log", Logger.Verbosity.High);
				_path = null;
				_hitsEndNode = false;
			}
			else
			{
				Logger.LogMessage("Pathfinder: Found a path.  Compiling path to a proper ordered list.", @"logger.log", Logger.Verbosity.High);
				//okay, we found a path!  now we just put it all together
				_path = compilePath(start, current);
				_hitsEndNode = true;
			}

			Logger.LogMessage("Pathfinder: Done.", @"logger.log", Logger.Verbosity.High);
			_done = true;
		}
		
		/// <summary>
		/// Compiles the path by tracing back from <paramref name="endRecord"/>.
		/// </summary>
		/// <returns>The path.</returns>
		/// <param name="start">Start node.</param>
		/// <param name="endRecord">End record.</param>
		private List<NodeComponent> compilePath(NodeComponent start, NodeRecord endRecord)
		{
			NodeRecord current = endRecord;
			List<NodeComponent> path = new List<NodeComponent>();
			while(current.node != start)
			{
				path.Add(current.node);
				current = current.connection;
			}
			path.Add(start);
			//since the path is reversed the way we just did things, reverse it to get it right
			path.Reverse();
			return path;
		}

		/// <summary>
		/// Gets a list of <see cref="NodeComponents"/> that represent a path.
		/// </summary>
		/// <value>The path.</value>
		public List<NodeComponent> Path { get { return _path; } }

		/// <summary>
		/// Gets a value indicating whether this <see cref="Inkspot.Pathfinding.Pathfinder"/> path reaches the specified end in the last path find.
		/// </summary>
		/// <value><c>true</c> if path reaches end; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// This can be used to determine if a path exists at all.
		/// </remarks>
		public bool PathReachesEnd { get { return _hitsEndNode; } }

		/// <summary>
		/// Gets a value indicating whether this <see cref="Inkspot.Pathfinding.Pathfinder"/> is done with the last path finding started.
		/// </summary>
		/// <value><c>true</c> if done; otherwise, <c>false</c>.</value>
		public bool Done { get { return _done; } }
	}
}