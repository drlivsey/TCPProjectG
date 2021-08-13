using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TCPToolkit;

[RequireComponent(typeof(TCPClient))]
public class TCPViewsRegistry : MonoBehaviour
{
    [SerializeField] private static TCPClient m_client = null;
    [SerializeField] private static List<TCPBaseView> m_views = new List<TCPBaseView>();
    [SerializeField] private static List<TCPBaseView> m_otherViews = new List<TCPBaseView>();

    private static int _idCounter = Mathf.Max(m_views.Count, _idCounter);

    private void Awake()
    {
        m_views = GameObject.FindObjectsOfType<TCPBaseView>().ToList();

        for (var i = 0; i < m_views.Count; i++)
        {
            m_views[i].SetViewID(i.ToString());
        }

        m_client = GetComponent<TCPClient>();
    }

    private void OnEnable() => m_client.onServerReceivedData += UpdateViewLocal;
    private void OnDisable() => m_client.onServerReceivedData -= UpdateViewLocal;

    private void UpdateViewLocal(TCPMessage message)
    {
        if (m_views.Any(x => x.ID == message.ViewID))
        {
            var view = m_views.First(x => x.ID == message.ViewID);
            view.UpdateViewLocal(message);
        }
    }

    public static void RegisterTCPView(TCPBaseView view)
    {
        m_otherViews.Add(view);        
    }

    public static void UnregisterTCPView(TCPBaseView view)
    {
        if (m_views.Contains(view))
        {
            m_views.Remove(view);
        }

        if (m_otherViews.Contains(view))
        {
            m_otherViews.Remove(view);
        }
    }

    public static void UpdateViewGlobal(TCPMessage message)
    {
        m_client.SendMessageToServer(message);
    }
}
