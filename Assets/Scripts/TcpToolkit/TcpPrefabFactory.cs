using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace TcpToolkit
{
    [RequireComponent(typeof(TcpInstancesRegistry))]
    public class TcpPrefabFactory : MonoBehaviour
    {
        private const string PrefabsPath = "TCPPrefabs/";

        public static async void InstantiateTcpObject(TcpMessage message)
        {
            var tcpObject = message.ToTcpObject() as TcpInstance;
            
            var prefab = Resources.LoadAsync<GameObject>(PrefabsPath + tcpObject.ObjectName);
                
            while (!prefab.isDone)
            {
                await Task.Yield();
            }
        
            var instance = Instantiate(prefab.asset as GameObject);
            
            instance.name = $"{tcpObject.ObjectName}";
            
            SetupTcpComponents(instance, tcpObject.ObjectId);
            
            instance.name = $"[{tcpObject.ObjectId}] {tcpObject.ObjectName}";
            
            tcpObject.SetRootObject(instance);
            
            TcpInstancesRegistry.RegisterInstance(tcpObject);
        }

        public static void DestroyTcpObject(TcpMessage message)
        {
            var tcpObject = message.ToTcpObject() as TcpInstance;
            
            Debug.Log($"Destroy request [{tcpObject.ObjectId}] {tcpObject.ObjectName}");
            
            var target = GameObject.Find($"[{tcpObject.ObjectId}] {tcpObject.ObjectName}");
            
            if (target == null) return;
            
            Debug.Log("Find!");

            tcpObject.SetRootObject(target);
            
            TcpInstancesRegistry.UnregisterInstance(tcpObject);
        }

        public static void InstantiateForAllByName(string name)
        {
            var message = new TcpMessage(TcpClient.Client.ID, TcpDataType.Instantiate, TcpClient.ClientType, name);
            TcpClient.Client.SendMessageToServer(message);
        }
        
        public static async Task<GameObject> InstantiateForOtherByNameAsync(string name)
        {
            var message = new TcpMessage(TcpClient.Client.ID, TcpDataType.Instantiate, TcpClient.ClientType, name);
            TcpClient.Client.SendMessageToServer(message);

            do
            {
                if (TcpInstancesRegistry.TryGetInstance(out var instance, name))
                    return instance;
                else
                    await Task.Yield();
            } while (true);
        }

        public static void DestroyForAllByName(string name)
        {
            var message = new TcpMessage(TcpClient.Client.ID, TcpDataType.Destroy, TcpClient.ClientType, name);
            TcpClient.Client.SendMessageToServer(message);
        }

        private static GameObject Instantiate(GameObject target)
        {
            return GameObject.Instantiate(target, Vector3.zero, Quaternion.identity);
        }

        private static void SetupTcpComponents(GameObject target, string senderID)
        {
            var views = target.GetComponentsInChildren<TcpBaseView>(true);
            var counter = 0;
            foreach (var view in views)
            {
                counter++;
                view.SetViewID($"{view.gameObject.name}_{senderID}_{counter}");
                TcpViewsRegistry.RegisterTcpView(view);
            }
        }
    }
}
