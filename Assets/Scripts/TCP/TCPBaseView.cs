using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCPToolkit;

public class TCPBaseView : MonoBehaviour
{
    [SerializeField] protected string _id;
    [SerializeField] protected UserAcess _canInteract = UserAcess.All;
    [SerializeField] protected bool _isActive = false;

    public string ID  => _id;

    public void SetViewID(string id) => _id = id;

    public virtual void UpdateViewLocal(TCPMessage message)
    {
        if (_canInteract == UserAcess.Nobody)
            return;
        
        if (_canInteract == UserAcess.Root && message.Sender != TCPSenderType.RootUser)
            return;
    }

    protected virtual void UpdateViewGlobal() { }
    protected virtual void UpdateViewGlobal(TCPMessage message)
    {
        TCPViewsRegistry.UpdateViewGlobal(message);
    }

    protected virtual void KeepValue() { }
}
