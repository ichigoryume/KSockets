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
    internal class ServerSocketStateIdle : AServerSocketState
    {
        public ServerSocketStateIdle(ServerSocket serverSocket) : base(serverSocket)
        {
        }


        public override void StartRequested(int port)
        {
            ServerSocket.Port = port;
            ServerSocket.UpdateState(this, new ServerSocketStateAccepting(ServerSocket));
        }

        public override void CloseRequested()
        {
            throw new Exception("Not Started");
        }

        public override void NotifyAccepted(System.Net.Sockets.TcpClient tcpClient)
        {
        }

        public override void NotifyAcceptError(Exception e)
        {
        }
    }
}
