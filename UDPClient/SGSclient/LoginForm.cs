using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UDPClient
{
    public partial class LoginForm : Form
    {
        public Socket clientSocket;
        public EndPoint epServer;
        public string strName;
        public string address;
        public string NumOfPep;
        public byte selectListBox = 0;

        public LoginForm()
        {
            InitializeComponent();
            txtName.Text = "Enter your name...";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            strName = txtName.Text;

            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPAddress ipAddress = IPAddress.Parse(address);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

                epServer = (EndPoint)ipEndPoint;

                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Login;
                msgToSend.strMessage = null;
                msgToSend.strName = strName;

                byte[] byteData = msgToSend.ToByte();

                clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "UDPClient", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
                strName = txtName.Text;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "UDPClient On Send", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            /*if (MessageBox.Show("Вы уверены что хотите выйти?", "UDPClient: " + strName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
            {
                Application.Exit();
                return;
            }*/
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            if (txtName.Text.Length > 0)
                btnOK.Enabled = true;
            else
                btnOK.Enabled = false;
        }


        private void LoginForm_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }

        public Socket StaticSocketClient;
        byte[] StaticByteData = new byte[1024];

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                timer1.Enabled = true;
                StaticSocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); 
                StaticSocketClient.EnableBroadcast = true;
                EndPoint remoteIp = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 1000);

                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Static;
                msgToSend.strName = null;
                msgToSend.strMessage = null;

                byte[] StaticByteData = msgToSend.ToByte();
                StaticSocketClient.SendTo(StaticByteData, SocketFlags.None, remoteIp);
                Task listeningTask = new Task(Listen); 
                listeningTask.Start(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "UDPClient button1", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

         void Listen()
         {
            try
            {
                EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 1000);
                int bytes = 0; 
                byte[] data = new byte[1024];
                listBox1.Items.Clear();

                do
                {
                    bytes = StaticSocketClient.ReceiveFrom(data, ref remoteIp);
                    string tmp = Encoding.Unicode.GetString(data, 0, bytes);
                    Data msgReceived = new Data(data);
                    address = msgReceived.strName;
                    NumOfPep = msgReceived.strMessage;                    
                    listBox1.Items.Add("Server " + address + ":1000 " + NumOfPep + "/32");

                } while (StaticSocketClient.Available > 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "UDPClient Listen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (txtName.Text.Length > 0)
            {
                btnOK.PerformClick();
            }
            else
            {
                MessageBox.Show("Enter data in the \"Name\" field\n\nВведіть дані в поле \"Name\"", "UDPClient", MessageBoxButtons.OK);
            }
        }

        private void listBox1_MouseEnter(object sender, EventArgs e)
        {
            if (txtName.Text.Length > 0)
            {
                btnOK.PerformClick();
            }
            else
            {
                MessageBox.Show("Enter data in the \"Name\" field\n\nВведіть дані в поле \"Name\"", "UDPClient", MessageBoxButtons.OK);
            }
        }

       

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                UDPClient ClientForm = new UDPClient();
                ClientForm.clientSocket = clientSocket;
                ClientForm.strName = strName;
                ClientForm.epServer = epServer;
                timer1.Enabled = false;
                ClientForm.ShowDialog();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectListBox = 1; 
        }

        private void txtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            txtName.Text = null;
        }

        private void txtName_MouseClick(object sender, MouseEventArgs e)
        {
            txtName.Text = null;

        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
           /* if (MessageBox.Show("Вы уверены что хотите выйти?", "UDPClient: " + strName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
            {
                Application.Exit();
                return;
            }*/
        }
    }
}