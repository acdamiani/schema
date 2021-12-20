using System;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

namespace Schema.Runtime
{
    public class Root : Node
    {
        public override bool _canHaveParent => false;
        public override int _maxChildren => 1;
    }
}