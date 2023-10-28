# Creating Custom Flow Nodes

Flow nodes control how the behavior tree is executed and how control is ceded from one node to the other. You can create your own Flow nodes to have greater control over how your tree is executed.

Flow nodes can be extremely powerful, but the way that they work is quite simple. They don't have any control over the status or the next node beyond its parent or children, and they only act as scaffolding for the rest of the tree.

The method used to control the "flow" of the tree is the `Tick` method. It has the following signature:

```csharp
public abstract int Tick(object flowMemory, NodeStatus status, int index);
```

If you've read the Creating Custom Actions page, you'll know the purpose of the `flowMemory` parameter. The two other parameters, `status` and `index`, contain information about the context given to this node.

The `Tick` method for a Flow node will run under two circumstances:

- It is ticked from its parent
- It is ticked after its child is done executing

The `index` parameter is tasked with containing the values that describe these circumstances. When a flow node is ticked from its parent, the `index` parameter will have a value of -1. When ticked from its child, it will contain the index of that child in the array of children for this flow node.

The `status` parameter will contain the last status of the tree. When ticking from a child, its value will be that of the child's `Tick` return value. However, if the Flow node is ticked from its parent, it will contain the status of the last action executed by the tree. It may not necessarily be in the current flow's child array. Keep this in mind when programming your logic.

As stated above, flow nodes cannot modify the status of their child nodes, only pass them along to other parts of the tree. Its return value, of type `int` gives the index of the next child node to execute. This index is zero-based, so if you want to execute a flow's second child, return a value of 1. A return value of -1 will move up teh tree to the flow's parent, passing along the status of the last node.

As an example, here is the code for the `Tick` method for the Selector node. This node will execute its children, in order, until one of them returns `Success`.

```csharp
if (index == -1 && children.Length > 0)
    return 0;

if (index + 1 > children.Length - 1 || status == NodeStatus.Success)
    return -1;

return index + 1;
```

The first `if` statement is used to execute the first child no matter what the `status` parameter's value is. Remember, it contains the value of the last executed action node, not necessarily in the current children array.

The second if statement checks for two possibilities:

- The index is at the end of the child array
- The status of the last executed action node is Success

In both cases, we want to travel back up to the parent, so we return -1.

If the node was not successful but also not the last node in the array, we move onto the next node by returning `index + 1`.

You can also include variables and instance parameters in your `Flow` class to add extra control over the next node executed. For example, you could have an extremely simple node that will execute the node given by a Blackboard variable:

```csharp
using Schema;
using UnityEngine;

public class SelectFromVariable : Flow
{
    public BlackboardEntrySelector<int> nodeIndex;

    public override int Tick(object flowMemory, NodeStatus status, int index)
    {
        int i = nodeIndex.value;

        // If i is out of bounds, move up to the parent
        if (i < 0 || i > children.Length - 1)
            return -1;

        return i;
    }
}
```

This behavior makes creating custom flow nodes a very powerful tool for making your trees as custom-fit as possible.
