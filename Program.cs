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
            
            Console.Read();
        }
    }
}
public enum PacketType
{
    MovePlayer,
    SpawnPlayer, StartGame, Input, SpawnObject, UpdateEquipping,
    FurnaceServerUpdate, FurnaceClientMsg,
    ItemDrop, RoomInteraction,
    SpawnEnemy, UpdateEnemy,PowerupInteraction,
    ChestInteraction, ItemDropObjInteraction, OreInteraction, DestroyObject, PlayerDisconnect, PlayerInteraction,
}
