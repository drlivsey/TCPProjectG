using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TcpToolkit
{
    public static class TcpInstanceValidator 
    {
        public static bool ContainsInstance(this IEnumerable<TcpInstance> container, TcpInstance instance)
        {
            return container.Any(x => x.ObjectId == instance.ObjectId 
                                      && x.ObjectName == instance.ObjectName
                                      && x.ObjectOwnerID == instance.ObjectOwnerID);
        }
    }
}