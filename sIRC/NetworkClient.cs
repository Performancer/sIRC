using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace sIRC
{
    class NetworkClient
    {
        public delegate void CommandResponseHandler(string message);
        public event CommandResponseHandler CommandResponseReceived;

        private readonly string server;
        private readonly int port;

        private TcpClient irc;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread listener;

        private Queue<string> buffer;
        private bool closed;

        public NetworkClient(string server, int port)
        {
            this.server = server;
            this.port = port;

            buffer = new Queue<string>();
        }

        public virtual void Slice()
        {
            for (int i = 0; i < buffer.Count; i++)
            {
                string response = buffer.Dequeue();

                if(response != null)
                    CommandResponseReceived?.Invoke(response);
            }
        }

        protected virtual void Notification(string message)
        {
            Log.WriteLine(message, ConsoleColor.DarkRed);
        }

        public virtual bool Connect(string nick, string user)
        {
            Notification("! Connecting...");

            try
            {
                irc = new TcpClient(server, port);
                stream = irc.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                closed = false;

                Notification("! Connected to the server");
            }
            catch
            {
                Notification("! Could not connect to the server");
                return false;
            }

            Send("NICK " + nick, false);
            Send("USER " + user, false);

            listener = new Thread(new ThreadStart(Receive));
            listener.Start();

            return true;
        }

        protected virtual void Receive()
        {
            Notification("! Starting to listen the server");

            try
            {
                while (!closed)
                {
                    string input;
                    while ((input = reader.ReadLine()) != null)
                        buffer.Enqueue(input);
                }
            }
            catch
            {
                Notification("! Could not continue listening the server");
            }
        }

        public virtual bool Send(string output, bool notify)
        {
            if (closed)
            {
                Notification("! Tried to send when the stream was already closed");
                return false;
            }

            writer.WriteLine(output);
            writer.Flush();

            if(notify)
                Log.WriteLine(output, ConsoleColor.DarkYellow);

            return Connected();
        }

        public virtual bool Connected()
        {
            bool connected = irc != null && irc.Connected;

            if (!closed && !connected)
            {
                Notification("! Disconnected from the server");
                Close();
            }

            return connected;
        }

        public virtual void Close()
        {
            Notification("! Closing the network stream");
            closed = true;

            if (irc != null)
                irc.Close();

            if (stream != null)
                stream.Close();

            if (reader != null)
                reader.Close();

            if (writer != null)
                reader.Close();

            while (listener != null && listener.IsAlive)
                Notification("! Waiting for the listener to abort...");
        }
    }
}
