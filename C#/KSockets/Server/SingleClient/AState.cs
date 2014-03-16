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
    internal abstract class AState
    {
        /*-------------------------------------------------------------------------------
         * Properties
         *-------------------------------------------------------------------------------*/
        protected ServerSocket ServerSocket { get; private set; }


        /*-------------------------------------------------------------------------------
         * Public Methods
         *-------------------------------------------------------------------------------*/
        public AState(ServerSocket serverSocket)
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
        public abstract void SendRequested(String message);


        /*-------------------------------------------------------------------------------
         * Notification from ServerSocket
         *-------------------------------------------------------------------------------*/
        public abstract void NotifyAccepted();
        public abstract void NotifyAcceptError(Exception e);

        public abstract void NotifyReceived(Byte[] buffer, int receivedCount);
        public abstract void NotifyReceiveError(Exception e);
    }
}
