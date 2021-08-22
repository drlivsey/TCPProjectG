using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TcpToolkit;

namespace TcpToolkit
{
    public class TcpInstanceView : TcpBaseView
    {
        [SerializeField] private GameObject m_prefab = null;
        [SerializeField] protected bool m_instantiateForAll = false;
        [SerializeField] protected bool m_destroyOnDisconnect = false;

        private GameObject _instance = null;
        
        public void Create()
        {
            if (m_instantiateForAll == false || m_prefab == null) return;

            InstantiateForAll();
        }

        private void OnDestroy()
        {
            if (m_destroyOnDisconnect)
                DestroyForAll();
        }

        private void InstantiateForAll()
        {
            TcpPrefabFactory.InstantiateForAllByName(m_prefab.name);
        }

        private void DestroyForAll()
        {
            TcpPrefabFactory.DestroyForAllByName(_instance.name);
        }
    }
}
