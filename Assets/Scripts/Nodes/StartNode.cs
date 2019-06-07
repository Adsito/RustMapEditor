using XNode;
using NodeVariables;

public class StartNode : Node
{
    [Output] public NextTask NextTask;
	public override object GetValue(NodePort port) 
	{
		return true;
	}
}