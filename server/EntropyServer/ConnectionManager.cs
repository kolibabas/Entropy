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
        public static SqlCeConnection SQL_Connection;
        public static List<Connection> Connections { get; set; }

        private static TcpListener tcp_listener;
        private static Thread listen_thread;
        private static List<Thread> client_threads;
        private static int port;

        public static void Initialize(int p)
        {
            Connections = new List<Connection>();
            client_threads = new List<Thread>();

            TimeManager.SecondChangeEvent += new TimeManager.SecondChangeHandler(SecondChanged);

            Connect_Database();

            GameManager.Initialize();

            port = p;
            tcp_listener = new TcpListener(IPAddress.Any, port);
            listen_thread = new Thread(new ThreadStart(Listen_For_Clients));
            listen_thread.Start();

        }

        public static void Connect_Database()
        {
            try
            {
                string db_file = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName + "\\PlayerDatabase.sdf";
                SQL_Connection = new SqlCeConnection("datasource=" + db_file);

                SQL_Connection.Open();

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
                conn.client.Close();
            }

            foreach (Thread cl in client_threads)
            {
                cl.Abort();
            }

            SQL_Connection.Close();

            tcp_listener.Stop();
            listen_thread.Abort();   
        }

        private static void Listen_For_Clients()
        {
            tcp_listener.Start();

            MainForm.Log("Listen Server Started, now accepting clients");

            while (!MainForm.Shutdown_Requested)
            {
                try
                {
                    //blocks until a client has connected to the server
                    TcpClient client = tcp_listener.AcceptTcpClient();

                    MainForm.Log("Client " + Connections.Count + " Connected! Waiting for Auth...");

                    //create a thread to handle communication
                    //with connected client
                    client_threads.Add(new Thread(new ParameterizedThreadStart(Handle_Client_Comm)));
                    client_threads[client_threads.Count - 1].Start(client);
                }
                catch (Exception)
                {
                    MainForm.Log("Error with Client Connection");
                }
            }

            tcp_listener.Stop();
        }

        private static void Handle_Client_Comm(object client)
        {
            TcpClient tcp_client = (TcpClient)client;
            NetworkStream client_stream = tcp_client.GetStream();

            Connection new_connection = new Connection(tcp_client, Connections.Count);
            Connections.Add(new_connection);

            byte[] message = new byte[4096];
            int bytes_read;

            while (!MainForm.Shutdown_Requested)
            {
                bytes_read = 0;

                try
                {
                    //blocks until a client sends a message
                    bytes_read = client_stream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytes_read == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();
                string rx_string = encoder.GetString(message, 0, bytes_read);
                
                //Tell connection to parse it
                new_connection.Parse_Input(rx_string);
                MainForm.Log("Got Message from Client: " + new_connection.conn_num + ": " + rx_string);
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
                conn.client.Close();
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
