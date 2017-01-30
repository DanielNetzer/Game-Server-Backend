using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServersManager
{

    [Serializable]
    class MessageData
    {

        public int type = 0;
        public string stringData = "";

        public static MessageData FromByteArray(byte[] input)
        {
            MemoryStream stream = new MemoryStream(input);
            BinaryFormatter formatter = new BinaryFormatter();
            MessageData data = new MessageData();
            try
            {
                data.type = (int)formatter.Deserialize(stream);
                data.stringData = (string)formatter.Deserialize(stream);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: Deserializing failed {0}", e);
            }

            if (data.stringData == "")
            {
                data.type = 999;
                data.stringData = "No command included";
            }

            return data;
        }

        public static byte[] ToByteArray(MessageData msg)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, msg.type);
            formatter.Serialize(stream, msg.stringData);

            return stream.ToArray();
        }
    }
}
