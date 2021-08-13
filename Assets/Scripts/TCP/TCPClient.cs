using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleTcp;
using TMPro;
using System.Linq;
using System.Text;
using UnityEngine.Events;
using TCPToolkit;

public class TCPClient : MonoBehaviour
{
    [SerializeField] private TMP_InputField _ip;
    [SerializeField] private string _id = string.Empty;
    [SerializeField] private static TCPSenderType _clientType = 0;

    public static TCPClient Client => _unityClient;
    private static TCPClient _unityClient = null;

    private SimpleTcpClient _client;

    public string ID => _id;
    public static TCPSenderType ClientType => _clientType;
    public static bool IsRoot => _clientType == TCPSenderType.RootUser;
    public event UnityAction<TCPMessage> onServerReceivedData;
    public event UnityAction<TCPMessage> onUserReceivedData;

    private void Awake()
    {
        if (_unityClient == null) 
        { 
            _unityClient = this;
        } 
        else
        {
            Destroy(gameObject);
        }

        _id = GenRandomString(6);
        _client = new SimpleTcpClient(_ip.text);
    }

    private void OnEnable()
    {
        _client.Events.Connected += OnConnected;
        _client.Events.Disconnected += OnDisconnected;
        _client.Events.DataReceived += OnDataReceived;
        onServerReceivedData += ProcessServerMessage;
    }

    private void OnDisable()
    {
        _client.Disconnect();

        _client.Events.Connected -= OnConnected;
        _client.Events.Disconnected -= OnDisconnected;
        _client.Events.DataReceived -= OnDataReceived;

        onServerReceivedData -= ProcessServerMessage;
    }

    private void OnConnected(object sender, ClientConnectedEventArgs e)
    {
        Debug.Log("CONNECTED!");
    }

    private void OnDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        Debug.Log($"DISCONNECTED! {e.Reason}");
    }

    private void OnDataReceived(object sender, DataReceivedEventArgs e)
    {
        try 
        {
            var message = new TCPMessage(e.Data);

            if (message.Sender == TCPSenderType.Server)
                onServerReceivedData?.Invoke(message);
            else
                onUserReceivedData?.Invoke(message);
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void ProcessServerMessage(TCPMessage message)
    {
        if (message.MessageType == TCPDataType.ClientRoot)
        {
            if (message.Data == "root")
                _clientType = TCPSenderType.RootUser;
            else
                _clientType = TCPSenderType.User;
        }
    }

    public void SendMessageToServer(TCPMessage message)
    {
        if (_client.IsConnected)
        {
            _client.Send(message.ByteMessage);
        }
    }

    public void Connect()
    {
        try 
        {
            _client.Connect();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void Disconnect()
    {
        try 
        {
            if (_client.IsConnected)
                _client.Disconnect();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private string GenRandomString(int length)
    {
        System.Random rnd = new System.Random();
        var alphabet = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm";
        string sb = string.Empty;
        
        for (int i = 0; i < length; i++)
        {
            sb += alphabet[rnd.Next(0, alphabet.Length-1)];                
        }

        return sb.ToString();
    }
}