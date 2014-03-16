//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Threading;
using System.Net.Sockets;

using KSockets.Sender;
using KSockets.Receiver;

namespace KSockets.Server.MultiClients
{
    public class Client
    {
        /*-------------------------------------------------------------------------------
         * Events
         *-------------------------------------------------------------------------------*/
        public delegate void ReceivedHandler(Client client, String message);
        public delegate void DisconnectedHandler(Client client);
        public delegate void ErrorHandler(Client client, String errorMessage);

        public event ReceivedHandler Received;
        public event DisconnectedHandler Disconnected;
        public event ErrorHandler Error;


        /*-------------------------------------------------------------------------------
         * Public Properties
         *-------------------------------------------------------------------------------*/        
        public String Address { get; private set; }


        /*-------------------------------------------------------------------------------
         * Internal Properties
         *-------------------------------------------------------------------------------*/
        internal ServerSocket ServerSocket { get; private set; }
        internal TcpClient TcpClient { get; set; }
        internal ISender Sender { get; set; }
        internal IReceiver Receiver { get; set; }


        /*-------------------------------------------------------------------------------
         * Private Properties
         *-------------------------------------------------------------------------------*/
        private Dispatcher Dispatcher { get; set; }
        private AClientState State { get; set; }

        private Byte[] buffer = new Byte[1024];


        /*-------------------------------------------------------------------------------
         * Public Methods
         *-------------------------------------------------------------------------------*/
        public void Send(String message)
        {
            State.SendRequested(message);
        }

        public void Close()
        {
            State.CloseRequested();
        }


        /*-------------------------------------------------------------------------------
         * Internal Methods
         *-------------------------------------------------------------------------------*/
        internal Client(ServerSocket serverSocket, TcpClient tcpClient, ISender sender, IReceiver receiver)
        {
            ServerSocket = serverSocket;
            Dispatcher = ServerSocket.Dispatcher;

            TcpClient   = tcpClient;
            Sender      = sender;
            Receiver    = receiver;

            Address = TcpClient.Client.RemoteEndPoint.ToString();

            State = new ClientStateConnected(this);
            State.Enter();
        }

        internal void UpdateState(AClientState requester, AClientState nextState)
        {
            if (State != requester)
            {
                return;
            }

            State.Leave();
            State = nextState;
            State.Enter();
        }

        internal async void ReceiveAsync()
        {
            try
            {
                int receiveBytes = await TcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);
                State.NotifyReceived(buffer, receiveBytes);
            }
            catch (Exception e)
            {
                State.NotifyReceiveError(e);
            }
        }

        internal void CloseTcpClientAndRemoveFromServerSocket()
        {
            if (TcpClient != null)
            {
                TcpClient.Close();
                TcpClient = null;
            }

            if (ServerSocket.ClientList.Contains(this))
            {
                ServerSocket.ClientList.Remove(this);
            }
        }

        // event dispatch method 群
        internal void DispatchReceived(String message)
        {
            Dispatcher.BeginInvoke(new Action(() => OnReceived(message)), null);
        }

        internal void DispatchDisconnected()
        {
            Dispatcher.BeginInvoke(new Action(() => OnDisconnected()), null);
        }

        internal void DispatchError(Exception e)
        {
            Dispatcher.BeginInvoke(new Action(() => OnError(e.Message)), null);
        }

        /*-------------------------------------------------------------------------------
         * Private Methods
         *-------------------------------------------------------------------------------*/
        private void OnReceived(String message)
        {
            if (Received == null)
            {
                return;
            }

            try
            {
                Received(this, message);
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        private void OnDisconnected()
        {
            if (Disconnected == null)
            {
                return;
            }

            try
            {
                Disconnected(this);
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        private void OnError(String message)
        {
            if (Error == null)
            {
            }

            try
            {
                Error(this, message);
            }
            catch
            {
            }
        }
    }
}
