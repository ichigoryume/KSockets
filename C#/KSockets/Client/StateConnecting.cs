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
    internal class StateConnecting : AState
    {
        public StateConnecting(ClientSocket clientSocket)
            : base(clientSocket)
        {

        }

        public override void Enter()
        {
            base.Enter();

            ClientSocket.CreateTcpClient();
            ClientSocket.ConnectAsync();
        }

        public override void Leave()
        {
            base.Leave();
        }

        public override void StartRequested(String serverAddress, int serverPort)
        {
            throw new Exception("Already Started");
        }

        public override void SendRequested(string message)
        {
            throw new Exception("Not Connected.");
        }

        public override void CloseRequested()
        {
            ClientSocket.CloseTcpClient();
            ClientSocket.UpdateState(this, new StateIdle(ClientSocket));
        }

        public override void NotifyConnected()
        {
            ClientSocket.UpdateState(this, new StateConnected(ClientSocket));
        }

        public override void NotifyConnectError(Exception e)
        {
            ClientSocket.DispatchEventError(e);
            ClientSocket.CloseTcpClient();
            ClientSocket.CreateTcpClient();
            ClientSocket.ConnectAsync();
        }

        public override void NotifyReceived(byte[] buffer, int receivedCount)
        {
        }

        public override void NotifyReceiveError(Exception e)
        {
        }
    }
}
