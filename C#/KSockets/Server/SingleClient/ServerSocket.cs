//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;

using KSockets.Sender;
using KSockets.Receiver;

namespace KSockets.Server.SingleClient
{
    public class ServerSocket
    {
        /*-------------------------------------------------------------------------------
         * Events
         *-------------------------------------------------------------------------------*/
        public delegate void AcceptedHandler(String clientAddress);
        public delegate void ReceivedHandler(String message);
        public delegate void DisconnectedHandler(String clientAddress);
        public delegate void ErrorHandler(String errorMessage);

        public event AcceptedHandler        Accepted;
        public event ReceivedHandler        Received;
        public event DisconnectedHandler    Disconnected;
        public event ErrorHandler           Error;


        /*-------------------------------------------------------------------------------
         * Public Properties
         *-------------------------------------------------------------------------------*/
        public int Port { get; internal set; }
        public String ClientAddress { get; private set; }


        /*-------------------------------------------------------------------------------
         * Internal Properties
         *-------------------------------------------------------------------------------*/
        internal TcpListener    TcpListener { get; private set; }
        internal TcpClient      TcpClient   { get; private set; }
        internal ISender        Sender      { get; private set; }
        internal IReceiver      Receiver    { get; private set; }


        /*-------------------------------------------------------------------------------
         * Private Properties
         *-------------------------------------------------------------------------------*/
        private Dispatcher  Dispatcher  { get; set; }
        private AState      State       { get; set; }

        private Byte[] buffer = new Byte[1024];


        /*-------------------------------------------------------------------------------
         * Creation Methods
         *-------------------------------------------------------------------------------*/
        public static ServerSocket CreateVariableMessageSocket()
        {
            return new ServerSocket(new VariableSizeMessageSender(), new VariableSizeMessageReceiver());
        }

        public static ServerSocket CreateFixedSizeMessageSocket(int messageSize)
        {
            return new ServerSocket(new FixedSizeMessageSender(messageSize), new FixedSizeMessageReceiver(messageSize));
        }

        public static ServerSocket CreateTerminatedMessageSocket(String terminator)
        {
            return new ServerSocket(new TerminatedMessageSender(terminator), new TerminatedMessageReceiver(terminator));
        }

        public ServerSocket(ISender sender, IReceiver receiver)
        {
            Sender = sender;
            Receiver = receiver;

            Dispatcher = Dispatcher.CurrentDispatcher;
            State = new StateIdle(this);

            State.Enter();
        }



        /*-------------------------------------------------------------------------------
         * Public Methods
         *-------------------------------------------------------------------------------*/
        public void Start(int port)
        {
            State.StartRequested(port);
        }

        public void Send(String message)
        {
            State.SendRequested(message);
        }

        public void Close()
        {
            State.CloseRequested();
            ClientAddress = null;
        }


        /*-------------------------------------------------------------------------------
         * Internal Methods
         *-------------------------------------------------------------------------------*/
        internal void UpdateState(AState requester, AState nextState)
        {
            if (State != requester)
            {
                return;
            }

            State.Leave();
            State = nextState;
            State.Enter();
        }

        internal void CreateTcpListener()
        {
            if(TcpListener != null)
            {
                throw new Exception("TcpListener != null");
            }

            TcpListener = new TcpListener(IPAddress.Any, Port);
        }

        internal void CloseTcpListenerAndClient()
        {
            if (TcpListener != null)
            {
                TcpListener.Stop();
                TcpListener = null;
            }

            if(TcpClient != null)
            {
                TcpClient.Close();
                TcpClient = null;
            }
        }

        internal async void AcceptAsync()
        {
            try
            {
                TcpListener.Start();
                TcpClient = await TcpListener.AcceptTcpClientAsync();
                State.NotifyAccepted();
            }
            catch (Exception e)
            {
                State.NotifyAcceptError(e);
            }
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

        // event dispatch method 群
        internal void DispatchAccepted()
        {
            ClientAddress = TcpClient.Client.RemoteEndPoint.ToString();
            Dispatcher.BeginInvoke(new Action(() => OnAccepted(ClientAddress)), null);
        }

        internal void DispatchReceived(String message)
        {
            Dispatcher.BeginInvoke(new Action(() => OnReceived(message)), null);
        }

        internal void DispatchDisconnected()
        {
            Dispatcher.BeginInvoke(new Action(() => OnDisconnected(ClientAddress)), null);
        }

        internal void DispatchError(Exception e)
        {
            Dispatcher.BeginInvoke(new Action(() => OnError(e.Message)), null);
            ClientAddress = null;
        }


        /*-------------------------------------------------------------------------------
         * Private Methods
         *-------------------------------------------------------------------------------*/
        private void OnAccepted(String clientAddress)
        {
            if(Accepted == null)
            {
                return;
            }

            try
            {
                Accepted(clientAddress);
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        private void OnReceived(String message)
        {
            if(Received == null)
            {
                return;
            }

            try
            {
                Received(message);
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        private void OnDisconnected(String clientAddress)
        {
            if(Disconnected == null)
            {
                return;
            }

            try
            {
                Disconnected(clientAddress);
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        private void OnError(String message)
        {
            if(Error == null)
            {
            }

            try
            {
                Error(message);
            }
            catch
            {
            }
        }
    }
}
