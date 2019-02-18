using System;
using System.Collections.Generic;
using System.Text;

namespace UDPClient
{
    enum Command
    {
        Login,
        Logout,
        Message,
        List,
        Static,
        Package,
        LocalMessage,
        Null
    }

    class Data
    {
        public string strName;
        public string strMessage;
        public Command cmdCommand;

        public Data()
        {
            this.cmdCommand = Command.Null;
            this.strMessage = null;
            this.strName = null;
        }

        public Data(byte[] data)
        {
            this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);
            int nameLen = BitConverter.ToInt32(data, 4);
            int msgLen = BitConverter.ToInt32(data, 8);

            if (nameLen > 0)
                this.strName = Encoding.UTF8.GetString(data, 12, nameLen);
            else
                this.strName = null;

            if (msgLen > 0)
                this.strMessage = Encoding.UTF8.GetString(data, 12 + nameLen, msgLen);
            else
                this.strMessage = null;
        }

        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();
            result.AddRange(BitConverter.GetBytes((int)cmdCommand));

            if (strName != null)
                result.AddRange(BitConverter.GetBytes(strName.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            if (strMessage != null)
                result.AddRange(BitConverter.GetBytes(strMessage.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            if (strName != null)
                result.AddRange(Encoding.UTF8.GetBytes(strName));

            if (strMessage != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage));

            return result.ToArray();
        }


    }
}
