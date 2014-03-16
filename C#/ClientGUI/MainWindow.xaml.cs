//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using KSockets.Client;

namespace ClientGUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClientSocket ClientSocket { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void connectButton_Checked(object sender, RoutedEventArgs e)
        {
            WriteLog("Start");
            int serverPort = int.Parse(serverPortTextBox.Text);

            if (variableSizeMessageRadioButton.IsChecked == true)
            {
                ClientSocket = ClientSocket.CreateVariableMessageSocket();
            }
            else if (fixedSizeMessageRadioButton.IsChecked == true)
            {
                ClientSocket = ClientSocket.CreateFixedSizeMessageSocket(int.Parse(messageSizeTextBox.Text));
            }
            else
            {
                String terminator = terminationStringTextBox.Text;
                terminator = terminator.Replace("\\n", "\n");
                terminator = terminator.Replace("\\r", "\r");
                ClientSocket = ClientSocket.CreateTerminatedMessageSocket(terminator);
            }

            ClientSocket.Connected      += ClientSocket_Connected;
            ClientSocket.Disconnected   += ClientSocket_Disconnected;
            ClientSocket.Received       += ClientSocket_Received;
            ClientSocket.Error          += ClientSocket_Error;

            ClientSocket.Start(serverAddressTextBox.Text, serverPort);
            WriteLog("Started");
        }

        private void connectButton_Unchecked(object sender, RoutedEventArgs e)
        {
            WriteLog("Stop");
            ClientSocket.Close();

            ClientSocket.Connected      -= ClientSocket_Connected;
            ClientSocket.Disconnected   -= ClientSocket_Disconnected;
            ClientSocket.Received       -= ClientSocket_Received;
            ClientSocket.Error          -= ClientSocket_Error;

            ClientSocket = null;
            WriteLog("Stopped");
        }


        void ClientSocket_Connected()
        {
            WriteLog("Connected");
        }

        void ClientSocket_Received(string message)
        {
            WriteLog("Received : " + message);
        }

        void ClientSocket_Disconnected()
        {
            WriteLog("Disconnected");
        }

        void ClientSocket_Error(string errorMessage)
        {
            WriteLog("Error : " + errorMessage);
        }

        private void WriteLog(String message)
        {
            logPanel.AppendText(message + Environment.NewLine);
            logPanel.ScrollToEnd();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClientSocket.Send(messageTextBox.Text);
            }
            catch (Exception exception)
            {
                WriteLog("Error : " + exception.Message);
            }
        }

    }
}
