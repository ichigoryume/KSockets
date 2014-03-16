//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Windows.Threading;

using KSockets.Sender;
using KSockets.Receiver;

namespace KSockets.Client
{
    public class ClientSocket
    {
        /*-------------------------------------------------------------------------------
         * Events
         *-------------------------------------------------------------------------------*/
        public delegate void ReceivedHandler(String message);
        public delegate void ErrorHandler(String errorMessage);

        public event Action             Connected;
        public event ReceivedHandler    Received;
        public event Action             Disconnected;
        public event ErrorHandler       Error;


        /*-------------------------------------------------------------------------------
         * Public Properties
         *-------------------------------------------------------------------------------*/
        public String   ServerAddress   { get; internal set; }
        public int      ServerPort      { get; internal set; }


        /*-------------------------------------------------------------------------------
         * Internal Properties
         *-------------------------------------------------------------------------------*/
        internal TcpClient  TcpClient   { get; private set; }
        internal ISender    Sender      { get; private set; }
        internal IReceiver  Receiver    { get; private set; }

        
        /*-------------------------------------------------------------------------------
         * Private Properties
         *-------------------------------------------------------------------------------*/
        private Dispatcher  Dispatcher  { get; set; }
        private AState      State       { get; set; }

        private Byte[]      buffer      = new Byte[1024];


        /*-------------------------------------------------------------------------------
         * Creation Methods
         *-------------------------------------------------------------------------------*/
        public static ClientSocket CreateVariableMessageSocket()
        {
            return new ClientSocket(new VariableSizeMessageSender(), new VariableSizeMessageReceiver());
        }

        public static ClientSocket CreateFixedSizeMessageSocket(int messageSize)
        {
            return new ClientSocket(new FixedSizeMessageSender(messageSize), new FixedSizeMessageReceiver(messageSize));
        }

        public static ClientSocket CreateTerminatedMessageSocket(String terminator)
        {
            return new ClientSocket(new TerminatedMessageSender(terminator), new TerminatedMessageReceiver(terminator));
        }

        public ClientSocket(ISender sender, IReceiver receiver)
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
        public void Start(String serverAddress, int serverPort)
        {
            State.StartRequested(serverAddress, serverPort);
        }

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

        internal void CreateTcpClient()
        {
            if(TcpClient != null)
            {
                throw new Exception("TcpClient != null");
            }

            TcpClient = new TcpClient();
        }

        internal void CloseTcpClient()
        {
            if (TcpClient == null)
            {
                return;
            }
 
            TcpClient.Close();
            TcpClient = null;
        }

        internal async void ConnectAsync()
        {
            try
            {
                await TcpClient.ConnectAsync(ServerAddress, ServerPort);
                State.NotifyConnected();
            }
            catch (Exception e)
            {
                State.NotifyConnectError(e);
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

        internal void DispatchEventConnected()
        {
            Dispatcher.BeginInvoke(new Action(() => OnConnected()), null);
        }

        internal void DispatchEventReceived(String message)
        {
            Dispatcher.BeginInvoke(new Action(() => OnReceived(message)), null);
        }

        internal void DispatchEventDisconnected()
        {
            Dispatcher.BeginInvoke(new Action(() => OnDisconnected()), null);
        }

        internal void DispatchEventError(Exception e)
        {
            Dispatcher.BeginInvoke(new Action(() => OnError(e.Message)), null);
        }


        /*-------------------------------------------------------------------------------
         * Private Methods
         *-------------------------------------------------------------------------------*/
        private void OnConnected()
        {
            if(Connected == null)
            {
                return;
            }

            try
            {
                Connected();
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

        private void OnDisconnected()
        {
            if(Disconnected == null)
            {
                return;
            }

            try
            {
                Disconnected();
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
