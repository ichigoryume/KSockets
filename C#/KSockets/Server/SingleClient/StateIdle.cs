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
    internal class StateIdle : AState
    {
        public StateIdle(ServerSocket serverSocket)
            : base(serverSocket)
        {
        }

        public override void Enter()
        {
            ServerSocket.Port = 0;
        }

        public override void StartRequested(int port)
        {
            ServerSocket.Port = port;
            ServerSocket.UpdateState(this, new StateAccepting(ServerSocket));
        }

        public override void CloseRequested()
        {
            throw new Exception("Not Started");
        }

        public override void SendRequested(string message)
        {
            throw new Exception("Not Accepted");
        }

        public override void NotifyAccepted()
        {
        }

        public override void NotifyAcceptError(Exception e)
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
