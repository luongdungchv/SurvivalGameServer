using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


public class Server
{
    private Dictionary<string, Room> roomList;
    private TcpListener server;
    private Random randomObj;
    public Server()
    {
        roomList = new Dictionary<string, Room>();
        randomObj = new Random(DateTime.Now.Millisecond);
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
        if (roomList.ContainsKey(id) && !roomList[id].started) return roomList[id];
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
    public Room AddRoomWithRandomID(){
        if(roomList.Count >= 100000) return null;
        var randomId = this.randomObj.Next(0, 99999).ToString();
        while(roomList.ContainsKey(randomId)){
            randomId = this.randomObj.Next(0, 99999).ToString();
        }
        var room = new Room(randomId, this);
        roomList.Add(randomId, room);
        return room;
    }
}