using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace UDPClient
{
    public partial class UDPClient : Form
    {
        public Socket clientSocket; // основной клиентский сокет
        public string strName;      // Имя, под которым пользователь входит в комнату
        public EndPoint epServer;   // Конечная точка сервера
        byte[]byteData = new byte[1024];

        public UDPClient()
        {
            InitializeComponent();
        }

        // Трансляция сообщения, набранного пользователем всем
        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                // Заполняем информацию для отправки сообщения
                Data msgToSend = new Data();

                msgToSend.strName = strName;
                msgToSend.strMessage = txtMessage.Text;
                msgToSend.cmdCommand = Command.Message;

                byte[] byteData = msgToSend.ToByte();

                //Send it to the server
                clientSocket.BeginSendTo (byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);

                txtMessage.Text = null;
            }
            catch (Exception)
            {
                MessageBox.Show("Отправка сообщение - не возможна.", "UDPClient: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (ObjectDisposedException)
            {
                MessageBox.Show("ObjectDisposedException OnSend");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, " OnSend UDPClient: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
           
            try
            {                
                clientSocket.EndReceive(ar);

                // Преобразуем полученные байты в объект типа Data
                Data msgReceived = new Data(byteData);
            
                // Соответственно обрабатываем полученное сообщение
                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:
                        lstChatters.Items.Add(msgReceived.strName);
                        break;

                    case Command.Logout:
                        lstChatters.Items.Remove(msgReceived.strName);
                        break;

                    case Command.Message:
                        break;

                    case Command.List:
                        lstChatters.Items.AddRange(msgReceived.strMessage.Split('*'));
                        lstChatters.Items.RemoveAt(lstChatters.Items.Count - 1);
                        
                        txtChatBox.Text += "<<<" + strName + " has joined the room>>>\r\n";
                        break;
                }

                if (msgReceived.strMessage != null && msgReceived.cmdCommand != Command.List)
                    txtChatBox.Text += msgReceived.strMessage + "\r\n";

                byteData = new byte[1024];

                // Начать прослушивание, чтобы получить больше данных от пользователя
                clientSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epServer,
                                           new AsyncCallback(OnReceive), null);
            }
            catch (ObjectDisposedException)
            {
                MessageBox.Show("ObjectDisposedException OnReceive");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "OnReceive UDPClient: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
	      CheckForIllegalCrossThreadCalls = false;

            this.Text = "UDPClient: " + strName;

            // Пользователь вошел в систему, поэтому теперь мы просим сервер отправить имена всех пользователей, которые находятся в чате.
            Data msgToSend = new Data ();
            msgToSend.cmdCommand = Command.List;
            msgToSend.strName = strName;
            msgToSend.strMessage = null;

            byteData = msgToSend.ToByte();

            clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);

            byteData = new byte[1024];
            // начинаем слушать данные асинхронно
            clientSocket.BeginReceiveFrom (byteData, 0, byteData.Length, SocketFlags.None,  ref epServer,  new AsyncCallback(OnReceive), null);
            //MessageBox.Show("ТИК1");

        }

        private void txtMessage_TextChanged(object sender, EventArgs e)
        {
            if (txtMessage.Text.Length == 0)
                btnSend.Enabled = false;
            else
                btnSend.Enabled = true;
        }


        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend_Click(sender, null);
            }
        }

        private void UDPClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Вы уверены что хотите выйти?", "UDPClient: " + strName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            try
            {
                // Отправить сообщение для выхода из сервера
                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Logout;
                msgToSend.strName = strName;
                msgToSend.strMessage = null;

                byte[] b = msgToSend.ToByte();
                clientSocket.SendTo(b, 0, b.Length, SocketFlags.None, epServer);
                clientSocket.Close();
            }
            catch (ObjectDisposedException)
            { MessageBox.Show("ObjectDisposedException UDPClient_FormClosing"); }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "UDPClient_FormClosing UDPClient: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    
    
}