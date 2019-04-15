using System;
using System.Collections;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace sIRC
{
    class Client
    {
        private readonly Log log;
        private readonly NetworkClient network;
        private readonly Hashtable channels;
        private readonly Spinner spinner;

        private readonly string server;
        private readonly string user;
        private readonly string nick;

        private Channel activeChannel;
        private string command;
        private bool ready, refresh, quit;
        private DateTime lastPingTime;
        private TimeSpan pingDelay = TimeSpan.FromMinutes(1);

        public Client(string server, int port, string user, string nick)
        {
            this.server = server;
            this.user = user;
            this.nick = nick;

            log = new Log();
            log.OnWrite += Log_OnWrite;

            network = new NetworkClient(server, port);
            network.CommandResponseReceived += Network_OnCommandResponseReceived;

            spinner = new Spinner();

            activeChannel = new Channel(server);
            channels = new Hashtable();
            channels.Add(server, activeChannel);
        }

        private void Log_OnWrite()
        {
            refresh = true;
        }

        private void Network_OnCommandResponseReceived(string message)
        {
            HandleReceived(message);
        }

        public virtual void Update()
        {
            refresh = false;

            if (ready)
            {
                Log.ClearCurrentLine();
                Console.Write(activeChannel.ToString() + "> " + command);
            }
            else
            {
                spinner.Reset();
            }
        }

        public virtual void Start()
        {
            while (!network.Connect(nick, user))
            {
                Console.WriteLine("Attempting to reconnect in 5 seconds");
                Thread.Sleep(5000);
            }

            Run();
        }

        public void Run()
        {
            while (network.Connected())
            {
                network.Slice();

                if (ready)
                {
                    if (lastPingTime + pingDelay < DateTime.Now)
                        Ping();

                    if (Console.KeyAvailable)
                        HandleInput(Console.ReadKey().KeyChar);
                }
                else
                {
                    spinner.Turn();

                    if (Console.KeyAvailable)
                    {
                        char character = Console.ReadKey(true).KeyChar;
                    }
                }

                if (refresh)
                    Update();
            }

            Restart();
        }

        public void Restart()
        {
            ready = false;

            Console.WriteLine("Write 'RECONNECT' or 'EXIT' to continue...");

            while (quit)
            {
                string command = Console.ReadLine();

                if (command.ToUpper() == "RECONNECT")
                    quit = false;
                else if (command.ToUpper() == "EXIT")
                    return;
            }

            Start();
        }

        public virtual void HandleInput(char character)
        {
            switch (character)
            {
                case (char)08: //BACKSPACE
                    Console.CursorLeft++;

                    if (command != null && command.Length > 0)
                    {
                        command = command.Remove(command.Length - 1);
                        Log.ClearLastCharacter();
                    }
                    break;
                case (char)10: //LINE FEED
                    break;
                case (char)13: //ENTER
                    string temp = command;
                    command = "";

                    if (temp.ToUpper() == "/SWITCH")
                    {
                        SetLayout();
                    }
                    else if (temp.StartsWith("/"))
                    {
                        temp = temp.Substring(1);
                        network.Send(temp, true);

                        if (temp.Split(' ')[0].ToUpper() == "QUIT")
                        {
                            quit = true;
                            Ping();
                        }
                    }
                    else if (activeChannel != GetChannel(server))
                    {
                        network.Send("PRIVMSG " + activeChannel + " :" + temp, false);
                        activeChannel.AddEntry(string.Format("<{0}> {1}", nick, temp), true, ConsoleColor.DarkMagenta);
                    }
                    else
                    {
                        Console.WriteLine("Use the prefix '/' before typing a command.");
                    }
                    break;

                default:
                    command += character;
                    break;
            }
        }

        public virtual void Ping()
        {
            network.Send("PING :" + DateTime.Now, false);
            lastPingTime = DateTime.Now;
        }

        public virtual void HandleReceived(string received)
        {
            Channel channel;
            CommandResponse response = new CommandResponse(nick, received);

            string file = "parsing.txt";
            Log.WriteToFile(file, received, false);
            Log.WriteToFile(file, response.ToString(), false);

            bool isNumeric = int.TryParse(response.Command, out int number);

            switch ((ResponseID)number)
            {
                case ResponseID.RPL_WELCOME: ready = true; break;
                case ResponseID.RPL_AWAY: channel = GetChannel(response.GetParameter(0)); channel.HandleResponse(response, channel == activeChannel); return;
            }

            if (response.Command == "PING")
            {
                network.Send("PONG " + response.GetParameter(0), false);
                return;
            }

            if (response.Target != null)
                channel = GetChannel(response.Target);
            else if (response.Prefix != null && response.Prefix != server && !response.Prefix.StartsWith(nick))
                channel = GetChannel(response.Prefix.Split('!')[0]);
            else
                channel = GetChannel(server);

            channel.HandleResponse(response, channel == activeChannel);
        }

        public Channel GetChannel(string name)
        {
            if (channels.ContainsKey(name))
                return (Channel)channels[name];

            Log.WriteLine("! A new channel named '" + name + "' registered.", ConsoleColor.DarkRed);

            return (channels[name] = new Channel(name)) as Channel;
        }

        public void SetLayout()
        {
            List<string> keys = channels.Keys.Cast<string>().ToList();

            int index = keys.IndexOf(activeChannel.ToString());

            Console.Clear();
            activeChannel = channels[keys[++index < keys.Count ? index : 0]] as Channel;
            activeChannel.Load();
        }
    }
}
