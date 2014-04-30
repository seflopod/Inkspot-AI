// BehaviorTree.cs
// by: Peter Bartosch <bartoschp@gmail.com>
// 
// The root of any behavior tree
//
// TODO
// * Make this serializable in Unity scene
// * (De)serialize with XML for external loading/saving.
// * Find a way to define the XML schema and document it
// * Add Blackboard initialization through XML
//
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Inkspot.Utility;
using Inkspot.Behaviors.Nodes;
using Inkspot.Scheduling;
using System.Reflection;

/*
 * xml sample
 * <behavior>
 *  <node id="[int]">
 *   <node_type>[string]</node_type>
 *   <parameter>
 *    <name>[string]</name>
 *    <value>[as string]</value>
 *   </parameter>
 *   ...(0..* params available)
 *   <children>
 *    <child_id>[int]</child_id>
 *    ...(0..* children)
 *   </children>
 *  </node>
 * </behavior>
 */
namespace Inkspot.Behaviors
{
	[System.Serializable]
	public class BehaviorTree : Inkspot.Scheduling.SchedulerBase
	{
		public TextAsset behaviorTreeFile;

		[HideInInspector]
		private Nodes.RootNode _root;

		/*[SerializeField]
		[HideInInspector]*/
		private Blackboard _blackboard = new Blackboard();

		private bool _running = false;
		private bool _doTree = false;

		#region monobehaviour
		public void Start()
		{
			if(behaviorTreeFile == null)
			{
				_doTree = false;
				Logger.LogMessage("BehaviorTree: EXCEPTION: behaviorTreeFile left null, cannot run behavior tree.", @"logger.log", System.IO.FileMode.Append, false);
				throw new System.ArgumentNullException("behaviorTreeFile left null, cannot run behavior tree.");
			}
			else
			{
				_doTree = true;
				getAllBehaviorNodes();
			}
		}

		public new void Update()
		{
			if(_doTree)
			{
				if(!_running) ScheduleTask(parseTree());
				base.Update();
			}
		}
		#endregion

