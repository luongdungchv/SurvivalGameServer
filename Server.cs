using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


public class Server
{
    private Dictionary<string, Room> roomList;
    private TcpListener server;
    public Server()
    {
        roomList = new Dictionary<string, Room>();
    }
    public void StartServer(int port)
    {
        this.server = new TcpListener(IPAddress.Any, port);
        server.Start();
        Console.WriteLine("ver 1.0");
        Console.WriteLine($"Server started on port: {port}");
        server.BeginAcceptTcpClient(TcpAcceptClientCallback, null);
    }
    public Room GetRoom(string id)
    {
        if (roomList.ContainsKey(id)) return roomList[id];
        return null;
    }
    public Room AddRoom(string id)
    {
        if (roomList.ContainsKey(id)) return null;
        var room = new Room(id, this);
        roomList.Add(id, room);
        return room;
    }
    public bool RemoveRoom(string id)
    {
        return roomList.Remove(id);
    }
    private void TcpAcceptClientCallback(IAsyncResult result)
    {
        var client = server.EndAcceptTcpClient(result);
        Console.WriteLine("Incoming connection from " + client.Client.RemoteEndPoint);
        var newPlayer = new Player(this, client);
        newPlayer.BeginReceiveTCPMsg();
        server.BeginAcceptTcpClient(TcpAcceptClientCallback, null);

    }
}