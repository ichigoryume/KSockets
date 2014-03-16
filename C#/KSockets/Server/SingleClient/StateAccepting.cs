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
    internal class StateAccepting : AState
    {
        public StateAccepting(ServerSocket serverSocket)
            : base(serverSocket)
        {
        }

        public override void Enter()
        {
            base.Enter();

            ServerSocket.CreateTcpListener();
            ServerSocket.AcceptAsync();
        }

        public override void Leave()
        {
            base.Leave();
        }

        public override void StartRequested(int port)
        {
            throw new Exception("Already Started");
        }

        public override void SendRequested(string message)
        {
            throw new Exception("Not Accepted");
        }

        public override void CloseRequested()
        {
            ServerSocket.CloseTcpListenerAndClient();
            ServerSocket.UpdateState(this, new StateIdle(ServerSocket));
        }

        public override void NotifyAccepted()
        {
            ServerSocket.UpdateState(this, new StateAccepted(ServerSocket));
        }

        public override void NotifyAcceptError(Exception e)
        {
            ServerSocket.DispatchError(e);
            ServerSocket.CloseTcpListenerAndClient();
            ServerSocket.UpdateState(this, new StateAccepting(ServerSocket));
        }

        public override void NotifyReceived(byte[] buffer, int receivedCount)
        {
        }

        public override void NotifyReceiveError(Exception e)
        {
        }
    }
}