		#region xml loading
		/// <summary>
		/// Gets all behavior nodes by parsing the <see cref="XMLNode"/>s from <c>behaviorTreeFile</c>.
		/// </summary>
		private void getAllBehaviorNodes()
		{
			XMLNode rootNode = XMLParser.Parse(behaviorTreeFile.text).Children[0];
			Dictionary<int, List<int>> idToChildren = new Dictionary<int, List<int>>();
			Dictionary<int, BehaviorNode> idToNode = new Dictionary<int, BehaviorNode>();
			
			IEnumerator<XMLNode> childEnumerator = rootNode.Children.GetEnumerator();
			while(_doTree && childEnumerator.MoveNext())
			{
				XMLNode curNode = childEnumerator.Current;
				if(!curNode.Name.Equals("node")) continue;
				Type behaviorType = null;
				
				try
				{
					behaviorType = getNodeType(curNode);
				}
				catch
				{
					_doTree = false;
					throw;
				}
				
				if(behaviorType == null)
				{
					_doTree = false;
					Logger.LogMessage("BehaviorTree: EXCEPTION: Behavior node type does not exist or could not be found.", @"logger.log", System.IO.FileMode.Append, false);
					throw new NullReferenceException("Behavior node type does not exist or could not be found.");
				}
				else //make sure the found type inherits from BehaviorNode in some way
				{
					Type baseType = behaviorType;
					bool fromBehaviorNode = false;
					
					//Type.BaseType only returns a direct derivation, need to loop up through parents
					//this is only a possibility, but one worth accounting for.
					while(baseType != null && !fromBehaviorNode)
					{
						if(baseType == typeof(BehaviorNode))
							fromBehaviorNode = true;
						
						baseType = baseType.BaseType;
					}
					if(!fromBehaviorNode)
					{
						_doTree = false;
						Logger.LogMessage("BehaviorTree: EXCEPTION: BehaviorImportBadTye", @"logger.log", System.IO.FileMode.Append, false);
						throw new BehaviorImportBadTypeException();
					}
				}
				
				//assume that the node_type is valid, move on to adding
				//it to the dictionaries
				if(curNode.Attributes.ContainsKey("id"))
				{
					int id;
					if(Int32.TryParse(curNode.Attributes["id"], out id))
					{
						if(idToNode.ContainsKey(id))
						{
							_doTree = false;
							Logger.LogMessage("BehaviorTree: EXCEPTION The id " + id.ToString() + " already exists.", @"logger.log", System.IO.FileMode.Append, false);
							throw new ArgumentException("The id " + id.ToString() + " already exists.");
						}
						else
						{
							//we have an id and a node_type, now
							//we just need the parameters for making the
							//node and we're good!
							
							List<ParameterInfo> parameters = new List<ParameterInfo>();

							try
							{
								parameters = getParameterInfo(curNode);
							}
							catch
							{
								_doTree = false;
								throw;
							}


							MethodInfo[] methods = typeof(ScriptableObject).GetMethods(BindingFlags.Public | BindingFlags.Static);
							MethodInfo createInfo = null;
							bool done = false;
							for(int i=0;i<methods.Length && !done;++i)
							{
								System.Reflection.ParameterInfo[] paramsInfo = methods[i].GetParameters();
								for(int j=0;j<paramsInfo.Length;++j)
								{
									if(paramsInfo[j].ParameterType == typeof(Type))
									{
										createInfo = methods[i];
										done = true;
										break;
									}
								}
							}
							//ConstructorInfo ctorInfo = behaviorType.GetConstructor(Type.EmptyTypes);
							//if(ctorInfo != null)
							if(createInfo != null)
							{
								//I'm a bit afraid of the boxing issues here, this might not
								//work like I want.
								//BehaviorNode newNode = (BehaviorNode)ctorInfo.Invoke(null);
								BehaviorNode newNode = (BehaviorNode)createInfo.Invoke(null, new System.Object[1]{behaviorType});
								if(!newNode.Init(id, parameters))
								{
									Logger.LogMessage("BehaviorTree: EXCEPTION: BehaviorImportException, unable to initialize"+newNode.GetType().Name + " with id "+id+".", @"logger.log", System.IO.FileMode.Append, false);
									throw new BehaviorImportException("unable to initialize"+newNode.GetType().Name + " with id "+id+".");
								}
								idToNode.Add(id, newNode);
								
								if(behaviorType == typeof(Nodes.RootNode))
								{
									if(_root == null)
									{
										_root = (Nodes.RootNode)newNode;
									}
									else
									{
										_doTree = false;
										Logger.LogMessage("BehaviorTree: EXCEPTION: BehaviorImportRootNodeException", @"logger.log", System.IO.FileMode.Append, false);
										throw new BehaviorImportRootNodeException();
									}
								}
							}
							else
							{
								//wtf?  we shouldn't get here
								_doTree = false;
								//throw new BehaviorImportException("BehaviorNode ctor not found.  This should not appear.");
								Logger.LogMessage("BehaviorTree: EXCEPTION: ScriptableObject CreateInstance not found.  This should not appear.", @"logger.log", System.IO.FileMode.Append, false);
								throw new BehaviorImportException("ScriptableObject CreateInstance not found.  This should not appear.");
							}
							
							//keep track of what this node considers its children
							try
							{
								idToChildren.Add(id, getChildList(curNode));
							}
							catch
							{
								_doTree = false;
								throw;
							}
						}
					}
				}
				else
				{
					//I think I should do this...
					_doTree = false;
					Logger.LogMessage("BehaviorTree: EXCEPTION: Required attribute 'id' not found on node tag.", @"logger.log", System.IO.FileMode.Append, false);
					throw new BehaviorImportBadXMLException("Required id attribute not found on node tag.");
				}
			}
			
			if(_doTree)
			{
				//at this point we should have all our nodes, almost.
				//we still need to specify children
				var allNodes = idToNode.Values;
				BehaviorNode[] nodeArray = new BehaviorNode[allNodes.Count];
				allNodes.CopyTo(nodeArray, 0);
				for(int i=0;i<nodeArray.Length;++i)
					foreach(int childId in idToChildren[nodeArray[i].Id])
						nodeArray[i].Children.Add(idToNode[childId]);
			}
			else
			{
				Logger.LogMessage("BehaviorTree: EXCEPTION: BehaviorImportExecption", @"logger.log", System.IO.FileMode.Append, false);
				throw new BehaviorImportException();
			}
		}
		/// <summary>
		/// Gets a list of parameters for the passed <see cref="XMLNode"/>
		/// </summary>
		/// <returns>A <see cref="List{T}"/> of <see cref="ParameterInfo"/>.</returns>
		/// <param name="node">The <see cref="XMLNode"/> to check.</param>
		private List<ParameterInfo> getParameterInfo(XMLNode node)
		{
			List<ParameterInfo> ret = new List<ParameterInfo>();
			List<XMLNode> paramNodes = new List<XMLNode>();

			//first find the nodes containng all of the parameter information
			foreach(XMLNode childNode in node.Children)
			{
				if(childNode.Name.Equals("parameter"))
					paramNodes.Add(childNode);
			}

			//loop through the parameter nodes and extract their values
			foreach(XMLNode paramNode in paramNodes)
			{
				string name = null;
				string valueStr = null;

				//each parameter as a name and a value
				//get that information
				foreach(XMLNode childNode in paramNode.Children)
				{
					if(childNode.Name.Equals("name"))
					{
						name = childNode.Text;
					}
					else if(childNode.Name.Equals("value"))
					{
						valueStr = childNode.Text;
					}
					else
					{
						Logger.LogMessage("BehaviorTree: EXCEPTION: Invalid tag found in parameter", @"logger.log", System.IO.FileMode.Append, false);
						throw new BehaviorImportBadXMLException("Invalid tag found in parameter.");
					}
				}

				if(name != null) ret.Add(new ParameterInfo(name, valueStr));
				else
				{
					Logger.LogMessage("BehaviorTree: EXCEPTION: 'parameter' tag is required to have a non-null child 'name'", @"logger.log", System.IO.FileMode.Append, false);
					throw new BehaviorImportBadXMLException("parameter tag is required to have a non-null name");
				}
			}

			return ret;
		}

