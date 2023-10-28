# Creating Custom Actions

It is almost certain that at some point during development, you will require an Action with no built-in equivalent. Luckily, Schema is designed to be extremely extendable, allowing you to easily integrate custom Actions into your trees. The most important part of an action node is a method, executed once per frame, that returns the status of the node. The method signature is as shown:

```csharp
public abstract NodeStatus Tick();
```

This is the simplest version of the Tick method, which takes no arguments and simply returns a status. This status, of type `NodeStatus`, is an enum that will determine whether the Node should finish its execution and move throughout the tree, or continue running. The possible values for this enum are `Success`, `Failure`, or `Running`. Returning `Success` or `Failure` will immediately terminate the Node's execution and continue on in the tree, while Running will return to the method next frame.

A more complex method signature is provided, which allows for the use of instance variables, as well as accessing the agent running the node.

```csharp
public abstract NodeStatus Tick(object nodeMemory, SchemaAgent agent);
```

Let's go over the first argument. To save on memory as trees scale to potentially hundreds of agents, memory preservation becomes increasingly important. For this reason, Schema will create only one instance of your `Action` class to save on memory. This has the pleasant side effect of making the Node's properties (displayed in the inspector) affect all tree instances. So, you could change a field on in the Node class, and it would affect all of the running agent instances. Unfortunately, this means that fields that you do want changed per-agent are not available. This is where the first argument comes in. Schema will optionally create an instance of a "memory" class that acts as a data container for per-instance memory. Schema will then pass the instance of this class into the Tick method on runtime. Schema will create this memory instance from the first nested class inside of a node. If no such class is provided, it will pass `null` as the memory argument.

Take the following class declaration:

```csharp
using Schema;
using UnityEngine;

public class TestAction : Action
{
    class TestActionMemory
    {
        public float testField = 3.14f;
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        TestActionMemory memory = (TestActionMemory)nodeMemory;
        // Outputs 3.14
        Debug.Log(memory.testField);
        return NodeStatus.Success;
    }
}
```

As you can see from the code, the class `TestActionMemory` is the first nested class inside the node class definition. Schema will therefore pass an instance of this class, per agent instance, to the Tick method. You can safely cast to the memory type from the object parameter. Modifying these fields will also affect the state and will be reflected in future executions of the node.

The second parameter is much simpler. It is the reference to the SchemaAgent executing this node. You can use this parameter to access and modify information about the current `GameObject`, such as its position, name, tag, etc.

## Creating an example action

Let's use this information to create a custom Action, that will increment a counter for each frame that it is ticked and print the value of that counter.

First, begin by creating a new file and name it `ExampleAction.cs`.

The `ExampleAction` class should inherit from `Action`, which provides the methods and attributes that you need to create a node. The `Action` class is exposed in the Schema namespace, so be sure to import it at the top.

```csharp
using Schema;
using UnityEngine;

public class ExampleAction : Action
{

}
```

Schema requires you to override the `Tick` method, so we will override the one with the memory parameter, as we need it to create the counter.

```csharp
public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
{
    return NodeStatus.Success;
}
```

All errors should now be gone, and you can add your node to the tree. However, since there is no method logic, nothing will happen when the node gets run. It will simply yield execution and the tree will move on to the next node.

Let's add a `Debug.Log` before returning from the method.

```csharp
Debug.Log("Hello World!");
return NodeStatus.Success;
```

Now, when you run the node, it will print "Hello World!" and move on to the next node. To add a counter, we need to create the class that will store the memory for the node. It can be named anything, but for clarity's sake, it is a good idea to name it the name of the class suffixed with "Memory". Additionally, add the counter field that will be modified in the memory.

Your class should now look like this:

```csharp
public class TestAction : Action
{
    // This class will be created and passed into the Tick method
    class TestActionMemory
    {
        // The counter for our node
        public int counter;
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        Debug.Log("Hello World");
        return NodeStatus.Success;
    }
}
```

To use our counter, we first need to access the underlying `TestActionMemory` instance from the object parameter. To do that, simply cast to your desired type and store it in a method variable.

```csharp
TestActionMemory memory = (TestActionMemory)objectMemory;
```

Now that we have the instance, we can access the counter variable, as well as modify it. To print out and then increment the counter, replace the "Hello World" line with these lines:

```csharp
memory.counter++;
Debug.LogFormat("The node has been run {0} times!", memory.counter);
```

You can combine these lines if you want:

```csharp
Debug.LogFormat("The node has been run {0} times!", ++memory.counter);
```

And with that, our simple node is complete! Your full file should now contain this code:

```csharp
using Schema;
using UnityEngine;

public class TestAction : Action
{
    class TestActionMemory
    {
        public int counter;
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        TestActionMemory memory = (TestActionMemory)nodeMemory;
        Debug.LogFormat("The node has been run {0} times!", ++memory.counter);
        return NodeStatus.Success;
    }
}
```

Now running your node, you should see it behave as expected.

Alongside the `Tick` method, Schema provides other methods run throughout the lifecycle of the node, all taking memory and agent arguments. These methods are listed below.

```csharp
// Run when the tree is initialized (similar to Start in a MonoBehavior)
public virtual void OnInitialize(object nodeMemory, SchemaAgent agent) { }
// Run when the node is first Ticked
public virtual void OnNodeEnter(object nodeMemory, SchemaAgent agent) { }
// Run when the node is done with its execution
public virtual void OnNodeExit(object nodeMemory, SchemaAgent agent) { }
```

Those are the basics of creating custom nodes! Alongside Action nodes, Schema also allows you to create custom Flow, Conditionals, and Modifiers.
