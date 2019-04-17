using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace sIRC
{
    class NetworkClient
    {
        private TcpClient irc;
        private NetworkStream stream;
        private Thread listener;

        private Queue<string> responseBuffer;
        private Queue<string> notificationBuffer;

        public bool Connected { get { return irc != null && irc.Connected; } }
        public bool ResponseAvailable { get { return responseBuffer.Count > 0; } }

        public NetworkClient()
        {
            responseBuffer = new Queue<string>();
            notificationBuffer = new Queue<string>();
        }

        private void Notification(string message)
        {
            notificationBuffer.Enqueue(message);
        }

        public virtual void Slice()
        {
            while(notificationBuffer.Count > 0)
                Log.WriteLine(notificationBuffer.Dequeue(), ConsoleColor.DarkRed);
        }

        public virtual string ReadMessage()
        {
            lock (this)
            {
                return responseBuffer.Dequeue();
            }
        }

        public virtual bool Connect(string server, int port, string nick, string user)
        {
            Notification("! Connecting...");

            try
            {
                irc = new TcpClient();
                irc.Connect(server, port);
                stream = irc.GetStream();

                Notification("! Connected to the server");
            }
            catch
            {
                Notification("! Could not connect to the server");
                return false;
            }

            Send("NICK " + nick);
            Send("USER " + user);

            listener = new Thread(Receive);
            listener.Start();

            return true;
        }

        protected virtual void Receive()
        {
            Notification("! Starting to listen the server");
            using (StreamReader reader = new StreamReader(stream))
            {
                string response;

                try
                {
                    while (true)
                    {
                        if ((response = reader.ReadLine()) != null)
                        {
                            lock (this)
                            {
                                responseBuffer.Enqueue(response);
                            }
                        }
                    }
                }
                catch
                {
                    Notification("! The listener has stopped executing");
                }
            }
        }

        public virtual void Send(string output)
        {
            if (Connected)
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(output);
                writer.Flush();

                Log.WriteLine(output, ConsoleColor.DarkYellow);
            }
            else
            {
                Notification("! Disconnected from the server");
            }
        }

        public virtual void Close()
        {
            Notification("! Closing the connection");

            if (stream != null)
                stream.Close();

            if (irc != null)
                irc.Close();

            if (listener != null && listener.IsAlive)
                listener.Abort();

            irc = null;
            stream = null;
            listener = null;
        }
    }
}
