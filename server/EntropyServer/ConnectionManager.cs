using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Data.SqlServerCe;
using System.Data;

namespace EntropyServer
{

    static class ConnectionManager
    {
        public static SqlCeConnection SQLConnection;
        public static List<Connection> Connections { get; set; }

        private static TcpListener tcpListener;
        private static Thread listenThread;
        private static List<Thread> clientThreads;
        private static int port;

        public static void Initialize(int p)
        {
            Connections = new List<Connection>();
            clientThreads = new List<Thread>();

            TimeManager.SecondChangeEvent += new TimeManager.SecondChangeHandler(SecondChanged);

            ConnectDatabase();

            GameManager.Initialize();

            port = p;
            tcpListener = new TcpListener(IPAddress.Any, port);
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();

        }

        public static void ConnectDatabase()
        {
            try
            {
                string dbfile = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName + "\\PlayerDatabase.sdf";
                SQLConnection = new SqlCeConnection("datasource=" + dbfile);

                SQLConnection.Open();

                MainForm.Log("Database Connection Established");
                MainForm.Log("ConnectionManager Initialized");
            }
            catch (Exception)
            {
                MainForm.Log("Error Establishing Database Connection");
            }
        }

        public static void Shutdown()
        {
            foreach (Connection conn in Connections)
            {
                conn.Client.Close();
            }

            foreach (Thread cl in clientThreads)
            {
                cl.Abort();
            }

            SQLConnection.Close();

            tcpListener.Stop();
            listenThread.Abort();   
        }

        private static void ListenForClients()
        {
            tcpListener.Start();

            MainForm.Log("Listen Server Started, now accepting clients");

            while (!MainForm.Shutdown_Requested)
            {
                try
                {
                    //blocks until a client has connected to the server
                    TcpClient client = tcpListener.AcceptTcpClient();

                    MainForm.Log("Client " + Connections.Count + " Connected! Waiting for Auth...");

                    //create a thread to handle communication
                    //with connected client
                    clientThreads.Add(new Thread(new ParameterizedThreadStart(HandleClientComm)));
                    clientThreads[clientThreads.Count - 1].Start(client);
                }
                catch (Exception)
                {
                    MainForm.Log("Error with Client Connection");
                }
            }

            tcpListener.Stop();
        }

        private static void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            Connections.Add(new Connection(tcpClient, Connections.Count));

            byte[] message = new byte[4096];
            int bytesRead;

            while (!MainForm.Shutdown_Requested)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();
                string rxString = encoder.GetString(message, 0, bytesRead);
                MainForm.Log("Got Message from Client: " + rxString);
            }
        }

        private static void SecondChanged()
        {
            List<Connection> timed_out_connections = new List<Connection>();

            foreach (Connection conn in Connections)
            {
                if (conn.Is_Timed_Out())
                {
                    timed_out_connections.Add(conn);
                }
            }

            foreach (Connection conn in timed_out_connections)
            {
                MainForm.Log("Client " + conn.conn_num + " timed out!");
                conn.Client.Close();
                Connections.Remove(conn);
            }

            for (int i = 0; i < Connections.Count; i++)
            {
                MainForm.Log("Re-sorting client IDs");
                Connections[i].conn_num = i;
            }
        }
    }
}
