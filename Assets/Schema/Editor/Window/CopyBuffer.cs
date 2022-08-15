using System;
using System.Linq;
using System.Collections.Generic;
using Schema;
using SchemaEditor.Internal.ComponentSystem;
using SchemaEditor.Internal.ComponentSystem.Components;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SchemaEditor.Internal
{
    public class CopyBuffer
    {
        public Object[] buffer { get { return _buffer; } }
        private Object[] _buffer;
        public Graph graph { get; }
        public ComponentCanvas canvas { get; }
        public CopyBuffer(ComponentCanvas canvas, IEnumerable<Object> objectsToCopy, Graph graph)
        {
            _buffer = Array.Empty<Object>();

            this.canvas = canvas;
            this.graph = graph;

            Copy(objectsToCopy);
        }
        ~CopyBuffer()
        {
            ClearBuffer();
        }
        private void ClearBuffer()
        {
            for (int i = 0; i < _buffer.Length; i++)
                Object.DestroyImmediate(_buffer[i]);

            _buffer = Array.Empty<Object>();
        }
        private void Copy(IEnumerable<Object> objectsToCopy)
        {
            Descriptor descriptor = GetDescriptor(objectsToCopy);

            if (descriptor == Descriptor.None)
                return;

            ClearBuffer();

            switch (descriptor)
            {
                case Descriptor.Conditionals:
                    _buffer = objectsToCopy
                        .Where(x => x is Conditional)
                        .Select(x => Conditional.Instantiate((Conditional)x))
                        .ToArray();
                    break;
                case Descriptor.NodesWithConditionals:
                    _buffer = DoNodeCopy(objectsToCopy).ToArray();
                    break;
            }
        }
        private IEnumerable<Object> DoNodeCopy(IEnumerable<Object> objectsToCopy)
        {
            IEnumerable<Node> nodes = objectsToCopy
                .Where(x => x is Node)
                .Cast<Node>();

            IEnumerable<Conditional> conditionals = objectsToCopy
                .Where(x => x is Conditional)
                .Cast<Conditional>();

            Dictionary<Node, Node> copiesForwards = nodes
                .ToDictionary(x => x, x => Node.Instantiate(x, conditionals));

            Dictionary<Node, Node> copiesBackwards = copiesForwards
                .ToDictionary(x => x.Value, x => x.Key);

            foreach (Node copy in copiesForwards.Values)
            {
                Node original = copiesBackwards[copy];

                copiesForwards.TryGetValue(original.parent, out Node parent);

                if (parent != null)
                    parent.AddConnection(copy);
            }

            return copiesForwards.Values;
        }
        public void Flush(IEnumerable<Object> selected)
        {
            Descriptor descriptor = GetDescriptor(buffer);

            foreach (Object o in _buffer)
            {
                Schema.Internal.GraphObject g = o as Schema.Internal.GraphObject;

                if (g == null)
                    continue;

                canvas.SelectWhenCreated(
                    x => x is IGraphObjectProvider && ((IGraphObjectProvider)x).Equals(g)
                    );
            }

            switch (descriptor)
            {
                case Descriptor.Conditionals:
                    IEnumerable<Node> selectedNodes = selected
                        .Where(x => x is Node)
                        .Cast<Node>();

                    foreach (Node node in selectedNodes)
                    {
                        for (int i = 0; i < _buffer.Length; i++)
                            node.AddConditional((Conditional)buffer[i]);
                    }

                    break;
                case Descriptor.NodesWithConditionals:
                    for (int i = 0; i < _buffer.Length; i++)
                        graph.AddNode((Node)_buffer[i]);
                    break;
            }
        }
        public Descriptor GetDescriptor(IEnumerable<Object> objectsToCopy)
        {
            Descriptor ret = Descriptor.None;

            if (
                objectsToCopy
                    .Where(x => x is Node)
                    .Count() > 0
            )
                ret = Descriptor.NodesWithConditionals;
            else if (
                objectsToCopy
                    .Where(x => x is Conditional)
                    .Count() > 0
            )
                ret = Descriptor.Conditionals;

            return ret;
        }
        public enum Descriptor
        {
            None,
            NodesWithConditionals,
            Conditionals
        }
    }
}