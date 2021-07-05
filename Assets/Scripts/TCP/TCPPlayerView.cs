using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCPToolkit;

public class TCPPlayerView : TCPBaseView
{
    [SerializeField] private TCPTransform _leftHand;
    [SerializeField] private TCPTransform _rightHand;
    [SerializeField] private TCPTransform _body;
    [SerializeField] private List<TCPColor> _colors;

    public TCPTransform LeftHand => _leftHand;
    public TCPTransform RightHand => _rightHand;
    public List<TCPColor> Colors => _colors;

}
