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
        public CopyBuffer()
        {
            _buffer = Array.Empty<Object>();
        }
        ~CopyBuffer()
        {
            ClearBuffer();
        }
        public void ClearBuffer()
        {
            for (int i = 0; i < _buffer.Length; i++)
                Object.DestroyImmediate(_buffer[i]);

            _buffer = Array.Empty<Object>();
        }
        public void Copy(IEnumerable<Object> objectsToCopy)
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
            }
        }
        public void Paste(IEnumerable<Object> selected)
        {
            Descriptor descriptor = GetDescriptor(buffer);

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
            }
        }
        public Descriptor GetDescriptor(IEnumerable<Object> objectsToCopy)
        {
            Descriptor ret = Descriptor.None;

            if (
                objectsToCopy
                    .Any(x => x is Node)
            )
                ret = Descriptor.NodesWithConditionals;
            else if (
                objectsToCopy
                    .All(x => x is Conditional)
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