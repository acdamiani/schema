using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Schema.Runtime;

namespace Schema.Runtime
{
	[Serializable]
	public abstract class Action : Node
	{
		public override int _maxChildren => 0;
		public virtual void OnInitialize(object nodeMemory, SchemaAgent agent) { }
		public virtual void OnNodeEnter(object nodeMemory, SchemaAgent agent) { }
		public virtual void OnNodeExit(object nodeMemory, SchemaAgent agent) { }
		public abstract NodeStatus Tick(object nodeMemory, SchemaAgent agent);
	}
}