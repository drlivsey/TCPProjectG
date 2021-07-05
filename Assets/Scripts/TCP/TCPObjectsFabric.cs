using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCPToolkit;

public class TCPObjectsFabric : MonoBehaviour
{

    [SerializeField] private List<GameObject> _objectPrefabs = null;
    [HideInInspector] public static TCPObjectsFabric ObjectsFabric => _fabric;
    private static TCPObjectsFabric _fabric = null;

    private void Awake()
    {
        if (_fabric == null)
        {
            _fabric = this;
        }
        else if (_fabric == this)
        {
            Destroy(gameObject);
        }
    }

    
}
