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
    internal abstract class AState
    {
        /*-------------------------------------------------------------------------------
         * Properties
         *-------------------------------------------------------------------------------*/
        protected ClientSocket ClientSocket { get; private set; }


        /*-------------------------------------------------------------------------------
         * Public Methods
         *-------------------------------------------------------------------------------*/
        public AState(ClientSocket clientSocket)
        {
            ClientSocket = clientSocket;
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
        public abstract void StartRequested(String serverAddress, int serverPort);
        public abstract void CloseRequested();
        public abstract void SendRequested(String message);


        /*-------------------------------------------------------------------------------
         * Notification from ClientSocket
         *-------------------------------------------------------------------------------*/
        public abstract void NotifyConnected();
        public abstract void NotifyConnectError(Exception e);

        public abstract void NotifyReceived(Byte[] buffer, int receivedCount);
        public abstract void NotifyReceiveError(Exception e);
    }
}
