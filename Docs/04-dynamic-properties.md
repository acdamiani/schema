# Dynamic Properties

Dynamic properties are a feature of the Blackboard that allows you to get/set properties on objects in the blackboard. For example, instead of using nodes to get and set the position of a `GameObject` variable, you can use the actual properties of the object when assigning it in the inspector. This drastically reduces the amount of nodes present in your tree, and makes it easier to see past the fluff and look at the important parts of the tree.

> **Note**: Dynamic properties are not recommended if you are compiling using IL2CPP or for an AOT platform like iOS. Dynamic properties use dynamic code generation to get and set values for fields and properties extremely quickly, almost as fast as native access. However, this method is not possible when using AOT due to restrictions on the platform. Dynamic properties will therefore be much slower when compiling for these platforms.

Using dynamic properties is extremely simple. Unless the node has the `[DisableDynamicBinding]` attribute set for its selectors, you will see a dropdown of the properties when selecting an entry for a field.

For example, a node might require a `Vector3` to do some calculation. Maybe it rotates it around a point, translates it, or just prints it to the console. If you wanted to input a `GameObject`'s position, you would have to create a temporary blackboard variable, store the position using a node, and access that blackboard variable later in the tree. With dynamic properties, you can access that property *directly* on the `GameObject` without writing any code or creating any extra nodes.
