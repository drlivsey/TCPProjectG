using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TcpToolkit
{

    public class TcpBaseView : MonoBehaviour
    {
        [SerializeField] protected string m_ID;
        [SerializeField] protected UserAcess m_canInteract = UserAcess.All;
        [SerializeField] protected bool m_isActive = false;

        public string ID => m_ID;

        public void SetViewID(string id) => m_ID = id;

        public virtual void UpdateViewLocal(TcpMessage message)
        {
            if (m_canInteract == UserAcess.Nobody)
                return;

            if (m_canInteract == UserAcess.Root && message.Sender != TcpSenderType.RootUser)
                return;
        }

        protected virtual void UpdateViewGlobal()
        {
        }

        protected virtual void UpdateViewGlobal(TcpMessage message)
        {
            TcpViewsRegistry.UpdateViewGlobal(message);
        }

        protected virtual void KeepValue()
        {
        }
    }
}