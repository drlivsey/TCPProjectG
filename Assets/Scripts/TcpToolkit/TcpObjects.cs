using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;

namespace TcpToolkit
{
    public class TcpObject
    {
        public new virtual string ToString()
        {
            return base.ToString();
        }
    }

    public class TcpTransform : TcpObject
    {
        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        public Vector3 LocalScale { get; }

        public TcpTransform() {}
        public TcpTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.Position = position;
            this.Rotation = rotation;
            LocalScale = scale;
        }
        public TcpTransform(Transform transform)
        {
            LocalScale = transform.localScale;
            Rotation = transform.rotation;
            Position = transform.position;
        }
        public TcpTransform(TcpMessage message)
        {
            var splitedData = message.Data.Split(';').Select(x => float.Parse(x)).ToArray();

            Position = new Vector3(splitedData[0], splitedData[1], splitedData[2]);
            Rotation = new Quaternion(splitedData[3], splitedData[4], splitedData[5], splitedData[6]);
            LocalScale = new Vector3(splitedData[7], splitedData[8], splitedData[9]);
        }

        public override string ToString()
        {
            return $"{Position.x};{Position.y};{Position.z};" +
                $"{Rotation.x};{Rotation.y};{Rotation.z};{Rotation.w};" +
                $"{LocalScale.x};{LocalScale.y};{LocalScale.z}";
        }
    }

    public class TcpColor :TcpObject
    {
        private readonly Color _color = Color.black;

        public Color Color => _color;
        public float a => _color.a;
        public float r => _color.r;
        public float g => _color.g;
        public float b => _color.b;
        
        public TcpColor() {}
        public TcpColor(Color color)
        {
            _color = color;
        }
        public TcpColor(float r, float g, float b, float a)
        {
            _color = new Color(r, g, b, a);
        }
        public TcpColor(float r, float g, float b)
        {
            _color = new Color(r, g, b);
        }
        public TcpColor(TcpMessage message)
        {
            var splitedData = message.Data.Split(';').Select(float.Parse).ToArray();

            _color = new Color(splitedData[0], splitedData[1], splitedData[2], splitedData[3]);
        }

        public override string ToString()
        {
            return $"{r};{g};{b};{a}";
        }
    }
    
    public class TcpInstance : TcpObject
    {
        public string ObjectOwnerID { get; private set; } = String.Empty;
        public string ObjectId { get; private set; } = String.Empty;
        public string ObjectName { get; private set; } = String.Empty;
        
        public GameObject RootGameObject { get; private set; }

        public bool IsMine => ObjectOwnerID == TcpClient.Client.ID;
        
        public TcpInstance(TcpMessage message)
        {
            var splitedData = message.Data.Split(';').ToArray();

            (ObjectOwnerID, ObjectId, ObjectName) = (splitedData[0].Trim(), splitedData[1].Trim(), splitedData[2].Trim());
        }

        public void SetRootObject(GameObject root)
        {
            if (RootGameObject != null)
                return;
            
            RootGameObject = root;
        }
    }

    public class TcpMessage
    {
        private int _sender = 0;
        private int _type = 0;
        public string ViewID { get; private set; } = string.Empty;
        public string ClientID { get; private set; } = string.Empty;
        public TcpSenderType Sender 
        {
            get => (TcpSenderType)_sender;
            set => _sender = (int)value;
        }
        public TcpDataType MessageType 
        {
            get => (TcpDataType)_type;
            set => _type = (int)value;
        } 
        public string Data { get; private set; } = string.Empty;

        public TcpMessage() {}
        public TcpMessage(byte[] data)
        {
            var clientIdBytes = TrimZeros(data.Skip(0).Take(16).ToArray());
            var idBytes = TrimZeros(data.Skip(16).Take(16).ToArray());
            var typeBytes = TrimZeros(data.Skip(32).Take(16).ToArray());
            var senderBytes = TrimZeros(data.Skip(48).Take(16).ToArray());
            var valueBytes = TrimZeros(data.Skip(64).Take(1024).ToArray());

            ClientID = Encoding.UTF8.GetString(clientIdBytes);
            ViewID = Encoding.UTF8.GetString(idBytes);
            _type = int.Parse(Encoding.UTF8.GetString(typeBytes));
            _sender = int.Parse(Encoding.UTF8.GetString(senderBytes));
            Data = Encoding.UTF8.GetString(valueBytes);
        }
        public TcpMessage(string viewID, TcpDataType type, TcpSenderType sender, string data) 
        {
            (ClientID, ViewID, _type, _sender, Data) = (TcpClient.Client.ID, viewID, (int)type, (int)sender, data);
        }

        public override string ToString() => $"{ClientID} {ViewID} {_type} {_sender} {Data}";
        public byte[] ByteMessage => ConvertToBytes();
        
        public static readonly TcpMessage IDRequest = new TcpMessage("", TcpDataType.ClientId, TcpSenderType.Unset, "");
        public static readonly TcpMessage RootRequest = new TcpMessage("", TcpDataType.ClientRoot, TcpSenderType.Unset, "");

        // Convert this message instance to TcpObject
        public TcpObject ToTcpObject()
        {
            return (TcpDataType) _type switch
            {
                TcpDataType.Transform => new TcpTransform(this),
                TcpDataType.Color => new TcpColor(this),
                TcpDataType.Instantiate => new TcpInstance(this),
                TcpDataType.Destroy => new TcpInstance(this),
                _ => new TcpObject()
            };
        }

        // Converts this message instance to byte array
        private byte[] ConvertToBytes()
        {
            var messageBytes = new byte[16 + 16 + 16 + 16 + 1024];

            var clientIdByteValue = Encoding.UTF8.GetBytes($"{ClientID}");
            Array.Reverse(clientIdByteValue);
            
            var idByteValue = Encoding.UTF8.GetBytes($"{ViewID}");
            Array.Reverse(idByteValue);

            var typeByteValue = Encoding.UTF8.GetBytes($"{_type}");
            Array.Reverse(typeByteValue);

            var senderByteValue = Encoding.UTF8.GetBytes($"{_sender}");
            Array.Reverse(senderByteValue);

            var dateByteValue = Encoding.UTF8.GetBytes(Data);
            Array.Reverse(dateByteValue);

            for (var i = 0; i < clientIdByteValue.Length; i++)
            {
                messageBytes[15 - i] = clientIdByteValue[i];
            }
            for (var i = 0; i < idByteValue.Length; i++)
            {
                messageBytes[31 - i] = idByteValue[i];
            }
            for (var i = 0; i < typeByteValue.Length; i++)
            {
                messageBytes[47 - i] = typeByteValue[i];
            }
            for (var i = 0; i < senderByteValue.Length; i++)
            {
                messageBytes[63 - i] = senderByteValue[i];
            }
            for (var i = 0; i < dateByteValue.Length; i++)
            {
                messageBytes[1087 - i] = dateByteValue[i];
            }

            return messageBytes;
        }

        // Trim zeros in byte array and returns only significant bytes
        private byte[] TrimZeros(byte[] bytes)
        {
            var index = -1;
            for (var i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0)
                    continue;
                
                index = i;
                break;
            }
            return bytes.Skip(index).ToArray();
        }
    }

    public enum TcpDataType : uint
    {
        Signal = 0,
        Transform = 1,
        Color = 2,
        TextMessage = 3,
        State = 4,
        ClientRoot = 5,
        ClientId = 6,
        Instantiate = 7,
        Destroy = 8,
        RelevantRequest = 9,
        UserJoined = 10,
        UserLeft = 11
    }

    public enum TcpSenderType : uint
    {
        Unset = 0,
        Server = 1,
        User = 2, 
        RootUser = 3
    }

    public enum UserAcess
    {
        Nobody = 0,
        Root = 1,
        All = 2
    }
}
