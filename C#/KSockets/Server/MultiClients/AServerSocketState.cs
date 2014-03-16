//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

namespace KSockets.Server.MultiClients
{
    internal abstract class AServerSocketState
    {
        /*-------------------------------------------------------------------------------
         * Properties
         *-------------------------------------------------------------------------------*/
        protected ServerSocket ServerSocket { get; private set; }


        /*-------------------------------------------------------------------------------
         * Public Methods
         *-------------------------------------------------------------------------------*/
        public AServerSocketState(ServerSocket serverSocket)
        {
            ServerSocket = serverSocket;
        }

        public virtual void Enter()
        {
            //Console.WriteLine("Enter " + GetType().Name);
        }

        public virtual void Leave()
        {
            //Console.WriteLine("Leave " + GetType().Name);
        }


        /*-------------------------------------------------------------------------------
         * Request From User
         *-------------------------------------------------------------------------------*/
        public abstract void StartRequested(int port);
        public abstract void CloseRequested();


        /*-------------------------------------------------------------------------------
         * Notification from ServerSocket
         *-------------------------------------------------------------------------------*/
        public abstract void NotifyAccepted(TcpClient tcpClient);
        public abstract void NotifyAcceptError(Exception e);

    }
}
