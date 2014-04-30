using UnityEngine;
using System.Collections;
using Inkspot.Utility;

public class ExitFlagBehaviour : MonoBehaviour
{
	private void Start()
	{
		collider.enabled = false;
		renderer.enabled = false;

	}
	private void OnTriggerEnter(Collider other)
	{
		Logger.LogMessage("ExitFlagBehaviour: hit exit flag", @"logger.log", Logger.Verbosity.High);
	}

	public void Enable()
	{
		Logger.LogMessage("ExitFlagBehaviour: enabling this flag and removing the wall.", @"logger.log");
		collider.enabled = true;
		renderer.enabled = true;
		GameObject.FindGameObjectWithTag("exitWall").SetActive(false);
		GetComponentInChildren<Light>().enabled = true;
	}
}
