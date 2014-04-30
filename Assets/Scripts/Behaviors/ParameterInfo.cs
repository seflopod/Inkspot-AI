// ParameterInfo.cs
// by: Peter Bartosch <bartoschp@gmail.com>
//
// A small struct for storing initialization parameter info for BehaviorNodes
//
using UnityEngine;
using System.Collections;

namespace Inkspot.Behaviors
{
	/// <summary>
	/// A struct for storing parameters from XML imports
	/// </summary>
	public struct ParameterInfo
	{
		public string name;
		public string value;
		
		public ParameterInfo(string name, string value)
		{
			this.name = name;
			this.value = value;
		}
	}
}