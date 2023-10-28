# Creating Custom Modifiers

Modifiers are a unique part of the Schema behavior tree that allows you to, as the name suggests, *modify* how a node functions, completely independent of the implementation of the node's logic. They are currently somewhat limited in what they can do, but still serve a valuable purpose in making nodes work well.

The method most important to know for creating modifiers is the `Modify` method. This method runs after the node it is attached to has completed its execution, and has the following signature:

```csharp
public virtual Message Modify(object modifierMemory, SchemaAgent agent, NodeStatus status) { }
```

This method is not required, as you can have a modifier that doesn't actually modify anything (maybe you want to Log a message to the console anytime its attached node has completed its execution for debugging), but this is the method you want to use for any sort of custom functionality.

Its parameters should be familiar to you if you've read the other documentation pages for creating custom nodes and conditionals. Its `status` parameter, as you may have guessed, contains the status of the node that this modifier is attached to.

Its return value, an enum of type `Message` has four possible values:

- `None`: WIll not do anything to the node
- `Repeat`: Will repeat the node, re-executing this method when the node has finished its execution
- `ForceSuccess`: Force the status of this node to be `NodeStatus.Success`
- `ForceFailure`: Force the status of this node to be `NodeStatus.Failure`

The order of the modifier stack in the inspector matters. If a `LoopForever` modifier is placed *after* a `ForceStatus` modifier, it will always loop the node, and ignore all other `Message` values returned by Modifiers above it. Note that the other `Modify` methods wil still run.
