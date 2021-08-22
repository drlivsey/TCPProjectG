using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TcpToolkit
{
    [RequireComponent(typeof(Renderer))]
    public class TcpColorView : TcpBaseView
    {
        private Material _targetMaterial;
        private float _oldA = 0f;
        private float _oldR = 0f;
        private float _oldG = 0f;
        private float _oldB = 0f;

        private bool _isChanged => _oldA != _targetMaterial.color.a || _oldR != _targetMaterial.color.r ||
                                   _oldG != _targetMaterial.color.g || _oldB != _targetMaterial.color.b;

        private void Awake()
        {
            _targetMaterial = this.GetComponent<Renderer>().material;
            RememberColor(_targetMaterial.color);
        }

        protected void Update()
        {
            if (!m_isActive) return;

            if (!_isChanged) return;

            if (m_canInteract == UserAcess.Nobody)
                KeepValue();

            if (m_canInteract == UserAcess.Root && TcpClient.IsRoot == false)
                KeepValue();

            RememberColor(_targetMaterial.color);
        }

        protected void FixedUpdate()
        {
            if (!m_isActive) return;

            if (!_isChanged) return;

            UpdateViewGlobal();
        }

        public override void UpdateViewLocal(TcpMessage message)
        {
            base.UpdateViewLocal(message);

            try
            {
                var receivedColor = message.ToTcpObject() as TcpColor;
                
                if (receivedColor == null) return;

                var newColor = new Color(receivedColor.r, receivedColor.g, receivedColor.b, receivedColor.a);

                _targetMaterial.color = newColor;

                RememberColor(newColor);
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        protected override void UpdateViewGlobal()
        {
            var colorToSend = new TcpColor(_targetMaterial.color);

            var message = new TcpMessage(m_ID, TcpDataType.Color, TcpClient.ClientType, colorToSend.ToString());

            TcpViewsRegistry.UpdateViewGlobal(message);
        }

        protected override void KeepValue()
        {
            _targetMaterial.color = new Color(_oldR, _oldG, _oldB, _oldA);
        }

        private void RememberColor(Color color)
        {
            _oldA = color.a;
            _oldR = color.r;
            _oldG = color.g;
            _oldB = color.b;
        }
    }
}
