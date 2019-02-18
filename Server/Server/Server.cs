using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace Server
{
    class Server
    {
        struct ClientInfo
        {
            public EndPoint endpoint;  
            public string strName;      
        }

        static ArrayList clientList = new ArrayList();
        static Socket serverSocket; 

        static void Main(string[] args)
        {
            Console.WriteLine();
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Task listeningTask = new Task(Listen); 
                listeningTask.Start(); 
                listeningTask.Wait(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Main");
                Console.ReadKey();
            }
        }
        private static void Listen()
        {
            try
            {
                EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 1000);
                IPEndPoint remoteIP2 = remoteIp as IPEndPoint;
                serverSocket.Bind(remoteIp);  

                while (true)
                {
                    StringBuilder builder = new StringBuilder(); 
                    int bytes = 0; 
                    byte[] data = new byte[1024]; 

                    do
                    {
                        bytes = serverSocket.ReceiveFrom(data, ref remoteIp);
                        string tmp = Encoding.Unicode.GetString(data, 0, bytes);
                        builder.Append(tmp);
                    }
                    while (serverSocket.Available > 0);

                    Data msgReceived = new Data(data);
                    Data msgToSend = new Data();
                    msgToSend.cmdCommand = msgReceived.cmdCommand;
                    msgToSend.strName = msgReceived.strName;
                    byte[] message;
                    message = msgToSend.ToByte();
                    
                    switch (msgReceived.cmdCommand)
                    {
                        case Command.Login:

                            ClientInfo clientInfo = new ClientInfo();
                            clientInfo.endpoint = remoteIp;
                            clientInfo.strName = msgReceived.strName;
                            clientList.Add(clientInfo);
                            msgToSend.strMessage = "<<<" + msgReceived.strName + " has joined the room>>>";
                            break;

                        case Command.Logout:
                            int nIndex = 0;
                            foreach (ClientInfo client in clientList)
                            {
                                if (client.endpoint == remoteIp)
                                {
                                    clientList.RemoveAt(nIndex);
                                    break;
                                }
                                ++nIndex;
                            }
                            msgToSend.strMessage = "<<<" + msgReceived.strName + " has left the room>>>";
                            break;

                        case Command.Message:
                            msgToSend.strMessage = msgReceived.strName + ": " + msgReceived.strMessage;
                            break;

                        case Command.List:
                            msgToSend.cmdCommand = Command.List;
                            msgToSend.strName = null;
                            msgToSend.strMessage = null;

                            foreach (ClientInfo client in clientList)
                            {
                                msgToSend.strMessage += client.strName + "*";
                            }
                            message = msgToSend.ToByte();
                            serverSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, remoteIp, new AsyncCallback(OnSend), remoteIp);
                            break;

                        case Command.Static:
                            Console.WriteLine("Static " + msgReceived.strMessage +" "+ remoteIp);
                            msgToSend.strMessage = clientList.Count.ToString();
                            msgToSend.strName = "127.0.0.1";
                            message = msgToSend.ToByte();
                            serverSocket.SendTo(message, SocketFlags.None, remoteIp);
                            break;

                        case Command.Package:
                            msgToSend.cmdCommand = Command.Package;
                            msgToSend.strName = null;
                            msgToSend.strMessage = null;
                            message = msgToSend.ToByte();
                            serverSocket.SendTo(message, SocketFlags.None, remoteIp);
                            break;

                        case Command.LocalMessage:
                            msgToSend.strName = msgReceived.strName; // komy
                            msgToSend.strMessage = msgReceived.strMessage; // chto

                            break;
                    }

                    if (msgToSend.cmdCommand == Command.LocalMessage )
                    {
                        message = msgToSend.ToByte();

                        foreach (ClientInfo clientInfo in clientList)
                        {
                            if (clientInfo.strName == msgToSend.strName)
                            {
                                if (clientInfo.endpoint != remoteIp || msgToSend.cmdCommand != Command.Login)
                                {
                                    serverSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, clientInfo.endpoint, new AsyncCallback(OnSend), clientInfo.endpoint);
                                }
                            }
                            
                        }

                        if (msgToSend.strMessage != null)
                            Console.WriteLine(msgToSend.strMessage + " inform " + msgReceived.strName);
                    }

                    if (msgToSend.cmdCommand != Command.List && msgToSend.cmdCommand != Command.Static && msgToSend.cmdCommand != Command.LocalMessage)
                    {
                        message = msgToSend.ToByte();

                        foreach (ClientInfo clientInfo in clientList)
                        {
                            if (clientInfo.endpoint != remoteIp ||
                                msgToSend.cmdCommand != Command.Login)
                            {
                                serverSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, clientInfo.endpoint, new AsyncCallback(OnSend), clientInfo.endpoint);
                            }
                        }

                        if (msgToSend.strMessage != null)
                        Console.WriteLine(msgToSend.strMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Listen");
                Console.ReadKey();
            }
        }

         static public void OnSend(IAsyncResult ar)
        {
            try
            {
                serverSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("OnSend");
                Console.ReadKey();
            }
        }
    }
}


