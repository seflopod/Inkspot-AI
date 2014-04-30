using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Inkspot.Utility;

namespace Inkspot.Scheduling
{
	public abstract class SchedulerBase : MonoBehaviour
	{
		private static List<IEnumerator> _tasks = new List<IEnumerator>();
		private static Dictionary<string, IEnumerator> _names = new Dictionary<string, IEnumerator>();

		public void ScheduleTask(string taskName, params object[] args)
		{
			MethodInfo method = this.GetType().GetMethod(taskName);
			if(args.Length == 0) args = null;
			IEnumerator task = method.Invoke(this, args) as IEnumerator;
			_names.Add(taskName, task);
			ScheduleTask(task);
		}

		public void ScheduleTask(IEnumerator task)
		{
			lock(SchedulerBase._tasks)
				SchedulerBase._tasks.Add(task);
		}

		public void RemoveTask(string taskName)
		{
			if(_names.ContainsKey(taskName))
			{
				IEnumerator task = _names[taskName];
				SchedulerBase._tasks.Remove(task);
				_names.Remove(taskName);
			}
		}

		public void Update()
		{
			for(int i=0;i<SchedulerBase._tasks.Count;++i)
			{
				bool remove = false;
				IEnumerator task = SchedulerBase._tasks[i];
				if(task.MoveNext())
				{
					Logger.LogMessage("Scheduler: Executing a task.", @"logger.log", Logger.Verbosity.VeryHigh);
					IInstruction inst = task.Current as IInstruction;
					if(inst != null) remove = !inst.Execute(this, task);
				}
				else
				{
					remove = true;
				}
				if(remove) _tasks.RemoveAt(i--);
			}
		}
	}
}