		/// <summary>
		/// Gets the node type referred to by the <see cref="XMLNode"/>.
		/// </summary>
		/// <returns>A <see cref="Type"/> for the node_type tag in <paramref name="node"/>.</returns>
		/// <param name="node">The <see cref="XMLNode"/> to check.</param>
		private Type getNodeType(XMLNode node)
		{
			Type ret = null;
			foreach(XMLNode childNode in node.Children)
			{
				if(childNode.Name.Equals("node_type"))
				{
					string qName = Assembly.CreateQualifiedName(typeof(BehaviorNode).Assembly.FullName, "Inkspot.Behaviors.Nodes." + childNode.Text);
					ret = Type.GetType(qName, false, true);
					if(ret == null) ret = Type.GetType(childNode.Text, false, true);
					break;
				}
			}

			return ret;
		}

		/// <summary>
		/// Gets the list of ids for the children of the specified <see cref="XMLNode"/>.
		/// </summary>
		/// <returns>A <see cref="List{int}/> reprenting the ids of the children of <paramref name="node"/>.</returns>
		/// <param name="node">An <see cref="XMLNode"/> using the schema for behavior trees.</param>
		private List<int> getChildList(XMLNode node)
		{
			List<int> ret = new List<int>();
			XMLNode childRoot = null;

			//first find the node that contains the ids (named "children")
			foreach(XMLNode childNode in node.Children)
			{
				if(childNode.Name.Equals("children"))
				{
					childRoot = childNode;
					break;
				}
			}

			//go through each "child_id" under the childNode and extract the id
			if(childRoot != null)
			{
				foreach(XMLNode childNode in childRoot.Children)
				{
					if(childNode.Name.Equals("child_id"))
					{
						try
						{
							ret.Add(Int32.Parse(childNode.Text));
						}
						catch
						{
							_doTree = false;
							throw;
						}
					}
				}
			}
			return ret;
		}
		#endregion

		#region behavior tree parsing
		/// <summary>
		/// Parses the tree.  This will flag that the tree is being traversed so
		/// that the update does not continuously try to run the behavior.
		/// </summary>
		/// <returns>The tree.</returns>
		private IEnumerator parseTree()
		{
			ReturnCodeWrapper retCode = new ReturnCodeWrapper(ReturnCode.Running);
			
			while(!_root.Executed)
			{
				if(!_running)
				{
					_running = true;
					yield return new AIInstruction(_root, _blackboard, retCode);
				}
				else yield return new ContinueInstruction();
			}
			_root.Executed = false;
			_root.Scheduled = false;
			_running = false;
		}
		#endregion

		/// <summary>
		/// Sets the variable in the blackboard.
		/// </summary>
		/// <param name="varName">Variable name.</param>
		/// <param name="obj">Object.</param>
		/// <remarks>
		/// Any exceptions from accessing the inner <c>Dictionary</c> of <see cref="Blackboard"/> are uncaught.
		/// </remarks>
		public void SetInBlackboard(string varName, System.Object obj)
		{
			_blackboard[varName] = obj;
		}

		/// <summary>
		/// Gets a variable from blackboard.
		/// </summary>
		/// <returns>The from blackboard.</returns>
		/// <param name="varName">Variable name.</param>
		/// <remarks>
		/// Any exceptions from accessing the inner <c>Dictionary</c> of <see cref="Blackboard"/> are uncaught.
		/// </remarks>
		public System.Object GetFromBlackboard(string varName)
		{
			return _blackboard[varName];
		}

		/// <summary>
		/// Determines if an item is in a blackboard.
		/// </summary>
		/// <returns><c>true</c> if the variable is on the blackboard; otherwise, <c>false</c>.</returns>
		/// <param name="varName">Variable name.</param>
		public bool IsOnBlackboard(string varName)
		{
			return _blackboard.Contains(varName);
		}
	}
}
