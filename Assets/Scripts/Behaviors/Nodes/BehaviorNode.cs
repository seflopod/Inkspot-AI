// BehaviorNode.cs
// by: Peter Bartosch <bartoschp@gmail.com>
//
// The base class for all BehaviorNodes in the behavior tree.
//
// TODO
// * Complete (de)serialization
using UnityEngine;
using System;
using System.Collections.Generic;
using Inkspot.Behaviors;

namespace Inkspot.Behaviors.Nodes
{
	/// <summary>
	/// The <c>BehaviorNode</c> class is used to define a node in the behavior
	/// tree.  This class defines all common operations used, leaving
	/// only <see cref="Inkspot.Behaviors.Nodes.BehaviorNode.Run"/>  and <see cref="Init"/>to be
	/// defined by inheritors.  DO NOT CREATE A DIRECT INSTANCE OF THIS CLASS.  Going by the
	/// guidelines in http://blogs.unity3d.com/2012/10/25/unity-serialization/ it appears that
	/// using an abstract class (the sane way to do this) is not okay.  This means that the
	/// notice "DO NOT CREATE A DIRECT INSTANCE OF THIS CLASS" must be given.  So
	/// DO NOT CREATE A DIRECT INSTANCE OF THIS CLASS.
	/// </summary>
	public class BehaviorNode : ScriptableObject
	{
		private static int _nextId = 1;
		private List<BehaviorNode> _children = new List<BehaviorNode>();
		private int _id;
		private bool _executed;
		private bool _scheduled;
		private bool _init;

		#region ctors and initializers
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="Inkspot.Behaviors.Nodes.BehaviorNode"/> class.
		/// </summary>
		public BehaviorNode()
		{
			_id = _nextId++;
			_executed = false;
			_scheduled = false;
			_init = false;
		}

		/// <summary>
		/// Init the <see cref="BehaviorNode"/> with the specified parameters.
		/// </summary>
		/// <param name="parameters">A list of parameters as <see cref="ParameterInfo"/> objects.</param>
		/// <returns>
		/// <c>true</c> if the <see cref="BehaviorNode"/> was initialized; <c>false</c> otherwise.
		/// </returns>
		public virtual bool Init(int id, List<ParameterInfo> parameters)
		{
			if(!_init)
			{
				_id = id;
				_init = true;
				return true;
			}

			return false;
		}
		#endregion

		#region behavior node functionality
		/// <summary>
		/// The coroutine for a specific
		/// <see cref="Inkspot.Behaviors.Nodes.BehaviorNode"/>.  This defines
		/// the actions a node takes when it is run.
		/// </summary>
		/// <param name="blackboard">The
		/// <see cref="Inkspot.Behaviors.Blackboard"/> used for data i/o in this
		/// behavior tree.
		/// </param>
		/// <param name="returnCode">
		/// The <see cref="Inkspot.Behaviors.ReturnCodeWrapper"/> for the parent of
		/// this behavior.  This is used to effectively pass
		/// <see cref="Inkspot.Behaviors.ReturnCode"/>s by reference.
		/// </param>
		public virtual System.Collections.IEnumerator Run(Blackboard blackboard, ReturnCodeWrapper returnCode)
		{
			return null;
		}

		/// <summary>
		/// Resets all child nodes of this
		/// <see cref="Inkspot.Behaviors.Nodes.BehaviorNode"/> by setting their
		/// <see cref="Inkspot.Behaviors.Nodes.BehaviorNode.Executed"/> and
		/// <see cref="Inkspot.Behaviors.Nodes.BehaviorNode.Scheduled"/>
		/// properities to <c>false</c>.
		/// </summary>
		protected void resetChildren()
		{
			for(int i=0; i<Children.Count; ++i)
			{
				Children[i].Executed = false;
				Children[i].Scheduled = false;
			}
		}

		/// <summary>
		/// Gets or sets the child nodes.
		/// </summary>
		/// <value>The children.</value>
		public List<BehaviorNode> Children
		{
			get { return _children; }
			set { _children = value; }
		}

		/// <summary>
		/// Gets the identifier for this node.
		/// </summary>
		/// <value>The identifier.</value>
		public int Id { get { return _id; } }

		/// <summary>
		/// Gets or sets a value indicating whether this
		/// <see cref="Inkspot.Behaviors.Nodes.BehaviorNode"/> has executed.
		/// </summary>
		/// <value><c>true</c> if executed; otherwise, <c>false</c>.</value>
		public bool Executed
		{
			get { return _executed; }
			set { _executed = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this
		/// <see cref="Inkspot.Behaviors.Nodes.BehaviorNode"/> is scheduled in
		/// the scheduler.
		/// </summary>
		/// <value><c>true</c> if scheduled; otherwise, <c>false</c>.</value>
		public bool Scheduled
		{
			get { return _scheduled; }
			set { _scheduled = value; }
		}
		#endregion
	}
}