using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TcpToolkit
{
    [RequireComponent(typeof(TcpClient))]
    public class TcpInstancesRegistry : MonoBehaviour
    {
        private static List<TcpInstance> _tcpInstances = new List<TcpInstance>();
        private static Queue<TcpInstance> _tcpInstancesToUnregister = new Queue<TcpInstance>();

        private void FixedUpdate()
        {
            lock (_tcpInstancesToUnregister)
            {
                if (_tcpInstancesToUnregister.Count == 0)
                    return;

                var target = _tcpInstancesToUnregister.Dequeue();
                
                DestroyInstance(target);
                _tcpInstances.Remove(target);
            }
        }

        private void OnEnable()
        {
            TcpClient.Client.onBeforeServerDisconnecting += DestroyOnDisconnect;
        }

        private void OnDisable()
        {
            TcpClient.Client.onBeforeServerDisconnecting -= DestroyOnDisconnect;
        }

        public static bool TryGetInstance(out GameObject instance, string name)
        {
            instance = null;
            
            if (_tcpInstances.Any(x => x.IsMine && x.ObjectName.Equals(name)) == false)
                return false;

            instance = _tcpInstances.First(x => x.IsMine && x.ObjectName.Equals(name)).RootGameObject;
            return true;
        }

        public static void RegisterInstance(TcpInstance instance)
        {
            _tcpInstances.Add(instance);
        }

        public static void UnregisterInstance(TcpInstance instance)
        {
            if (_tcpInstances.ContainsInstance(instance) == false)
                return;
            
            lock (_tcpInstancesToUnregister)
            {
                _tcpInstancesToUnregister.Enqueue(instance);
            }
        }

        private void DestroyOnDisconnect()
        {
            lock (_tcpInstancesToUnregister)
            {
                foreach (var instance in _tcpInstances)
                {
                    _tcpInstancesToUnregister.Enqueue(instance);
                }
            }
        }

        private static void DestroyInstance(TcpInstance instance)
        {
            if (_tcpInstances.ContainsInstance(instance) == false)
                return;
            
            if (instance.IsMine)
                TcpPrefabFactory.DestroyForAllByName(instance.RootGameObject.name);
            
            var views = instance.RootGameObject.GetComponentsInChildren<TcpBaseView>(true);

            foreach (var view in views)
            {
                TcpViewsRegistry.UnregisterTcpView(view);
            }
            
            Destroy(instance.RootGameObject);
        }
    }
}