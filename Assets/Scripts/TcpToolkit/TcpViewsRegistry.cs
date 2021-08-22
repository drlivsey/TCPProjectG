using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TcpToolkit
{
    [RequireComponent(typeof(TcpCommandExecutor))]
    public class TcpViewsRegistry : MonoBehaviour
    {
        private static TcpClient m_client = null;
        private static List<TcpBaseView> m_staticViews = new List<TcpBaseView>();
        private static List<TcpBaseView> m_dinamycViews = new List<TcpBaseView>();

        private void Awake()
        {
            m_staticViews = GameObject.FindObjectsOfType<TcpBaseView>().ToList();

            for (var i = 0; i < m_staticViews.Count; i++)
            {
                m_staticViews[i].SetViewID(i.ToString());
            }

            m_client = GetComponent<TcpClient>();
        }

        private void OnEnable() => m_client.onUserReceivedData += UpdateViewLocal;
        private void OnDisable() => m_client.onUserReceivedData -= UpdateViewLocal;

        private void UpdateViewLocal(TcpMessage message)
        {
            if (m_staticViews.Any(x => x.ID == message.ViewID))
            {
                var view = m_staticViews.First(x => x.ID == message.ViewID);
                TcpCommandExecutor.AddExecutableCommand(view.UpdateViewLocal, message);
                return;
            }

            if (m_dinamycViews.Any(x => x.ID == message.ViewID))
            {
                var view = m_dinamycViews.First(x => x.ID == message.ViewID);
                TcpCommandExecutor.AddExecutableCommand(view.UpdateViewLocal, message);
            }
        }

        public static void RegisterTcpView(TcpBaseView view)
        {
            m_dinamycViews.Add(view);
        }

        public static void UnregisterTcpView(TcpBaseView view)
        {
            if (m_dinamycViews.Contains(view))
            {
                m_dinamycViews.Remove(view);
            }
        }

        public static void UpdateViewGlobal(TcpMessage message)
        {
            m_client.SendMessageToServer(message);
        }
    }
}
