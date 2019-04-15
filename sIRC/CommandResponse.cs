using System;
using System.Linq;

namespace sIRC
{
    class CommandResponse
    {
        private readonly string[] parameters;

        public string Prefix { get; }
        public string Command { get; }
        public string Recipient { get; }
        public string Target { get; }

        public CommandResponse(string nick, string message)
        {
            string[] parts = message.Split(' ');
            int i = 0;

            if (message[0] == ':')
                Prefix = parts[i++].Substring(1);

            Command = parts[i++];

            if (parts[i] == nick)
                Recipient = parts[i++];

            if (parts[i] == "@" || parts[i] == "=" || parts[i] == "*") //secret, private, public channel
                i++;

            if (IsChannel(parts[i]))
                Target = parts[i++];

            parameters = ConstructParameters(parts.Skip(i).ToArray());
        }

        private string[] ConstructParameters(string[] parts)
        {
            if (parts.Any(s => s.StartsWith(":")))
            {
                int index = Array.FindIndex(parts, s => s.StartsWith(":"));

                parts[index] = parts[index].Substring(1);

                string[] parameters = new string[index + 1];

                Array.Copy(parts, parameters, index);
                parameters[index] = string.Join(" ", parts.Skip(index).ToArray());

                return parameters;
            }

            return parts;
        }

        public string[] GetParameters()
        {
            return parameters;
        }

        public string GetParameter(int index)
        {
            return parameters[index];
        }

        public override string ToString()
        {
            return string.Format("from({0}) command({1}) recipient({2}) target({3}) params( {4} )", Prefix, Command, Recipient, Target, string.Join(" ", parameters));
        }

        public bool IsChannel(string value)
        {
            return value[0] == '#' || value[0] == '&' || value[0] == '!';
        }
    }
}
