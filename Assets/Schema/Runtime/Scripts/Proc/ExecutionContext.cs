using System;
using System.Collections.Generic;

namespace Schema.Internal
{
    public class ExecutionContext
    {
        public static ExecutionContext current { get; set; }
        public SchemaAgent agent { get; }
        public ExecutableNode node { get; set; }
        public ExecutableNode last { get; set; }
        public NodeStatus status { get; set; }
        public ExecutionContext(SchemaAgent agent)
        {
            this.agent = agent;

            status = NodeStatus.Success;
        }
    }
}