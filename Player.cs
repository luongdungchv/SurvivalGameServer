using System.Net;
using System;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class Player
{
    public static HashSet<int> idOccupation;
    private TcpClient tcp;
    private NetworkStream tcpStream;
    private UdpClient udp;
    private Server server;
    private Room currentRoom;
    private string remoteHost;
    private int remotePort;
    private string name => $"{remoteHost}{remotePort}";
    private int id;
    private IPEndPoint remoteUDPEndpoint;
    public bool isReady;

    private byte[] receiveBuffer;
    private int bufferSize = 1024;
    public Player(Server server, TcpClient tcp)
    {
        this.tcp = tcp;
        this.tcpStream = tcp.GetStream();
        this.server = server;
        this.isReady = false;

        this.remoteHost = (tcp.Client.RemoteEndPoint as IPEndPoint).Address.ToString();

        this.remotePort = (tcp.Client.RemoteEndPoint as IPEndPoint).Port;
    }
    public void BeginReceiveTCPMsg()
    {
        receiveBuffer = new byte[bufferSize];
        tcpStream.BeginRead(receiveBuffer, 0, bufferSize, TcpReadCallback, 0);
    }
    public void BeginReceiveUDPMsg()
    {
        udp = new UdpClient(0);
        Console.WriteLine(udp.Client.LocalEndPoint);
        SendDataTCP($"2 {(udp.Client.LocalEndPoint as IPEndPoint).Port} {id} {currentRoom.mapSeed}");
        udp.BeginReceive(UdpReadCallback, null);
        //UDPReceiveAsync();
    }
    private void UdpReadCallback(IAsyncResult result)
    {
        try
        {
            var remoteEP = new IPEndPoint(IPAddress.Any, 0);
            var data = udp.EndReceive(result, ref remoteEP);
            if (data.Length <= 0)
            {
                Disconnect();
                return;
            }
            string msg = Encoding.ASCII.GetString(data);
            //Console.WriteLine($"Incoming UDP message from {remoteEP}: {msg}");
            if (msg == "con")
            {
                udp.Connect(remoteEP);
            }
            else this.currentRoom.BroadcastUDPMsg(msg, this);
            udp.BeginReceive(UdpReadCallback, null);
        }
        catch (Exception e)
        {
            Console.WriteLine($"error{e.ToString()}");
            Disconnect();
        }
    }
    private async void UDPReceiveAsync()
    {
        while (true)
        {
            try
            {
                var result = await udp.ReceiveAsync();
                var data = result.Buffer;
                var remoteEP = result.RemoteEndPoint;
                var msg = Encoding.ASCII.GetString(data);
                if (msg == "con")
                {
                    udp.Connect(remoteEP);
                }
                else this.currentRoom.BroadcastUDPMsg(msg, this);
            }
            catch (Exception e)
            {
                Console.WriteLine($"error{e.ToString()}");
                Disconnect();
                break;
            }
        }
    }
    private void TcpReadCallback(IAsyncResult result)
    {
        try
        {
            int dataLength = tcpStream.EndRead(result);
            if (dataLength <= 0)
            {
                Disconnect();
                Console.WriteLine(dataLength);
                return;
            }
            byte[] receiveData = new byte[dataLength];
            Array.Copy(receiveBuffer, receiveData, dataLength);
            string msg = Encoding.ASCII.GetString(receiveData);
            Console.WriteLine($"Message from client: {msg}");
            HandleTCPMessage(msg);
            tcpStream.BeginRead(receiveBuffer, 0, bufferSize, TcpReadCallback, 0);
        }
        catch
        {
            Disconnect();
        }
    }
    public void Disconnect()
    {
        Console.WriteLine($"A player disconnected: {remoteHost}:{remotePort}");
        LeaveRoom();
        tcp?.Close();
        udp?.Close();
        tcpStream = null;
        receiveBuffer = null;
        tcp = null;
    }
    public void SendDataTCP(string msg)
    {
        Console.WriteLine($"TCP Data to send: {msg}");
        var encodedMsg = Encoding.ASCII.GetBytes(msg);
        try
        {
            tcpStream.BeginWrite(encodedMsg, 0, encodedMsg.Length, null, null);
        }
        catch
        {
            Disconnect();
        }
    }
    private string lastUDP = "";
    public async void SendDataUDP(string msg)
    {
        var encodedMsg = Encoding.ASCII.GetBytes(msg);
        try
        {
            if (msg != lastUDP)
            {

                int sent = await udp.SendAsync(encodedMsg, encodedMsg.Length);
                lastUDP = msg;
            }
        }
        catch (Exception e)
        {
            Disconnect();
        }
    }
    private bool JoinRoom(Room room)
    {
        if (currentRoom != null) return false;
        if (room.AddPlayer(this))
        {
            currentRoom = room;
            Console.WriteLine($"{name} joins room {currentRoom.id}");
            return true;
        }
        return false;
    }
    public void LeaveRoom()
    {
        if (currentRoom != null && currentRoom.RemovePlayer(this))
        {
            Console.WriteLine($"{name} leaves room {currentRoom.id}");
            currentRoom = null;
        }
    }
    public void SetId(int _id)
    {
        this.id = _id;
    }
    private void HandleTCPMessage(string msg)
    {
        msg = msg.Trim();
        string[] split = msg.Split(" ");
        string cmd = split[0];
        if (cmd == "cr")
        {
            var randObj = new Random();
            var randomId = randObj.Next(10000, 99999);
            var room = this.server.AddRoom(randomId.ToString());
            while (room == null)
            {
                randomId = randObj.Next(10000, 99999);
                room = this.server.AddRoom(randomId.ToString());
            }
            room.host = this;
            JoinRoom(room);
            Console.WriteLine($"New room created: {room.id}");
        }
        else if (cmd == "jr")
        {
            Room room = this.server.GetRoom(split[1]);
            if (room != null) JoinRoom(room);
        }
        else if (cmd == "lv")
        {
            LeaveRoom();
        }
        else if (cmd == "rd")
        {
            if (currentRoom != null && currentRoom.host != this)
            {
                isReady = true;
            }
        }
        else if (cmd == "urd")
        {
            isReady = false;
            remoteUDPEndpoint = null;
            Console.WriteLine($"{name} is not ready");
        }
        else if (cmd == "st")
        {
            if (currentRoom != null && currentRoom.host == this)
            {

                isReady = true;
                currentRoom.mapSeed = split[1];
                var startGame = currentRoom.TryStartGame();
                if (!startGame)
                {
                    isReady = false;
                }
            }
        }
        else if (cmd.Length == 1 && Char.IsDigit(cmd[0]))
        {
            currentRoom.BroadcastTCPMsg(msg, this);
        }

    }

}
