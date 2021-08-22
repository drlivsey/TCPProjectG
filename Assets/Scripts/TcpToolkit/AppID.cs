using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppID : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text _id;

    private void Awake()
    {
        _id.text = SystemInfo.deviceName;
    }
}
