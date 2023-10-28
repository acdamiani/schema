# What are Behavior Trees and why do I need them?

A behavior tree is a model used throughout the AAA space to develop complex Artificial Intelligence modularly. The strength of a behavior tree comes in its simple modularity: a tree is composed of simple tasks, which when coupled together create complex and lifelike behavior. They are similar to [Finite State Machines](https://en.wikipedia.org/wiki/Finite-state_machine), where a series of behaviors are executed based on external factors, like the proximity to another object.

An example of an FSM is Unity's built in Animator Controller, which utilizes animation "states" and switches between them based on user-specified conditions. Behavior trees are essentially "hierarchical" FSMs, where tasks are executed based on high-level decisions further up in the tree. This hierarchical structure makes them much more useful for natural behavior, as decisions naturally lead to other decisions and/or actions taken.

## The Structure of a Behavior Tree

As mentioned in the quick start guide, the behavior implementation that Schema uses includes 3 main types of nodes: Root, Flow, and Action. These nodes all serve their specific purpose in the tree.

The **Root** node is where the execution of the behavior tree begins; you can only have one in the tree, and all other nodes must be attached to it, directly or indirectly, for execution to be possible.

**Flow** nodes guide the behavior tree; they dictate the "Flow" of the execution context. They have one parent and multiple children, which can be other Flow nodes or Action nodes. An example of a Flow node is a Sequence, which all children are executed in order, until one fails or they all complete successfully. Flow nodes determine which child to execute next based off the status and index of the previous node. This allows for unique flow nodes, like executing a random child based on a weight.

**Action** nodes are where the actual visual results of the tree happen; they are like simple functions hooked into the tree that together give rise to complex behavior. An action could log a message to the console, move to a location, play a sound, or any other action that can be programmed by the developer. Action nodes have three possible states: `Success`, `Failure`, and `Running`. Returning `Running` will cache the node, and return to it next frame to tick again. `Success` and `Failure` statuses will stop the node's execution, and move up the tree. Its parent will then determine the next node to execute based on its location and status, and the tree continues.

You may notice that none of these nodes allow for conditional logic, where a node can be run or not based on a boolean condition. This is where another object becomes relevant: the **Conditional**. Conditionals are extremely simple, boolean functions whose return value will be used to determine whether a node can run. However, they become more complicated when you want to stop a subtree when a condition changes from true to false or vice versa. For this, you can use aborts, which tells the engine to stop running a node if the condition changes, and specifically when to do it.

Another important part of a behavior tree is the **Modifier**: it allows nodes to be looped or for their states to be changed. When a node is complete, the modifiers will be run, and can instruct the engine to perform tasks like repeating the node.

### The Blackboard
This behavior tree structure lacks an important feature: local variables and the ability to change the behavior of a tree based on its state. The **Blackboard** is a variable container; it contains a list of all local and global variables for a tree, with the ability to easily modify their values similar to how you would in regular C# code. Local variables are local to the tree instance only, similar to an object member in a MonoBehavior: they can change between different behaviors. Global variables on the other hand, will be shared with all tree instances, similarly to a static member in a MonoBehavior.

## Why Use a Behavior Tree?
If you've ever tried to code Artificial Intelligence for a game, you may have realized that the code quickly spirals out of control and becomes difficult to maintain. The solution for this problem is modularity. By decoupling actions and the conditions that execute them, you can string together actions and conditional flow to create complex behavior without the spaghetti that comes with programming it manually.

Another advantage of behavior trees are their simplicity; no programming knowledge is required to use Schema to create Artificial Intelligence, lowering the barrier for entry for developers that may want to create Artificial Intelligence without the hassle of learning complex algorithms and programming concepts required for good game AI.
