using System;
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

using KSockets.Server.MultiClients;

namespace ServerSample_MultiClient_
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private ServerSocket ServerSocket { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void acceptButton_Checked(object sender, RoutedEventArgs e)
        {
            if (variableSizeMessageRadioButton.IsChecked == true)
            {
                ServerSocket = ServerSocket.CreateVariableMessageSocket();
            }
            else if (fixedSizeMessageRadioButton.IsChecked == true)
            {
                ServerSocket = ServerSocket.CreateFixedSizeMessageSocket(int.Parse(messageSizeTextBox.Text));
            }
            else
            {
                String terminator = terminationStringTextBox.Text;
                terminator = terminator.Replace("\\n", "\n");
                terminator = terminator.Replace("\\r", "\r");
                ServerSocket = ServerSocket.CreateTerminatedMessageSocket(terminator);
            }

            ServerSocket.Accepted += ServerSocket_Accepted;
            ServerSocket.Error += ServerSocket_Error;

            int listenPort = int.Parse(listenPortTextBox.Text);
            ServerSocket.Start(listenPort);
        }


        private void acceptButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ServerSocket.Close();

            ServerSocket.Accepted -= ServerSocket_Accepted;
            ServerSocket.Error -= ServerSocket_Error;

            ServerSocket = null;
        }

        void ServerSocket_Accepted(Client client)
        {
            WriteLog("Accepted : " + client.Address);

            client.Received += client_Received;
            client.Disconnected += client_Disconnected;
            client.Error += client_Error;
        }

        void ServerSocket_Error(string errorMessage)
        {
            WriteLog("Error : " + errorMessage);
        }

        void client_Received(Client client, string message)
        {
            WriteLog("Received from " + client.Address + " : " + message);
        }

        void client_Disconnected(Client client)
        {
            WriteLog("Disconnected with " + client.Address);
        }

        void client_Error(Client client, string errorMessage)
        {
            WriteLog("Error at " + client.Address + " : " + errorMessage);
        }

        private void WriteLog(String message)
        {
            logPanel.AppendText(message + Environment.NewLine);
            logPanel.ScrollToEnd();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Client client in ServerSocket.Clients)
            {
                client.Send(messageTextBox.Text);
            }
        }

    }
}
