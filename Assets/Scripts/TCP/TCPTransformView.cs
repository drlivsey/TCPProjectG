﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCPToolkit;
using System.Linq;

public class TCPTransformView : TCPBaseView
{
    private Vector3 _oldPosition;
    private Quaternion _oldRotation;
    private Vector3 _oldScale;

    private bool _isChanged => transform.localPosition != _oldPosition || transform.localRotation != _oldRotation || transform.localScale != _oldScale;

    private void Awake()
    {
        _oldPosition = transform.position;
        _oldRotation = transform.rotation;
        _oldScale = transform.localScale;
    }

    protected void Update()
    {
        if (!_isActive)
            return;

        if (!_isChanged)
            return;

        if (_canInteract == UserAcess.Nobody)
            KeepValue();
        
        if (_canInteract == UserAcess.Root && _client.ClientType != TCPSenderType.RootUser)
            KeepValue();

        _oldPosition = transform.position;
        _oldRotation = transform.rotation;
        _oldScale = transform.localScale;
    }

    protected void FixedUpdate()
    {
        if (!_isActive)
            return;

        if (!_isChanged)
            return;

        UpdateGlobalData();                  
    }

    public override void UpdateLocalData(TCPMessage message)
    {
        base.UpdateLocalData(message);

        try
        {
            var receivedTransform = message.ToTCPObject() as TCPTransform;
            
            this.transform.position = receivedTransform.position;
            this.transform.rotation = receivedTransform.rotation;
            this.transform.localScale = receivedTransform.localScale;

            _oldPosition = transform.localPosition;
            _oldRotation = transform.localRotation;
            _oldScale = transform.localScale;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    protected override void UpdateGlobalData()
    {
        var transformToSend = new TCPTransform(this.transform);

        var message = new TCPMessage(_id, TCPDataType.Transform, _client.ClientType, transformToSend.ToString());

        _client.SendMessageToServer(message);
    }

    protected override void KeepValue()
    {
        this.transform.position = _oldPosition;
        this.transform.rotation = _oldRotation;
        this.transform.localScale = _oldScale;
    }
}