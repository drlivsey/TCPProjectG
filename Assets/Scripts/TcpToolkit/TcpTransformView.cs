using System;
using System.Collections;
using UnityEngine;

namespace TcpToolkit
{
    public class TcpTransformView : TcpBaseView
    {
        private Vector3 _oldPosition;
        private Quaternion _oldRotation;
        private Vector3 _oldScale;
        
        private Transform _targetTransform;

        private bool _isChanged => transform.position != _oldPosition || transform.rotation != _oldRotation ||
                                   transform.localScale != _oldScale;

        private void Awake()
        {
            _targetTransform = this.transform;
            RememberTransform(_targetTransform);
        }

        protected void Update()
        {
            if (!m_isActive) return;

            if (!_isChanged) return;

            if (m_canInteract == UserAcess.Nobody)
                KeepValue();

            if (m_canInteract == UserAcess.Root && TcpClient.IsRoot == false)
                KeepValue();

            RememberTransform(_targetTransform);
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
                var receivedTransform = message.ToTcpObject() as TcpTransform;
                
                if (receivedTransform == null) return;

                _targetTransform.position = receivedTransform.Position;
                _targetTransform.rotation = receivedTransform.Rotation;
                _targetTransform.localScale = receivedTransform.LocalScale;
                    
                RememberTransform(_targetTransform);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        protected override void UpdateViewGlobal()
        {
            var transformToSend = new TcpTransform(this.transform);

            var message = new TcpMessage(m_ID, TcpDataType.Transform, TcpClient.ClientType, transformToSend.ToString());

            TcpViewsRegistry.UpdateViewGlobal(message);
        }

        protected override void KeepValue()
        {
            _targetTransform.position = _oldPosition;
            _targetTransform.rotation = _oldRotation;
            _targetTransform.localScale = _oldScale;
        }

        private void RememberTransform(Transform target)
        {
            _oldPosition = target.position;
            _oldRotation = target.rotation;
            _oldScale = target.localScale;
        }
    }
}