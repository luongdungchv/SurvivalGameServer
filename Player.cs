using System.Net;
using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

public class Player
{
    public static HashSet<int> idOccupation;
    private TcpClient tcp;
    private NetworkStream tcpStream;
    private UdpClient udp;
    private Server server;
    public Room currentRoom;
    private string remoteHost;
    private int remotePort;
    public string name;
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
        this.name = $"{remoteHost}{remotePort}";
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
        Console.WriteLine($"Player {name} begins to receive udp msg");
        udp.BeginReceive(UdpReadCallback, null);
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
            Console.WriteLine($"Incoming udp msg from {name}: " + msg);
            remoteUDPEndpoint = remoteEP;
            this.currentRoom.BroadcastUDPMsg(msg, this);
            udp.BeginReceive(UdpReadCallback, null);
        }
        catch (Exception e)
        {
            Console.WriteLine($"error{e.ToString()}");
            Disconnect();
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
            Console.WriteLine($"Message from client {name}: {msg}");
            HandleTCPMessage(msg);
            tcpStream.BeginRead(receiveBuffer, 0, bufferSize, TcpReadCallback, 0);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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
        Console.WriteLine($"TCP msg send to {name}: {msg}");
        var encodedMsg = Encoding.ASCII.GetBytes(msg);
        try
        {
            tcpStream.BeginWrite(encodedMsg, 0, encodedMsg.Length, null, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Disconnect();
        }
    }
    private string lastUDP = "";
    public void SendDataUDP(string msg)
    {
        if (msg.Substring(0, 3) == "0 1")
        {
            if (msg != lastUDP)
            {
                lastUDP = msg;
            }

        }
        var encodedMsg = Encoding.ASCII.GetBytes(msg);
        try
        {
            udp.BeginSend(encodedMsg, encodedMsg.Length, remoteUDPEndpoint, null, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Disconnect();
        }
    }
    private bool JoinRoom(Room room)
    {
        if (currentRoom != null) return false;
        //Console.WriteLine($"{name} joins room {currentRoom.id}");
        Console.WriteLine(currentRoom);
        if (room.AddPlayer(this))
        {
            currentRoom = room;
            return true;
        }
        return false;
    }
    public void LeaveRoom()
    {
        if (currentRoom != null && currentRoom.RemovePlayer(this))
        {
            Console.WriteLine($"{name} leaves room {currentRoom.id}");
            currentRoom.BroadcastTCPMsg($"14 {id}~", this);
            currentRoom.BroadcastTCPMsg($"9 leave {id}~", this);
            this.SendDataTCP($"9 leave {id}");
            if (this.id == 0)
            {
                currentRoom.DisposeRoom();
            }
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
        string cmd = split[0].Trim('~');
        if (msg == "udp")
        {
            udp = new UdpClient(0);
            SendDataTCP($"2 {(udp.Client.LocalEndPoint as IPEndPoint).Port}");
            udp.BeginReceive(UdpReadCallback, null);
        }
        if (cmd == "cr")
        {
            Console.WriteLine("asdf " + this.name);
            var room = this.server.AddRoomWithRandomID();
            room.host = this;
            if (split.Length > 2) this.name = split[2].Trim('~');
            bool joinSuccess = JoinRoom(room);
            currentRoom.mapSeed = Utilities.RandomSeed();
            if (joinSuccess)
            {
                this.currentRoom.mapSeed = split[1].Trim('~');
                this.SendDataTCP($"9 create {currentRoom.id} {currentRoom.mapSeed} {name}");
            }
            Console.WriteLine($"New room created: {room.id}");
            
        }
        else if (cmd == "jr")
        {
            Room room = this.server.GetRoom(split[1].Trim('~'));
            if (split.Length > 2) this.name = split[2].Trim('~');
            if (room != null)
            {
                bool joinSuccess = JoinRoom(room);
                if (joinSuccess)
                {
                    Console.WriteLine(msg);
                    this.currentRoom.BroadcastTCPMsg("9 room_add " + this.name, this);
                    var joinMsg = $"9 join {this.currentRoom.id} {this.currentRoom.mapSeed} {this.currentRoom.GetPlayerNames()}";
                    //Console.WriteLine(joinMsg);
                    this.SendDataTCP(joinMsg);
                }
            }
        }
        else if (cmd == "lv")
        {
            LeaveRoom();
        }
        else if (cmd == "rd")
        {
            if (currentRoom != null && currentRoom.host != this)
            {
                isReady = !isReady;
                this.currentRoom.BroadcastTCPMsg($"9 ready {id} {(isReady ? 1 : 0)}~", this);
                this.SendDataTCP($"9 ready {id} {(isReady ? 1 : 0)}~");
            }
        }
        else if (cmd == "urd")
        {
            isReady = false;
            remoteUDPEndpoint = null;
            Console.WriteLine($"{name} is not ready");
        }
        else if (cmd == "set_name")
        {
            this.name = split[1].Trim('~');
        }
        else if (cmd == "st")
        {
            if (currentRoom != null && currentRoom.host == this)
            {
                isReady = true;
                var seedMsg = split[1].Trim('~');
                currentRoom.mapSeed = seedMsg == "0"  ? Utilities.RandomSeed() : split[1];
                var startGame = currentRoom.TryStartGame();
                if (!startGame)
                {
                    isReady = false;
                }
            }
        }
        else if (cmd == "con")
        {
            //this.udp.Connect(remoteHost, int.Parse(split[1].Trim('~')));
            remoteUDPEndpoint = new IPEndPoint(IPAddress.Parse(remoteHost), int.Parse(split[1].Trim('~')));
        }
        else if (cmd.Length <= 2 && Char.IsDigit(cmd[0]))
        {
            currentRoom.BroadcastTCPMsgViaHost(msg, this);
        }

    }

}
