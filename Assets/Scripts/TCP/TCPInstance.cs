using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCPToolkit;

public class TCPInstance : MonoBehaviour
{
    [SerializeField] private GameObject m_prefab = null;
    [SerializeField] protected bool _instantiateForAll = false;
    [SerializeField] protected bool _destroyOnDisconnect = false;

    private void Start() 
    {
        if (_instantiateForAll == false || m_prefab == null)
            return;
        
        InstantiateForAll();
    }

    private void OnDestroy() 
    {
        if (_destroyOnDisconnect)
            DestroyForAll();
    }

    private void InstantiateForAll()
    {
        
    }

    private void DestroyForAll()
    {

    }
}
