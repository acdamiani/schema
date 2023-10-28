# Creating Custom Conditionals

Custom conditionals were designed to be simple to create in Schema. All that you need for a functional conditional is a method with a boolean return type. This method has the following signature:

```csharp
public abstract bool Evaluate(object conditionalMemory, SchemaAgent agent);
```

This Evaluate method will simply determine if its attached node can run. A value of true will mean yes, all good here, this node can run, while a value of false will tell the engine to stop trying to execute the node (at least this frame). All conditionals have to evaluate as true for the node to be executed. So, even if your conditional evaluates as true, it doesn't mean that they all will.

Note the two parameters inside the `Evaluate` method. These parameters are functionally equivalent to the parameters for the `Node.Tick` method.

This method will run as determined by the engine and affected by user settings for the conditional in the inspector. Conditionals, like all other Schema objects support custom fields that will affect all instances of the conditional. They also support custom editors like any other Unity class.

## Example Conditional

As a quick example, let's create a conditional that will only run its attached node if the agent's distance to a vector is less than a specified value.

Start by creating a file with the given class declaration. If you read the guide for creating custom actions, you'll notice that the class declaration is similar to the one for an action. Just inherit from the `Conditional` class and override the required method.

```csharp
using Schema;
using UnityEngine;

public class ExampleConditional : Conditional
{
    // Override the Evaluate method or else your environment will throw an error
    public override bool Evaluate(object conditionalMemory, SchemaAgent agent)
    {
        // Will always run; conditional does nothing
        return true;
    }
}
```

To create the functionality for our custom Conditional, let's start by declaring our variables at the top of the class.

```csharp
public class ExampleConditional : Conditional
{
    public BlackboardEntrySelector<Vector3> vector;
    public BlackboardEntrySelector<float> maxDist;
    // ...
}
```

If you've never seen the `BlackboardEntrySelector` class before, take a look at the page for it. Essentially, they are a way to choose either a value from the blackboard instance, or a static value. If you want to access a value, use this class.

Now, for the actual logic:

```csharp
public override bool Evaluate(object conditionalMemory, SchemaAgent agent)
{
    // vector.value holds the Vector3 value that we want to use
    Vector3 target = vector.value;
    // maxDist.value holds the maximum distance the agent can be away from the vector
    float dist = maxDist.value;

    // Only return true if the distance between the two positions is less than the distance value
    return Vector3.Distance(agent.transform.position, target) < dist;
}
```

And that's all you need to create a custom conditional! Note that the `Evaluate` method may be called every frame, but only if the conditional's `aborts` field is set to `Aborts.LowerPriority`, `Aborts.Self`, or `Aborts.Both`. And the current node can be aborted. Keep this in mind when creating conditionals, because expensive calculations can stack and drastically slow down the tree.

For reference, here is the full custom conditional class:

```csharp
using Schema;
using UnityEngine;

public class ExampleConditional : Conditional
{
    public BlackboardEntrySelector<Vector3> vector;
    public BlackboardEntrySelector<float> maxDist;

    // Override the Evaluate method or else your environment will throw an error
    public override bool Evaluate(object conditionalMemory, SchemaAgent agent)
    {
        // vector.value holds the Vector3 value that we want to use
        Vector3 target = vector.value;
        // maxDist.value holds the maximum distance the agent can be away from the vector
        float dist = maxDist.value;

        // Only return true if the distance between the two positions is less than the distance value
        return Vector3.Distance(agent.transform.position, target) < dist;
    }
}
```
