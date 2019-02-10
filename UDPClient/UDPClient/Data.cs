using System;
using System.Collections.Generic;
using System.Text;

namespace UDPClient
{
    // Команды для взаимодействия между сервером и клиентом
    enum Command
    {
        Login,      // Войти на сервер
        Logout,     // Выход из сервера 
        Message,    // Отправляем текстовое сообщение всем клиентам чата
        List,       // Получить список пользователей в чате с сервера
        Null,        // Нет команды
        Static
    }


    // Структура данных, с помощью которой сервер и клиент взаимодействуют друг с другом
    class Data
    {
        // Конструктор по умолчанию
        public Data()
        {
            this.cmdCommand = Command.Null;
            this.strMessage = null;
            this.strName = null;
        }

        // Преобразует байты в объект типа Data
        public Data(byte[] data)
        {
            // Первые четыре байта для команды
            this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);

            // Следующие четыре хранят длину имени
            int nameLen = BitConverter.ToInt32(data, 4);

            // Следующие четыре хранят длину сообщения
            int msgLen = BitConverter.ToInt32(data, 8);

            // Эта проверка проверяет, что strName было передано в массив байтов
            if (nameLen > 0)
                this.strName = Encoding.UTF8.GetString(data, 12, nameLen);
            else
                this.strName = null;

            // Это проверяет наличие пустого поля сообщения
            if (msgLen > 0)
                this.strMessage = Encoding.UTF8.GetString(data, 12 + nameLen, msgLen);
            else
                this.strMessage = null;
        }

        // Преобразует структуру данных в массив байтов
        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();

            // Первые четыре для команды
            result.AddRange(BitConverter.GetBytes((int)cmdCommand));

            // Добавить длину имени
            if (strName != null)
                result.AddRange(BitConverter.GetBytes(strName.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            // Длина сообщения
            if (strMessage != null)
                result.AddRange(BitConverter.GetBytes(strMessage.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            // Добавить имя
            if (strName != null)
                result.AddRange(Encoding.UTF8.GetBytes(strName));

            // И, наконец, мы добавляем текст сообщения в наш массив байтов
            if (strMessage != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage));

            return result.ToArray();
        }

        public string strName;      // Имя, под которым клиент входит в комнату
        public string strMessage;   //Текст сообщения
        public Command cmdCommand;  // Тип команды (вход, выход из системы, отправка сообщения и т. Д.)
    }
}
