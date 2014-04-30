using UnityEngine;
using System.Collections;
using Inkspot.Behaviors;
using Inkspot.Utility;

[RequireComponent(typeof(BehaviorTree))]
public class AIBehaviour : MonoBehaviour
{
	public float speed = 3f;
	public string graphName;

	private BehaviorTree _behaviorTree = null;

	public void Awake()
	{
		Logger.LogLevel = Logger.Verbosity.High;
		Logger.LoggingOn = true;
		Logger.LogMessage("AIBehaviour: Starting logging.", @"logger.log", System.IO.FileMode.Create, true, Logger.Verbosity.VeryLow);
	}

	private void Start()
	{
		_behaviorTree = gameObject.GetComponent<BehaviorTree>();

		//initialize some values on the behavior tree
		_behaviorTree.SetInBlackboard("initDone", false);
		_behaviorTree.SetInBlackboard("goName", gameObject.name);
		_behaviorTree.SetInBlackboard("aiSpeed", speed);
		gameObject.GetComponent<Inkspot.Pathfinding.PathUserBehaviour>().SelectRandomTarget();
	}

	public void GiveKey()
	{
		Inkspot.Utility.Logger.LogMessage("AIBehaviour: Received key", @"logger.log", System.IO.FileMode.Append, true);
		GameObject.FindGameObjectWithTag("exitFlag").GetComponent<ExitFlagBehaviour>().Enable();
		_behaviorTree.SetInBlackboard("haveKey", true);
	}

	public BehaviorTree Behavior
	{
		get { return _behaviorTree; }
		set { _behaviorTree = value; }
	}
}
