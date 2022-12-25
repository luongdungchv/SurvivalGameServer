using System;
using System.Net.Security;

namespace SurvivalGameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.StartServer(1234);
            Console.WriteLine((int)PacketType.StartGame);
            Console.Read();
        }
    }
}
public enum PacketType
{
    MovePlayer, SpawnPlayer, StartGame
}