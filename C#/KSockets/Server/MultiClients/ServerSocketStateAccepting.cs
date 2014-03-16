//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSockets.Server.MultiClients
{
    internal class ServerSocketStateAccepting : AServerSocketState
    {
        public ServerSocketStateAccepting(ServerSocket serverSocket) : base(serverSocket)
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

            ServerSocket.CloseTcpListenerAndAllClients();
        }

        public override void StartRequested(int port)
        {
            throw new Exception("Already Started");
        }

        public override void CloseRequested()
        {
            ServerSocket.UpdateState(this, new ServerSocketStateIdle(ServerSocket));
        }

        public override void NotifyAccepted(System.Net.Sockets.TcpClient tcpClient)
        {
            ServerSocket.DispatchAccepted(tcpClient);
            ServerSocket.AcceptAsync();
        }

        public override void NotifyAcceptError(Exception e)
        {
            ServerSocket.DispatchError(e);
            ServerSocket.UpdateState(this, new ServerSocketStateAccepting(ServerSocket));
        }
    }
}
