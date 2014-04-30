// BehaviorTreeImportException.cs
// by: Peter Bartosch <bartoschp@gmail.com>
//
// Define custom exceptions used in importing behaviors
//
// TODO
// * Document
//
using UnityEngine;
using System;
using System.Runtime.Serialization;

namespace Inkspot.Behaviors
{
	[System.Serializable]
	/// <summary>
	/// An exception to indicate a general error in importing a behavior
	/// </summary>
	public class BehaviorImportException : Exception, ISerializable
	{
		public BehaviorImportException() : base("Error importing behavior tree from XML.")
		{}

		public BehaviorImportException(string message) : base(message)
		{}

		public BehaviorImportException(string message, Exception inner) : base(message, inner)
		{}

		//Constructor required for serialization
		protected BehaviorImportException(SerializationInfo info, StreamingContext context) : base(info, context)
		{}
	}

	[System.Serializable]
	/// <summary>
	/// An exception to indicate a node_type indicated in the behavior XML does not derive from <see cref="BehaviorNode"/>.
	/// </summary>
	public class BehaviorImportBadTypeException : BehaviorImportException, ISerializable
	{
		public BehaviorImportBadTypeException() : base("Specified node_type does not derive from Inkspot.Behaviors.Nodes.BehaviorNode")
		{}

		public BehaviorImportBadTypeException(string message) : base(message)
		{}

		public BehaviorImportBadTypeException(string message, Exception inner) : base(message, inner)
		{}

		protected BehaviorImportBadTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{}
	}

	[System.Serializable]
	/// <summary>
	/// An exception to indicate that more than one <see cref="RootNode"/> was detected during import.
	/// </summary>
	public class BehaviorImportRootNodeException : BehaviorImportException, ISerializable
	{
		public BehaviorImportRootNodeException() : base("Multiple RootNodes detected, only one is allowed.")
		{}
		
		public BehaviorImportRootNodeException(string message) : base(message)
		{}
		
		public BehaviorImportRootNodeException(string message, Exception inner) : base(message, inner)
		{}
		
		protected BehaviorImportRootNodeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{}
	}

	/// <summary>
	/// An exception to indicate a general XML error during importing a behavior.
	/// </summary>
	public class BehaviorImportBadXMLException : BehaviorImportException, ISerializable
	{
		public BehaviorImportBadXMLException() : base("Bad XML for behavior.")
		{}
		
		public BehaviorImportBadXMLException(string message) : base(message)
		{}
		
		public BehaviorImportBadXMLException(string message, Exception inner) : base(message, inner)
		{}
		
		protected BehaviorImportBadXMLException(SerializationInfo info, StreamingContext context) : base(info, context)
		{}
	}
}