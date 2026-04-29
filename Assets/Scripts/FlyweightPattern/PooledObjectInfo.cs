using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class PooledObjectInfo
    {
        public string LookupString;
        public List<GameObject> InactiveObjects = new();
        public List<GameObject> ActiveObjects = new();
    }
}

