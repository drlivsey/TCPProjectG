using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;

namespace TCPToolkit
{
    public class TCPObject
    {
        public string objectId = string.Empty; 

        public TCPObject() {}
        public TCPObject(TCPMessage message)
        {
            objectId = message.ViewID;
        }
        
        new public virtual string ToString()
        {
            return base.ToString();
        }
    }

    public class TCPTransform : TCPObject
    {
        private Vector3 _objectPosition;
        private Quaternion _objectRotation;
        private Vector3 _objectScale;

        public Vector3 position => _objectPosition;
        public Quaternion rotation => _objectRotation;
        public Vector3 localScale => _objectScale;
        
        public TCPTransform() {}
        public TCPTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            _objectPosition = position;
            _objectRotation = rotation;
            _objectScale = scale;
        }
        public TCPTransform(Transform transform)
        {
            _objectScale = transform.localScale;
            _objectRotation = transform.rotation;
            _objectPosition = transform.position;
        }
        public TCPTransform(TCPMessage message) : base(message)
        {
            var splitedData = message.Data.Split(';').Select(x => float.Parse(x)).ToArray();

            _objectPosition = new Vector3(splitedData[0], splitedData[1], splitedData[2]);
            _objectRotation = new Quaternion(splitedData[3], splitedData[4], splitedData[5], splitedData[6]);
            _objectScale = new Vector3(splitedData[7], splitedData[8], splitedData[9]);
        }

        public override string ToString()
        {
            return $"{_objectPosition.x};{_objectPosition.y};{_objectPosition.z};" +
                $"{_objectRotation.x};{_objectRotation.y};{_objectRotation.z};{_objectRotation.w};" +
                $"{_objectScale.x};{_objectScale.y};{_objectScale.z}";
        }
    }

    public class TCPColor :TCPObject
    {
        private Color _color = Color.black;

        public Color Color => _color;
        public float a => _color.a;
        public float r => _color.r;
        public float g => _color.g;
        public float b => _color.b;
        
        public TCPColor() {}
        public TCPColor(Color color)
        {
            _color = color;
        }
        public TCPColor(float r, float g, float b, float a)
        {
            _color = new Color(r, g, b, a);
        }
        public TCPColor(float r, float g, float b)
        {
            _color = new Color(r, g, b);
        }
        public TCPColor(TCPMessage message) : base(message)
        {
            var splitedData = message.Data.Split(';').Select(x => float.Parse(x)).ToArray();

            _color = new Color(splitedData[0], splitedData[1], splitedData[2], splitedData[3]);
        }

        public override string ToString()
        {
            return $"{r};{g};{b};{a}";
        }
    }

    public class TCPMessage
    {
        private int _sender = 0;
        private string _id = string.Empty;
        private int _type = 0;
        private string _data = string.Empty;

        public TCPSenderType Sender 
        {
            get => (TCPSenderType)_sender;
            set => _sender = (int)value;
        }
        public string ViewID
        {
            get => _id;
            set => _id = value;
        }
        public TCPDataType MessageType 
        {
            get => (TCPDataType)_type;
            set => _type = (int)value;
        } 
        public string Data
        {
            get => _data;
            set => _data = value;
        }
        public TCPMessage() {}
        public TCPMessage(byte[] data)
        {
            var id_bytes = TrimZeros(data.Skip(0).Take(16).ToArray());
            var type_bytes = TrimZeros(data.Skip(16).Take(16).ToArray());
            var sender_bytes = TrimZeros(data.Skip(32).Take(16).ToArray());
            var value_bytes = TrimZeros(data.Skip(48).Take(1024).ToArray());

            _id = Encoding.UTF8.GetString(id_bytes);
            _type = int.Parse(Encoding.UTF8.GetString(type_bytes));
            _sender = int.Parse(Encoding.UTF8.GetString(sender_bytes));
            _data = Encoding.UTF8.GetString(value_bytes);
        }
        public TCPMessage(string viewID, TCPDataType type, TCPSenderType sender, string data) 
        {
            _id = viewID;
            _type = (int)type;
            _sender = (int)sender;
            _data = data;
        }

        public override string ToString() => $"{_id} {_type} {_sender} {_data}";
        public byte[] ByteMessage => ConverteToBytes();

        // Converte this message instance to TCPObject
        public TCPObject ToTCPObject()
        {
            switch ((TCPDataType)_type)
            {
                case TCPDataType.Transform:
                    return new TCPTransform(this);
                case TCPDataType.Color:
                    return new TCPColor(this);
                default:
                    return new TCPObject();
            }
        }

        // Convertes this message instance to byte array
        private byte[] ConverteToBytes()
        {
            var result_b = new byte[16 + 16 + 16 + 1024];

            var id_byte_value = Encoding.UTF8.GetBytes($"{_id}");
            Array.Reverse(id_byte_value);

            var type_byte_value = Encoding.UTF8.GetBytes($"{_type}");
            Array.Reverse(type_byte_value);

            var sender_byte_value = Encoding.UTF8.GetBytes($"{_sender}");
            Array.Reverse(sender_byte_value);

            var date_byte_value = Encoding.UTF8.GetBytes(_data);
            Array.Reverse(date_byte_value);

            for (var i = 0; i < id_byte_value.Length; i++)
            {
                result_b[15 - i] = id_byte_value[i];
            }
            for (var i = 0; i < type_byte_value.Length; i++)
            {
                result_b[31 - i] = type_byte_value[i];
            }
            for (var i = 0; i < sender_byte_value.Length; i++)
            {
                result_b[47 - i] = sender_byte_value[i];
            }
            for (var i = 0; i < date_byte_value.Length; i++)
            {
                result_b[1071 - i] = date_byte_value[i];
            }

            return result_b;
        }

        // Trim zeros in byte array and returns only significant bytes
        private byte[] TrimZeros(byte[] bytes)
        {
            var index = -1;
            for (var i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] != 0)
                {
                    index = i;
                    break;
                }
            }
            return bytes.Skip(index).ToArray();
        }
    }

    public enum TCPDataType
    {
        Signal = 0,
        Transform = 1,
        Color = 2,
        TextMessage = 3,
        State = 4,
        ClientRoot = 5,
        Instantiate = 6
    }

    public enum TCPSenderType
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
