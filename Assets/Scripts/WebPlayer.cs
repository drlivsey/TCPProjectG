using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TcpToolkit;

public class WebPlayer : MonoBehaviour
{
    [SerializeField] private GameObject m_prefab;
    
    private GameObject _instance = null;
    private WebPlayerAnatomy _anatomy = null;

    private void Start()
    {
        TcpClient.Client.onServerConnecting += AddListeners;
        TcpClient.Client.onServerDisconnecting += RemoveListeners;
    }

    private void FixedUpdate()
    {
        if (_instance == null) return;

        _instance.transform.position = this.transform.position;
        _anatomy.SetBodyPosition(transform.position);
    }

    private void OnDestroy()
    {
        TcpClient.Client.onServerConnecting -= AddListeners;
        TcpClient.Client.onServerDisconnecting -= RemoveListeners;
    }

    private void AddListeners()
    {
        TcpClient.Client.onClientInitializing += InstantiateForAll;
    }

    private void RemoveListeners()
    {
        TcpClient.Client.onClientInitializing -= InstantiateForAll;
    }

    private async void InstantiateForAll()
    {
        _instance = await TcpPrefabFactory.InstantiateForOtherByNameAsync(m_prefab.name);

        _anatomy = _instance.GetComponentInChildren<WebPlayerAnatomy>();
        
        var renderers = _instance.GetComponentInChildren<MeshRenderer>();
        renderers.enabled = false;
    }

    private void DestroyForAll()
    {
        TcpPrefabFactory.DestroyForAllByName(_instance.name);
    }
}
