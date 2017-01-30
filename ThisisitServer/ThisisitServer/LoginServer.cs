using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace ServersManager
{
    public class LoginServer
    {
        static LoginServer singleton;

        private Socket serverSocket = null;
        // mysql connection string
        private static string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private static MySqlConnection mysqlCon = null;

        ArrayList _connections = new ArrayList();
        ArrayList _buffer = new ArrayList();
        ArrayList _byteBuffer = new ArrayList();

        public static void StartServer()
        {
            LoginServer serverInstance = new LoginServer();
            // MySQL Connection
            try
            {
                mysqlCon = new MySqlConnection(connectionString);
                mysqlCon.Open();
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Error: {0}", e.ToString());
                if (mysqlCon != null)
                    mysqlCon.Close();
                Console.WriteLine("LoginServer :: will shut down. Press any key to continue...");
                Console.ReadLine();
                Environment.Exit(0);
            }
            finally
            {
                serverInstance.SetupServer();
                while (true)
                {
                    serverInstance.HoldListening();
                }
            }
        }

        private void SetupServer()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, 32211);
            serverSocket.Bind(ipLocal);
            serverSocket.Listen(100);
            singleton = this;
            Console.WriteLine("LoginServer :: started on " + ipLocal.ToString());

        }

        private void HoldListening()
        {
            ArrayList listenList = new ArrayList();
            listenList.Add(serverSocket);
            Socket.Select(listenList, null, null, 1000);
            for (int i = 0; i < listenList.Count; i++)
            {
                Socket newSocket = ((Socket)listenList[i]).Accept();
                _connections.Add(newSocket);
                _byteBuffer.Add(new ArrayList());
                Console.WriteLine("New connection from: " + newSocket.LocalEndPoint.ToString());
            }
            readData();
        }

        private void readData()
        {
            if (_connections.Count > 0)
            {
                ArrayList connection = new ArrayList(_connections);
                Socket.Select(connection, null, null, 1000);
                foreach (Socket socket in connection)
                {
                    byte[] receivedBytes = new byte[512];
                    ArrayList buffer = (ArrayList)_byteBuffer[_connections.IndexOf(socket)];
                    int read = socket.Receive(receivedBytes);
                    for (int i = 0; i < read; i++)
                    {
                        buffer.Add(receivedBytes[i]);
                    }
                    while (true && buffer.Count > 0)
                    {
                        int length = (byte)buffer[0];
                        if (length < buffer.Count)
                        {
                            ArrayList thisMsgBytes = new ArrayList(buffer);
                            thisMsgBytes.RemoveRange(length + 1, thisMsgBytes.Count - (length + 1));
                            thisMsgBytes.RemoveRange(0, 1);
                            if (thisMsgBytes.Count != length)
                            {
                                Console.WriteLine("Error Here");
                            }
                            buffer.RemoveRange(0, length + 1);
                            byte[] readBytes = (byte[])thisMsgBytes.ToArray(typeof(byte));
                            MessageData readMsg = MessageData.FromByteArray(readBytes);
                            _buffer.Add(readMsg);
                            HandleReceivedPacket(readMsg, socket);
                            if (singleton != this)
                            {
                                Console.WriteLine("Error Here");
                            }
                        }
                    }
                }
            }

        }

        // Start of Client requests
        // Types : 0-> Login request; 1-> Character Request; 2-> Registration Request

        private void HandleReceivedPacket(MessageData data, Socket socket)
        {
            switch (data.type)
            {
                case 0:
                    Console.WriteLine("Recieved a login request from {0}", socket.LocalEndPoint.ToString());
                    RequestHandler.Login(data.stringData.ToLower(), socket, mysqlCon);
                    break;
                case 1:
                    // TODO: HandleCharacterRequest
                    Console.WriteLine("Character request");
                    break;
                case 2:
                    Console.WriteLine("Recieved a registration request from {0}", socket.LocalEndPoint.ToString());
                    RequestHandler.Register(data.stringData.ToLower(), socket, mysqlCon);
                    break;
                default:
                    Console.WriteLine("Unknown request");
                    break;
            }
        }
    }
}
