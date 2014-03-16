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
    internal class StateIdle : AState
    {
        public StateIdle(ClientSocket clientSocket)
            : base(clientSocket)
        {
        }

        public override void Enter()
        {
            ClientSocket.ServerAddress  = null;
            ClientSocket.ServerPort     = 0;
        }

        public override void StartRequested(String serverAddress, int serverPort)
        {
            ClientSocket.ServerAddress  = serverAddress;
            ClientSocket.ServerPort     = serverPort;

            ClientSocket.UpdateState(this, new StateConnecting(ClientSocket));
        }

        public override void CloseRequested()
        {
            throw new Exception("Not Started");
        }

        public override void SendRequested(string message)
        {
            throw new Exception("Not Connected");
        }

        public override void NotifyConnected()
        {
        }

        public override void NotifyConnectError(Exception e)
        {
        }

        public override void NotifyReceived(byte[] buffer, int receivedCount)
        {
        }

        public override void NotifyReceiveError(Exception e)
        {
        }

    }
}
