using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCPToolkit;

public class TCPBaseView : MonoBehaviour
{
    [SerializeField] protected string _id;
    [SerializeField] protected TCPClient _client;
    [SerializeField] protected UserAcess _canInteract = UserAcess.All;
    [SerializeField] protected bool _isActive = false;

    public string ID  => _id;

    public void SetViewID(string id) => _id = id;

    protected virtual void Start()
    {
        if (!_client)
            _client = TCPClient.Client;
    }

    public virtual void UpdateLocalData(TCPMessage message)
    {
        if (_canInteract == UserAcess.Nobody)
            return;
        
        if (_canInteract == UserAcess.Root && message.Sender != TCPSenderType.RootUser)
            return;
    }

    protected virtual void UpdateGlobalData(){ }
    protected virtual void UpdateGlobalData(TCPMessage message)
    {
        _client.SendMessageToServer(message);
    }

    protected virtual void KeepValue(){ }
}
