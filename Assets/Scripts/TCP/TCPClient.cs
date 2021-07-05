using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleTcp;
using TMPro;
using System.Linq;
using System.Text;
using TCPToolkit;

public class TCPClient : MonoBehaviour
{
    [SerializeField] private TMP_InputField _ip;
    [SerializeField] private List<TCPBaseView> _views;

    public static TCPClient Client => null;
    private static TCPClient _unityClient = null;

    private SimpleTcpClient _client;
    [SerializeField] private TCPSenderType _clientType = 0;
    private Queue<TCPMessage> _localData;
    private Queue<TCPMessage> _serverMessages;
    private bool _updatingNow = false;
    [SerializeField] private string _id = string.Empty;

    public string ID => _id;
    public TCPSenderType ClientType => _clientType;
    public bool IsRoot => _clientType == TCPSenderType.RootUser;

    private void Awake()
    {
        if (_unityClient == null) 
        { 
            _unityClient = this; 
        } 
        else if(_unityClient == this)
        { 
            Destroy(gameObject);
        }

        _views = GameObject.FindObjectsOfType<TCPBaseView>().ToList();
        _id = GenRandomString(6);

        for (var i = 0; i < _views.Count; i++)
        {
            _views[i].SetViewID(i.ToString());
        }
    }

    private void Start()
    {
        _client = new SimpleTcpClient(_ip.text);
        _client.Events.Connected += Connected;
        _client.Events.Disconnected += Disconnected;
        _client.Events.DataReceived += DataReceived;

        _localData = new Queue<TCPMessage>();
        _serverMessages = new Queue<TCPMessage>();
    }

    private void FixedUpdate()
    {
        if (_updatingNow)
            return;
        
        UpdateViews();
        ProcessServerMessages();
    }

    private void Connected(object sender, ClientConnectedEventArgs e)
    {
        Debug.Log("CONNECTED!");
    }

    private void Disconnected(object sender, ClientDisconnectedEventArgs e)
    {
        Debug.Log($"DISCONNECTED! {e.Reason}");
    }

    private void DataReceived(object sender, DataReceivedEventArgs e)
    {
        try 
        {
            var message = new TCPMessage(e.Data);

            if (message.Sender == TCPSenderType.Server)
                _serverMessages.Enqueue(message);
            else
                _localData.Enqueue(message);
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void UpdateViews()
    {
        _updatingNow = true;
        while (_localData.Count != 0)
        {
            var message = _localData.Dequeue();
            foreach (var view in _views)
            {
                if (view.ID != message.ViewID)
                    continue;

                view.UpdateLocalData(message);
                break;
            }
        }
        _updatingNow = false;
    }

    private void ProcessServerMessages()
    {
        while (_serverMessages.Count != 0)
        {
            var message = _serverMessages.Dequeue();

            if (message.MessageType == TCPDataType.ClientRoot)
            {
                if (message.Data == "root")
                    _clientType = TCPSenderType.RootUser;
                else
                    _clientType = TCPSenderType.User;
            }
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

    private void OnDisable()
    {
        _client.Disconnect();
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