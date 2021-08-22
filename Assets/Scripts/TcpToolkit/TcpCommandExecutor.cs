using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TcpToolkit
{
    public class TcpCommandExecutor : MonoBehaviour
    {
        private static Queue<Tuple<Action<TcpMessage>, TcpMessage>> _updateActionsQueue = new Queue<Tuple<Action<TcpMessage>, TcpMessage>>();
        
        private void FixedUpdate()
        {
            lock (_updateActionsQueue)
            {
                if (_updateActionsQueue.Count == 0)
                    return;

                var (item1, item2) = _updateActionsQueue.Dequeue();
                
                Execute(item1, item2);
            }
        }

        public static void AddExecutableCommand(Action<TcpMessage> command, TcpMessage argument = null)
        {
            lock (_updateActionsQueue)
            {
                _updateActionsQueue.Enqueue(new Tuple<Action<TcpMessage>, TcpMessage>(command, argument));
            }
        }

        private void Execute(Action<TcpMessage> command, TcpMessage argument)
        {
            command?.Invoke(argument);
        }
    }
}