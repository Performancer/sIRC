using System;
using System.Collections.Generic;
using System.IO;

namespace sIRC
{
    class Channel
    {
        private struct Entry
        {
            public string Text {get;}
            public int Color { get; }

            public Entry(string text, int color)
            {
                Text = text;
                Color = color;
            }
        }

        private readonly string name;

        private List<string> members;
        private List<string> membersBuffer;

        private List<Entry> entries;

        public Channel(string name)
        {
            this.name = name;

            membersBuffer = new List<string>();
            entries = new List<Entry>();
        }

        public void Load()
        {
            foreach (Entry entry in entries)
                Log.WriteLine(entry.Text, (ConsoleColor)entry.Color);
        }

        public void HandleResponse(CommandResponse response, bool active)
        {
            //TODO: refactor and fix

            string message = null;
            ConsoleColor color = ConsoleColor.DarkGreen;

            bool isNumeric = int.TryParse(response.Command, out int number);

            if (isNumeric)
            {
                switch ((ResponseID)number)
                {
                    case ResponseID.RPL_TOPIC: message = string.Format("* Topic is: '{0}'", response.GetParameter(0)); break;
                    case ResponseID.RPL_NOTOPIC: message = "No topic is set."; break;
                    case ResponseID.RPL_TOPICSET:

                        string from = response.GetParameter(0);
                        long timestamp = int.Parse(response.GetParameter(1));

                        DateTime time = new DateTime(1970, 1, 1).AddSeconds(timestamp).ToLocalTime();
                        message = string.Format("* Set by {0} on {1}", from, time); break;

                    case ResponseID.RPL_NAMREPLY: membersBuffer.AddRange(response.GetParameter(0).Split(' ')); break;

                    case ResponseID.RPL_ENDOFNAMES:
                        members = membersBuffer;
                        membersBuffer = new List<string>();

                        message = string.Format("* Names in Channel: {0}", string.Join(" ", members.ToArray()));
                        break;

                    //private messaging
                    case ResponseID.RPL_AWAY: message = string.Format("{0} is away: {1}", response.GetParameter(0), response.GetParameter(1)); break;

                    default: message = string.Join(" ", response.GetParameters()); break;
                }
            }
            else
            {
                string name = null;
                string info = null;

                if (response.Prefix != null)
                {
                    string[] split = response.Prefix.Split('!');

                    if (split.Length >= 1)
                    {
                        name = split[0];

                        if (split.Length >= 2)
                            info = split[1];
                    }
                }

                switch (response.Command)
                {
                    case "JOIN": message = string.Format("{0} ({1}) has joined the channel", name, info); break;
                    case "PART": message = string.Format("{0} ({1}) has left the channel", name, info); color = ConsoleColor.Blue; break;
                    case "TOPIC": message = string.Format("{0} has set the topic: {1}", name, response.GetParameter(0)); break;
                    case "PRIVMSG": message = string.Format("<{0}> {1}", name, response.GetParameter(0)); color = ConsoleColor.Gray; break;

                    case "MODE":
                        if (response.Target != null)
                            message = string.Format("{0} sets mode: {1}", name, string.Join(" ", response.GetParameters()));
                        else
                            message = string.Format("Usermode changed: {0}", string.Join(" ", response.GetParameters()));
                        break;

                    case "PONG": message = string.Format("{0} {1}", response.Command, response.GetParameter(1)); break;

                    case "NICK": message = string.Format("{0} has changed nick to {1}", response.Prefix.Split('!')[0], response.GetParameter(0)); break;
                    case "QUIT": message = string.Format("{0} has quit IRC ({1})", response.Prefix.Split('!')[0], response.GetParameter(0)); color = ConsoleColor.Blue; break;

                    case "NOTICE":

                        if (response.GetParameter(0) == "AUTH")
                            message = string.Format("{0}", response.GetParameter(1));
                        else
                            message = string.Format("<{0}> {1}", name, response.GetParameter(0));
                        break;
                    default: message = response.Command + " " + string.Join(" ", response.GetParameters()); color = ConsoleColor.Red; break;
                }
            }

            if (message != null)
                AddEntry(message, active, color);
        }

        public void AddEntry(string message, bool active, ConsoleColor color)
        {
            string full = string.Format("[{0}][{1}] {2}", DateTime.Now.ToString("HH:mm:ss"), name, message);

            entries.Add(new Entry(full, (int)color));

            if (active)
                Log.WriteLine(full, color);

            Log.WriteToFile(name + ".txt", message, true);
        }

        public override string ToString()
        {
            return name;
        }
    }
}
