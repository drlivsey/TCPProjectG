using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCPToolkit;

[RequireComponent(typeof(Renderer))]
public class TCPColorView : TCPBaseView
{
    private Renderer _renderer;
    private float _oldA = 0f;
    private float _oldR = 0f;
    private float _oldG = 0f;
    private float _oldB = 0f;

    private bool _isChanged => _oldA != _renderer.material.color.a || _oldR != _renderer.material.color.r || _oldG != _renderer.material.color.g || _oldB != _renderer.material.color.b;

    private void Awake()
    {
        _renderer = this.GetComponent<Renderer>();

        var color = _renderer.material.color;
        _oldA = color.a;
        _oldR = color.r;
        _oldG = color.g;
        _oldB = color.b;
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
        
        _oldA = _renderer.material.color.a;
        _oldR = _renderer.material.color.r;
        _oldG = _renderer.material.color.g;
        _oldB = _renderer.material.color.b;
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
            var receivedColor = message.ToTCPObject() as TCPColor;
            
            var newColor = new Color(receivedColor.r, receivedColor.g, receivedColor.b, receivedColor.a);

            _renderer.material.color = newColor;

            _oldA = newColor.a;
            _oldR = newColor.r;
            _oldG = newColor.g;
            _oldB = newColor.b;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    protected override void UpdateGlobalData()
    {
        var colorToSend = new TCPColor(_renderer.material.color);

        var message = new TCPMessage(_id, TCPDataType.Color, _client.ClientType, colorToSend.ToString());

        _client.SendMessageToServer(message);
    }

    protected override void KeepValue()
    {
        _renderer.material.color = new Color(_oldR, _oldG, _oldB, _oldA);
    }
}
