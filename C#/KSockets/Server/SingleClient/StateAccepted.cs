//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSockets.Server.SingleClient
{
    internal class StateAccepted : AState
    {
        public StateAccepted(ServerSocket serverSocket)
            : base(serverSocket)
        {
        }

        public override void Enter()
        {
            base.Enter();

            ServerSocket.Receiver.Clear();
            ServerSocket.DispatchAccepted();
            ServerSocket.ReceiveAsync();
        }

        public override void Leave()
        {
            base.Leave();
        }

        public override void StartRequested(int port)
        {
            throw new Exception("Already Connected");
        }

        public override void CloseRequested()
        {
            ServerSocket.CloseTcpListenerAndClient();
            ServerSocket.UpdateState(this, new StateIdle(ServerSocket));
        }

        public override void SendRequested(string message)
        {
            ServerSocket.Sender.Send(message, ServerSocket.TcpClient.GetStream());
        }

        public override void NotifyAccepted()
        {
        }

        public override void NotifyAcceptError(Exception e)
        {
        }

        public override void NotifyReceived(byte[] buffer, int receivedCount)
        {
            if (receivedCount == 0)
            {
                ServerSocket.DispatchDisconnected();
                ServerSocket.CloseTcpListenerAndClient();
                ServerSocket.UpdateState(this, new StateAccepting(ServerSocket));
                return;
            }

            String[] messages = ServerSocket.Receiver.Push(buffer, receivedCount);
            foreach (String message in messages)
            {
                ServerSocket.DispatchReceived(message);
            }

            ServerSocket.ReceiveAsync();
        }

        public override void NotifyReceiveError(Exception e)
        {
            ServerSocket.DispatchError(e);
            ServerSocket.DispatchDisconnected();

            ServerSocket.CloseTcpListenerAndClient();
            ServerSocket.UpdateState(this, new StateAccepting(ServerSocket));
        }
    }
}
