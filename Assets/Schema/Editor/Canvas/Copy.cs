using System.Collections.Generic;
using System.Linq;
using Schema;
using SchemaEditor.Internal.ComponentSystem.Components;

namespace SchemaEditor.Internal
{
    public static class Copy
    {
        public static void DoCopy(ComponentCanvas canvas, IEnumerable<NodeComponent> nodes)
        {
            nodes = nodes.OrderBy(x => x.node.priority);

            Dictionary<Node, Node> copiesForwards = nodes
                .ToDictionary(x => x.node, x => Node.Instantiate(x.node));

            Dictionary<Node, Node> copiesBackwards = copiesForwards
                .ToDictionary(x => x.Value, x => x.Key);

            foreach (Node copy in copiesForwards.Values)
            {
                Node original = copiesBackwards[copy];

                copiesForwards.TryGetValue(original.parent, out Node parent);

                if (parent != null)
                    parent.AddConnection(copy);
            }

            foreach (Node copy in copiesForwards.Values)
            {
                NodeComponent.NodeComponentCreateArgs createArgs = new NodeComponent.NodeComponentCreateArgs();
                createArgs.fromExisting = copy;

                canvas.Create<NodeComponent>(createArgs);
                copy.graph.AddNode(copy, copy.graphPosition);
            }
        }
    }
}