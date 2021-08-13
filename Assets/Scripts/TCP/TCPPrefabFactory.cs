using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using TCPToolkit;

public class TCPPrefabFactory : MonoBehaviour
{
    [SerializeField] private static TCPClient m_client = null;
    private const string PREFABS_PATH = "TCPPrefabs/";

    private void Start()
    {
        if (!m_client)
            m_client = TCPClient.Client;
    }

    public static async void InstantiateByName(string name)
    {
        var prefab = Resources.LoadAsync<GameObject>(PREFABS_PATH + name);
        
        while (!prefab.isDone)
        {
            await Task.Yield();
        }

        Instantiate(prefab.asset as GameObject);
    }

    public static async void InstantiateTCPObjectByName(string senderID, string name)
    {
        var prefab = Resources.LoadAsync<GameObject>(PREFABS_PATH + name);
        
        while (!prefab.isDone)
        {
            await Task.Yield();
        }

        var instance = Instantiate(prefab.asset as GameObject);

        SetupTCPComponents(instance, senderID);
    }

    public static void InstantiateForAllByName(string name)
    {
        var message = new TCPMessage(m_client.ID, TCPDataType.Instantiate, TCPClient.ClientType, name);
        m_client.SendMessageToServer(message);
    }

    private static GameObject Instantiate(GameObject target)
    {
        return GameObject.Instantiate(target, Vector3.zero, Quaternion.identity);
    }

    private static void SetupTCPComponents(GameObject target, string senderID)
    {
        var views = target.GetComponentsInChildren<TCPBaseView>(true);

        for (var i = 1; i < views.Length; i++)
        {
            var view = views[i];
            view.SetViewID($"{senderID}_{i}");
            TCPViewsRegistry.RegisterTCPView(view);
        }
    }
}
