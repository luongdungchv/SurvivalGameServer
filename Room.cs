using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Microsoft.VisualBasic;

public class Room
{
    private List<Player> playerList = new List<Player>();
    private int maxPlayerCount = 4;
    public string mapSeed;
    private Player _host;
    public Player host { get => _host; set => _host = value; }
    public string id;
    private Server ownerServer;

    public Room(string id, Server owner)
    {
        this.id = id;
        this.ownerServer = owner;
    }

    public bool AddPlayer(Player newPlayer)
    {
        if (playerList.Count < maxPlayerCount)
        {
            newPlayer.SetId(playerList.Count);
            playerList.Add(newPlayer);
            return true;
        }
        return false;
    }
    public bool RemovePlayer(Player player)
    {
        var res = playerList.Remove(player);
        if (playerList.Count == 0) ownerServer.RemoveRoom(this.id);
        return res;
    }
    public bool TryStartGame()
    {
        foreach (var i in playerList)
        {
            if (!i.isReady)
            {
                return false;
            }
        }
        StartGame();
        return true;
    }
    public void BroadcastUDPMsg(string msg, Player broadcaster)
    {
        if (broadcaster == _host)
        {
            foreach (var i in playerList)
            {
                if (i == broadcaster) continue;
                i.SendDataUDP(msg);
            }
        }
        else
        {
            host.SendDataUDP(msg);
        }
    }
    public void BroadcastTCPMsg(string msg, Player broadcaster)
    {
        if (broadcaster == _host)
        {
            foreach (var i in playerList)
            {
                if (i == broadcaster) continue;
                i.SendDataTCP(msg);
            }
        }
        else
        {
            host.SendDataTCP(msg);
        }
    }
    private void StartGame()
    {
        Console.WriteLine("game start" + playerList.Count);
        foreach (var i in playerList)
        {
            i.BeginReceiveUDPMsg();
        }
    }

}