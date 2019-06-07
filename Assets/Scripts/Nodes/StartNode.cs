using XNode;
using NodeVariables;

public class StartNode : Node
{
    [Output(ShowBackingValue.Never, ConnectionType.Override)] public NextTask NextTask;
	public override object GetValue(NodePort port) 
	{
		return true;
	}
}