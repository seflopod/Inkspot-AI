using UnityEngine;
using System.Collections;
using Inkspot.Utility;

public class KeyFlagBehaviour : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		Logger.LogMessage("KeyFlagBehaviour: Hit key.", @"logger.log", Logger.Verbosity.VeryHigh);
		AIBehaviour ai = other.gameObject.GetComponent<AIBehaviour>();
		if(ai != null)
		{
			Logger.LogMessage("KeyFlagBehaviour: Hit key, giving key to AI.", @"logger.log", Logger.Verbosity.Low);
			ai.GiveKey();
			gameObject.SetActive(false);	
		}
	}
}
