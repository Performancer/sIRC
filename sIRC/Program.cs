using System;
using System.IO;
using System.Reflection;

namespace sIRC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "sIRC - Silly Internet Relay Chat";
            Console.SetWindowSize(150, 50);

            Console.WriteLine("sIRC - Silly Internet Relay Chat " + Assembly.GetEntryAssembly().GetName().Version.ToString());
            Console.WriteLine("Alex Leppäkoski © 2019");

            string path = "login.txt";

            if (!File.Exists(path))
                File.Create(path);

            string server, user, nick;
            int port;

            try
            {
                using (StreamReader reader = new StreamReader(File.OpenRead(path)))
                {
                    server = reader.ReadLine().Split(':')[1];
                    port = int.Parse(reader.ReadLine().Split(':')[1]);
                    user = reader.ReadLine();
                    user = user.Split(':')[1] + ":" + user.Split(':')[2];
                    nick = reader.ReadLine().Split(':')[1];
                }
            }
            catch
            {
                Console.WriteLine("Invalid login.txt file.");
                Console.WriteLine("server:ADDRESS");
                Console.WriteLine("port:PORT (4 digits)");
                Console.WriteLine("user:USER (UserName * * :Real Name)");
                Console.WriteLine("nick:NICK");
                Console.WriteLine("Press any key to exit...");

                Console.ReadKey(true);
                return;
            }

            Client client = new Client(server, port, user, nick);
            client.Start();
        }
    }
}
