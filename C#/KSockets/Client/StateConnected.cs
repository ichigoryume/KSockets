//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSockets.Client
{
    internal class StateConnected : AState 
    {
        public StateConnected(ClientSocket clientSocket)
            : base(clientSocket)
        {
        }

        public override void Enter()
        {
            base.Enter();

            ClientSocket.Receiver.Clear();
            ClientSocket.DispatchEventConnected();
            ClientSocket.ReceiveAsync();
        }

        public override void Leave()
        {
            base.Leave();
        }

        public override void StartRequested(String serverAddress, int serverPort)
        {
            throw new Exception("Already Connected.");
        }

        public override void CloseRequested()
        {
            ClientSocket.CloseTcpClient();
            ClientSocket.UpdateState(this, new StateIdle(ClientSocket));
        }

        public override void SendRequested(string message)
        {
            ClientSocket.Sender.Send(message, ClientSocket.TcpClient.GetStream());
        }

        public override void NotifyConnected()
        {
            // nothing to do
        }

        public override void NotifyConnectError(Exception e)
        {
            // nothing to do
        }

        public override void NotifyReceived(byte[] buffer, int receivedCount)
        {
            if (receivedCount <= 0)
            {
                ClientSocket.DispatchEventDisconnected();
                ClientSocket.CloseTcpClient();
                ClientSocket.UpdateState(this, new StateConnecting(ClientSocket));
                return;
            }

            String[] messages = ClientSocket.Receiver.Push(buffer, receivedCount);
            foreach(String message in messages)
            {
                ClientSocket.DispatchEventReceived(message);
            }

            ClientSocket.ReceiveAsync();
        }

        public override void NotifyReceiveError(Exception e)
        {
            ClientSocket.DispatchEventError(e);
            ClientSocket.DispatchEventDisconnected();

            ClientSocket.CloseTcpClient();
            ClientSocket.UpdateState(this, new StateConnecting(ClientSocket));
        }
    }
}
