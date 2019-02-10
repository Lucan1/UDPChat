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
            public EndPoint endpoint;   // Сокет клиента
            public string strName;      // Имя, под которым пользователь вошел в чат
        }

        static ArrayList clientList = new ArrayList();
        static Socket serverSocket; // Сокет

       // static List<IPEndPoint> clientList = new List<IPEndPoint>(); // Список "подключенных" клиентов

        static void Main(string[] args)
        {
            
            Console.WriteLine();
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); // Создание сокета
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

                Task listeningTask = new Task(Listen); // Создание потока для получения сообщений
                listeningTask.Start(); // Запуск потока
                listeningTask.Wait(); // Не идем дальше пока поток не будет остановлен
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Main");
                Console.ReadKey();
            }
        }

        

        // поток для приема подключений
        private static void Listen()
        {
            try
            {

                EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 1000); //адрес, с которого пришли данные
                IPEndPoint remoteIP2 = remoteIp as IPEndPoint;

                serverSocket.Bind(remoteIp);  // Привязать этот адрес к серверу

                while (true)
                {

                    StringBuilder builder = new StringBuilder(); // получаем сообщение
                    int bytes = 0; // количество полученных байтов
                    byte[] data = new byte[1024]; // буфер для получаемых данных
                   
                    
                    do
                    {
                        
                        bytes = serverSocket.ReceiveFrom(data, ref remoteIp);
                        string tmp = Encoding.Unicode.GetString(data, 0, bytes);
                        //Console.WriteLine(tmp);
                        builder.Append(tmp);
                    }
                    while (serverSocket.Available > 0);

                    // Преобразование массива байтов, полученных от пользователя, в интеллектуальную форму объекта Data
                    Data msgReceived = new Data(data);

                    // Мы отправим этот объект в ответ на запрос пользователя
                    Data msgToSend = new Data();

                    

                    // Если сообщение предназначено для входа в систему, выхода из системы или простого текстового сообщения, то при отправке другим тип сообщения остается тем же
                    msgToSend.cmdCommand = msgReceived.cmdCommand;
                    msgToSend.strName = msgReceived.strName;

                    byte[] message;
                    message = msgToSend.ToByte();

                    switch (msgReceived.cmdCommand)
                    {
                        case Command.Login:

                            // Когда пользователь входит на сервер, мы добавляем его в наш список клиентов.
                            ClientInfo clientInfo = new ClientInfo();
                            clientInfo.endpoint = remoteIp;
                            clientInfo.strName = msgReceived.strName;
                            clientList.Add(clientInfo);
                            // Устанавливаем текст сообщения, которое мы будем транслировать всем пользователям
                            msgToSend.strMessage = "<<<" + msgReceived.strName + " has joined the room>>>";
                            break;

                        case Command.Logout:

                            // Когда пользователь хочет выйти из сервера, мы ищем его в списке клиентов и закрываем соответствующее соединение
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
                            
                            // Устанавливаем текст сообщения, которое мы будем транслировать всем пользователям
                            msgToSend.strMessage = msgReceived.strName + ": " + msgReceived.strMessage;
                            break;

                        case Command.List:
                            // Отправляем имена всех пользователей в чате новому пользователю
                            msgToSend.cmdCommand = Command.List;
                            msgToSend.strName = null;
                            msgToSend.strMessage = null;

                            // Собираем имена пользователей в чате
                            foreach (ClientInfo client in clientList)
                            {
                                // Для простоты мы используем звездочку в качестве маркера для разделения имен пользователей
                                msgToSend.strMessage += client.strName + "*";
                            }

                            message = msgToSend.ToByte();

                            // Отправить имя пользователя в чате
                            serverSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, remoteIp, new AsyncCallback(OnSend), remoteIp);
                            break;

                        case Command.Static:
                        
                            Console.Write("Static ");
                            msgToSend.strMessage = "127.0.0.1:1000";

                            data = Encoding.Unicode.GetBytes("127.0.0.1:1000");

                            serverSocket.SendTo(data, remoteIp);

                            break;                       
                    }


                    // Список сообщений не транслируется
                    if (msgToSend.cmdCommand != Command.List)
                    {
                        message = msgToSend.ToByte();

                        foreach (ClientInfo clientInfo in clientList)
                        {
                            if (clientInfo.endpoint != remoteIp ||
                                msgToSend.cmdCommand != Command.Login)
                            {
                                // Отправить сообщение всем пользователям
                                serverSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, clientInfo.endpoint, new AsyncCallback(OnSend), clientInfo.endpoint);
                            }
                        }
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
               // serverSocket.EndSendTo(ar);
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


