using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NodeVariables;

public class StartNode : Node 
{
    [Output] public NextTask nextTask;
	public override object GetValue(NodePort port) 
	{
		return true;
	}
}