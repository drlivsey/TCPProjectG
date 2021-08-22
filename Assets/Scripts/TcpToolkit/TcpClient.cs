using System;
using System.Collections;
using UnityEngine;
using SimpleTcp;
using TMPro;
using UnityEngine.Events;

namespace TcpToolkit
{
    [RequireComponent(typeof(TcpViewsRegistry))]
    public class TcpClient : MonoBehaviour
    {
        [SerializeField] private TMP_InputField m_IP;
        [SerializeField] private string m_ID = string.Empty;
        public static TcpClient Client { get; private set; } = null;
        public static TcpSenderType ClientType { get; private set; } = TcpSenderType.Unset;
        public static bool IsRoot => ClientType == TcpSenderType.RootUser;
        
        public event UnityAction<TcpMessage> onServerReceivedData;
        public event UnityAction<TcpMessage> onUserReceivedData;
        public event UnityAction onServerConnecting;
        public event UnityAction onBeforeServerDisconnecting;
        public event UnityAction onServerDisconnecting;
        public event UnityAction onClientInitializing;
        public event UnityAction onUserJoinedRoom;
        public event UnityAction onUserLeftRoom;
        public string ID => m_ID;
        
        private SimpleTcpClient _client;

        private void Awake()
        {
            if (Client == null)
            {
                Client = this;
            }
            else
            {
                Destroy(gameObject);
            }

            _client = new SimpleTcpClient(m_IP.text);
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

        private IEnumerator AwaitInitializing()
        {
            SendMessageToServer(TcpMessage.IDRequest);
            while (m_ID == String.Empty)
            {
                yield return null;
            }
            
            SendMessageToServer(TcpMessage.RootRequest);
            while (ClientType == TcpSenderType.Unset)
            {
                yield return null;
            }
            
            onClientInitializing?.Invoke();
            
            var message = new TcpMessage("", TcpDataType.RelevantRequest, ClientType, "");
            
            SendMessageToServer(message);
        }

        private void OnConnected(object sender, ClientConnectedEventArgs e)
        {
            Debug.Log("CONNECTED!");
            onServerConnecting?.Invoke();

            StartCoroutine(AwaitInitializing());
        }

        private void OnDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Debug.Log($"DISCONNECTED! {e.Reason}");
            
            m_ID = String.Empty;
            ClientType = TcpSenderType.Unset;
            
            onServerDisconnecting?.Invoke();
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                var message = new TcpMessage(e.Data);
                
                if (message.Sender is TcpSenderType.Server)
                    onServerReceivedData?.Invoke(message);
                else
                    onUserReceivedData?.Invoke(message);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        private void ProcessServerMessage(TcpMessage message)
        {
            switch (message.MessageType)
            {
               case TcpDataType.ClientRoot:
                   ClientType = message.Data == TcpSenderType.RootUser.ToString() ? TcpSenderType.RootUser : TcpSenderType.User;
                   return;
               case TcpDataType.ClientId:
                   m_ID = message.Data;
                   return;
               case TcpDataType.Instantiate:
                   TcpCommandExecutor.AddExecutableCommand(TcpPrefabFactory.InstantiateTcpObject, message);
                   return;
               case TcpDataType.Destroy:
                   TcpCommandExecutor.AddExecutableCommand(TcpPrefabFactory.DestroyTcpObject, message);
                   return;
               case TcpDataType.UserJoined:
                   onUserJoinedRoom?.Invoke();
                   return;
               case TcpDataType.UserLeft:
                   onUserLeftRoom?.Invoke();
                   return;
            }
        }

        public void SendMessageToServer(TcpMessage message)
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
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public void Disconnect()
        {
            try
            {
                onBeforeServerDisconnecting?.Invoke();
                
                if (_client.IsConnected)
                    _client.Disconnect();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}