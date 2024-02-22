[1mdiff --git a/Player.cs b/Player.cs[m
[1mindex 196107c..3ef0672 100644[m
[1m--- a/Player.cs[m
[1m+++ b/Player.cs[m
[36m@@ -59,7 +59,7 @@[m [mpublic class Player[m
                 return;[m
             }[m
             string msg = Encoding.ASCII.GetString(data);[m
[31m-            Console.WriteLine($"Incoming udp msg from {name}: " + msg);[m
[32m+[m[32m            //Console.WriteLine($"Incoming udp msg from {name}: " + msg);[m
             remoteUDPEndpoint = remoteEP;[m
             this.currentRoom.BroadcastUDPMsg(msg, this);[m
             udp.BeginReceive(UdpReadCallback, null);[m
[36m@@ -158,9 +158,9 @@[m [mpublic class Player[m
         if (currentRoom != null && currentRoom.RemovePlayer(this))[m
         {[m
             Console.WriteLine($"{name} leaves room {currentRoom.id}");[m
[31m-            currentRoom.BroadcastTCPMsg($"14 {id}~", this);[m
[31m-            currentRoom.BroadcastTCPMsg($"9 leave {id}~", this);[m
[31m-            this.SendDataTCP($"9 leave {id}");[m
[32m+[m[32m            currentRoom.BroadcastTCPMsg($"{(int)PacketType.PlayerDisconnect} {id}~", this);[m
[32m+[m[32m            currentRoom.BroadcastTCPMsg($"{(int)PacketType.RoomInteraction} leave {id}~", this);[m
[32m+[m[32m            this.SendDataTCP($"{(int)PacketType.RoomInteraction} leave {id}");[m
             if (this.id == 0)[m
             {[m
                 currentRoom.DisposeRoom();[m
[36m@@ -188,6 +188,7 @@[m [mpublic class Player[m
         {[m
             Console.WriteLine("asdf " + this.name);[m
             var room = this.server.AddRoomWithRandomID();[m
[32m+[m[32m            //var room = this.server.AddRoom("12345");[m
             room.host = this;[m
             if (split.Length > 2) this.name = split[2].Trim('~');[m
             bool joinSuccess = JoinRoom(room);[m
[36m@@ -195,7 +196,7 @@[m [mpublic class Player[m
             if (joinSuccess)[m
             {[m
                 this.currentRoom.mapSeed = split[1].Trim('~');[m
[31m-                this.SendDataTCP($"9 create {currentRoom.id} {currentRoom.mapSeed} {name}");[m
[32m+[m[32m                this.SendDataTCP($"{(int)PacketType.RoomInteraction} create {currentRoom.id} {currentRoom.mapSeed} {name}");[m
             }[m
             Console.WriteLine($"New room created: {room.id}");[m
             [m
[36m@@ -210,8 +211,8 @@[m [mpublic class Player[m
                 if (joinSuccess)[m
                 {[m
                     Console.WriteLine(msg);[m
[31m-                    this.currentRoom.BroadcastTCPMsg("9 room_add " + this.name, this);[m
[31m-                    var joinMsg = $"9 join {this.currentRoom.id} {this.currentRoom.mapSeed} {this.currentRoom.GetPlayerNames()}";[m
[32m+[m[32m                    this.currentRoom.BroadcastTCPMsg($"{(int)PacketType.RoomInteraction} room_add " + this.name, this);[m
[32m+[m[32m                    var joinMsg = $"{(int)PacketType.RoomInteraction} join {this.currentRoom.id} {this.currentRoom.mapSeed} {this.currentRoom.GetPlayerNames()}";[m
                     //Console.WriteLine(joinMsg);[m
                     this.SendDataTCP(joinMsg);[m
                 }[m
[36m@@ -226,8 +227,8 @@[m [mpublic class Player[m
             if (currentRoom != null && currentRoom.host != this)[m
             {[m
                 isReady = !isReady;[m
[31m-                this.currentRoom.BroadcastTCPMsg($"9 ready {id} {(isReady ? 1 : 0)}~", this);[m
[31m-                this.SendDataTCP($"9 ready {id} {(isReady ? 1 : 0)}~");[m
[32m+[m[32m                this.currentRoom.BroadcastTCPMsg($"{(int)PacketType.RoomInteraction} ready {id} {(isReady ? 1 : 0)}~", this);[m
[32m+[m[32m                this.SendDataTCP($"{(int)PacketType.RoomInteraction} ready {id} {(isReady ? 1 : 0)}~");[m
             }[m
         }[m
         else if (cmd == "urd")[m
[1mdiff --git a/Program.cs b/Program.cs[m
[1mindex aca075e..2c9cd54 100644[m
[1m--- a/Program.cs[m
[1m+++ b/Program.cs[m
[36m@@ -9,11 +9,6 @@[m [mnamespace SurvivalGameServer[m
         {[m
             Server server = new Server();[m
             server.StartServer(1234);[m
[31m-            Console.WriteLine((int)PacketType.StartGame);[m
[31m-            [m
[31m-            for(int i = 0; i < 5; i++){[m
[31m-                Console.WriteLine(Utilities.RandomSeed());[m
[31m-            }[m
             [m
             Console.Read();[m
         }[m
[36m@@ -21,5 +16,10 @@[m [mnamespace SurvivalGameServer[m
 }[m
 public enum PacketType[m
 {[m
[31m-    MovePlayer, SpawnPlayer, StartGame[m
[31m-}[m
\ No newline at end of file[m
[32m+[m[32m    MovePlayer,[m
[32m+[m[32m    SpawnPlayer, StartGame, Input, SpawnObject, UpdateEquipping,[m
[32m+[m[32m    FurnaceServerUpdate, FurnaceClientMsg,[m
[32m+[m[32m    ItemDrop, RoomInteraction,[m
[32m+[m[32m    SpawnEnemy, UpdateEnemy,PowerupInteraction,[m
[32m+[m[32m    ChestInteraction, ItemDropObjInteraction, OreInteraction, DestroyObject, PlayerDisconnect, PlayerInteraction,[m
[32m+[m[32m}[m
