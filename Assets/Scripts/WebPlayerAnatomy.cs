using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebPlayerAnatomy : MonoBehaviour
{
    [SerializeField] private Transform m_body = null;

    public void SetBodyPosition(Vector3 position)
    {
        m_body.localPosition = position;
    }
}
