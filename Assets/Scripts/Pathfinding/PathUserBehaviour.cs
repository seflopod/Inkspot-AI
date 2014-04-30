// PathUserBehaviour.cs
// by: Peter Bartosch <bartoschp@gmail.com>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Inkspot.Pathfinding;
using Inkspot.Behaviors;
using Inkspot.Utility;

namespace Inkspot.Pathfinding
{
	/// <summary>
	/// A <c>MonoBehaviour</c> that uses pathfinding..
	/// </summary>
	/// <remarks>
	/// This is a sample of how to utilize the pathfinding provided in Inkspot.
	/// Users are encouraged to attempt to make their own version for their
	/// own projects, if only for the fun involved :).
	/// </remarks>
	public class PathUserBehaviour : MonoBehaviour
	{
		public float speed = 3f;
		public GameObject graphGameObject;
		public Vector3 targetPos;
		public bool useLookAt = true;
		public bool useBehaviorForTarget = false;
		public GameObject behaviorTreeObject;
		public string targetVarName;

		private Vector3 _curTarget;
		private int _pathIdx;
		private GraphComponent _pathGraph;
		private Pathfinder _pathfinder;
		private BehaviorTree _tree;

		// Use this for initialization
		private void Start()
		{
			if(useBehaviorForTarget)
				_tree = behaviorTreeObject.GetComponent<BehaviorTree>();

			_pathGraph = graphGameObject.GetComponent<GraphComponent>();
			_pathfinder = new Pathfinder();
			getNewTarget();
			_curTarget = transform.position;
			_pathIdx = 0;
		}
		
		// Update is called once per frame
		private void Update()
		{
			if(_curTarget != targetPos)
			{
				_curTarget = targetPos;
				//find new path to target
				StartCoroutine(_pathfinder.FindPath(_pathGraph, transform.position, _curTarget));
				_pathIdx = 0;
			}
			else if(_pathfinder.Done && _pathIdx < _pathfinder.Path.Count)
			{
				Vector3 nodePos = _pathfinder.Path[_pathIdx].transform.position;
				float delta = speed * Time.deltaTime;
				Logger.LogMessage("PathUserBehaviour: Moving "+delta+" units at "+speed+" units/sec.", Logger.Verbosity.Low);

				if(useLookAt)
				{	//look at the target position if appropriate and move in it's direction
					transform.LookAt(nodePos);
					transform.Translate(delta*Vector3.forward);
				}
				else
				{	//just move in direction of target
					Vector3 dir = (nodePos - transform.position).normalized;
					transform.Translate(delta*dir, Space.World);
				}

				//this should be refined to detect if a target node has been
				//overshot by more than .01 units due to speed.
				if((nodePos - transform.position).sqrMagnitude <= .01f)
				{
					Logger.LogMessage("PathUserBehaviour: Reached target node, moving to next.", Logger.Verbosity.High);
					_pathIdx++;
				}
				else
				{
					Logger.LogMessage("PathUserBehaviour: Done with this movement.", Logger.Verbosity.Low);
				}

				//at the end of the path, look for a new target.
				if(_pathIdx >= _pathfinder.Path.Count)
				{
					Logger.LogMessage("PathUserBehaviour: End of path.", Logger.Verbosity.Low);
					getNewTarget();
				}
			}

			//if we're getting the target from a blackboard, then check to
			//see if we have a new one to use.
			if(useBehaviorForTarget && _tree.IsOnBlackboard(targetVarName))
			{
				getNewTarget();
			}
		}

		/// <summary>
		/// Selects a random target.
		/// </summary>
		public void SelectRandomTarget()
		{
			targetPos = _pathGraph.Nodes[UnityEngine.Random.Range(0, _pathGraph.Nodes.Count)].transform.position;
		}

		/// <summary>
		/// Gets a new target position.
		/// </summary>
		private void getNewTarget()
		{
			Logger.LogMessage("PathUserBehaviour: Getting new target position.", Logger.Verbosity.Low);
			if(useBehaviorForTarget && _tree.IsOnBlackboard(targetVarName))
			{
				List<Sensable> listSensed = (List<Sensable>)_tree.GetFromBlackboard(targetVarName);

				targetPos = listSensed[0].transform.position;
			}
			else
				SelectRandomTarget();
		}
	}
}
