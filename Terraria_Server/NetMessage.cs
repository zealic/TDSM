using System;
using System.Text;
using System.IO;

using Terraria_Server.Commands;
using Terraria_Server.Events;
using Terraria_Server.Messages;
using Terraria_Server.Misc;
using Terraria_Server.Logging;

namespace Terraria_Server
{
	public partial class NetMessage
	{
		public static NetMessage PrepareThreadInstance()
		{
			if (threadInstance == null)
			{
				threadInstance = new NetMessage (65535);
			}
			else
			{
				threadInstance.sink.Position = 0;
				threadInstance.lenAt = 0;
			}
			return threadInstance;
		}
		
		public byte[] Output
		{
			get
			{
				var copy = new byte [sink.Position];
				Array.Copy (buf, copy, sink.Position);
				return copy;
			}
		}
		
		public int Written
		{
			get
			{
				return (int)sink.Position;
			}
		}
		
		public NetMessage (int bufSize = 65535)
		{
			buf = new byte [bufSize];
			sink = new SealedMemoryStream (buf);
			bin = new SealedBinaryWriter (sink);
		}
		
		public void Clear ()
		{
			lenAt = 0;
			sink.Position = 0;
		}
		public static MessageBuffer[] buffer = new MessageBuffer[257];

		public static void BootPlayer(int plr, String msg)
		{
			Netplay.slots[plr].Kick (msg);
		}
		
		public static int SendData (int packetId, int remoteClient = -1, int ignoreClient = -1, String text = "", int number = 0, float number2 = 0f, float number3 = 0f, float number4 = 0f, int number5 = 0)
		{
			try
			{
				var msg = PrepareThreadInstance();
	
				switch (packetId)
				{
					case (int)Packet.CONNECTION_REQUEST:
						msg.ConnectionRequest (Statics.CURRENT_TERRARIA_RELEASE_STR);
						break;
	
					case (int)Packet.DISCONNECT:
						msg.Disconnect (text);
						break;
						
					case (int)Packet.CONNECTION_RESPONSE:
						msg.ConnectionResponse (remoteClient);
						break;
	
					case (int)Packet.PLAYER_DATA:
						msg.PlayerData (number);
						break;
						
					case (int)Packet.INVENTORY_DATA:
						msg.InventoryData (number, (byte)number2, text);
						break;
						
					case (int)Packet.WORLD_REQUEST:
						msg.WorldRequest ();
						break;
						
					case (int)Packet.WORLD_DATA:
						msg.WorldData ();
						break;
						
					case (int)Packet.REQUEST_TILE_BLOCK:
						msg.RequestTileBlock ();
						break;
						
					case (int)Packet.SEND_TILE_LOADING:
						msg.SendTileLoading (number, text);
						break;
						
					case (int)Packet.SEND_TILE_ROW:
						msg.SendTileRow (number, (int)number2, (int)number3);
						break;
						
					case (int)Packet.SEND_TILE_CONFIRM:
						msg.SendTileConfirm (number, (int)number2, (int)number3, (int)number4);
						break;
						
					case (int)Packet.RECEIVING_PLAYER_JOINED:
						msg.ReceivingPlayerJoined (number);
						break;
						
					case (int)Packet.PLAYER_STATE_UPDATE:
						msg.PlayerStateUpdate (number);
						break;
						
					case (int)Packet.SYNCH_BEGIN:
						msg.SynchBegin (number, (int)number2);
						break;
						
					case (int)Packet.UPDATE_PLAYERS:
						msg.UpdatePlayers ();
						break;
						
					case (int)Packet.PLAYER_HEALTH_UPDATE:
						msg.PlayerHealthUpdate (number);
						break;
						
					case (int)Packet.TILE_BREAK:
						msg.TileBreak (number, (int)number2, (int)number3, (int)number4, (int)number5);
						break;
						
					case (int)Packet.TIME_SUN_MOON_UPDATE:
						msg.TimeSunMoonUpdate ();
						break;
						
					case (int)Packet.DOOR_UPDATE:
						msg.DoorUpdate (number, (int)number2, (int)number3, (int)number4);
						break;
						
					case (int)Packet.TILE_SQUARE:
						msg.TileSquare (number, (int)number2, (int)number3);
						break;
						
					case (int)Packet.ITEM_INFO:
						msg.ItemInfo (number);
						break;
						
					case (int)Packet.ITEM_OWNER_INFO:
						msg.ItemOwnerInfo (number);
						break;
						
					case (int)Packet.NPC_INFO:
						msg.NPCInfo (number);
						break;
						
					case (int)Packet.STRIKE_NPC:
						msg.StrikeNPC (number, (int)number2);
						break;
						
					case (int)Packet.PLAYER_CHAT:
						msg.PlayerChat (number, text, (byte)number2, (byte)number3, (byte)number4);
						break;
						
					case (int)Packet.STRIKE_PLAYER:
						msg.StrikePlayer (number, text, (int)number2, (int)number3, (int)number4);
						break;
						
					case (int)Packet.PROJECTILE:
						msg.Projectile (Main.projectile[number]);
						break;
	
					case (int)Packet.DAMAGE_NPC:
						msg.DamageNPC (number, (int)number2, number3, (int)number4);
						break;
	
					case (int)Packet.KILL_PROJECTILE:
						msg.KillProjectile (number, (int)number2);
						break;
						
					case (int)Packet.PLAYER_PVP_CHANGE:
						msg.PlayerPVPChange (number);
						break;
	
					case (int)Packet.OPEN_CHEST:
						msg.OpenChest ();
						break;
						
					case (int)Packet.CHEST_ITEM:
						msg.ChestItem (number, (int)number2);
						break;
						
					case (int)Packet.PLAYER_CHEST_UPDATE:
						msg.PlayerChestUpdate (number);
						break;
	
					case (int)Packet.KILL_TILE:
						msg.KillTile ();
						break;
						
					case (int)Packet.HEAL_PLAYER:
						msg.HealPlayer (number, (int)number2);
						break;
						
					case (int)Packet.ENTER_ZONE:
						msg.EnterZone (number);
						break;
						
					case (int)Packet.PASSWORD_REQUEST:
						msg.PasswordRequest ();
						break;
						
					case (int)Packet.PASSWORD_RESPONSE:
						msg.PasswordResponse ();
						break;
						
					case (int)Packet.ITEM_OWNER_UPDATE:
						msg.ItemOwnerUpdate (number);
						break;
						
					case (int)Packet.NPC_TALK:
						msg.NPCTalk (number);
						break;
	
					case (int)Packet.PLAYER_BALLSWING:
						msg.PlayerBallswing (number);
						break;
						
					case (int)Packet.PLAYER_MANA_UPDATE:
						msg.PlayerManaUpdate (number);
						break;
						
					case (int)Packet.PLAYER_USE_MANA_UPDATE:
						msg.PlayerUseManaUpdate (number, (int)number2);
						break;
						
					case (int)Packet.KILL_PLAYER_PVP:
						msg.KillPlayerPVP (number, text, (int)number2, (int)number3, (int)number4);
						break;
						
					case (int)Packet.PLAYER_JOIN_PARTY:
						msg.PlayerJoinParty (number);
						break;
						
					case (int)Packet.READ_SIGN:
						msg.ReadSign (number, (int)number2);
						break;
						
					case (int)Packet.WRITE_SIGN:
						msg.WriteSign (number);
						break;
						
					case (int)Packet.FLOW_LIQUID:
						msg.FlowLiquid (number, (int)number2);
						break;
						
					case (int)Packet.SEND_SPAWN:
						msg.SendSpawn ();
						break;
						
					case (int)Packet.PLAYER_BUFFS:
						msg.PlayerBuffs (number);
						break;
						
					case (int)Packet.SUMMON_SKELETRON:
						msg.SummonSkeletron ();
						break;
						
					default:
						{
							//Unknown packet :3
							return 0;
						}
				}
					
				var bytes = msg.Output;
				if (remoteClient == -1)
				{
					for (int num11 = 0; num11 < 256; num11++)
					{
						if (num11 != ignoreClient && Netplay.slots[num11].state >= SlotState.PLAYING && Netplay.slots[num11].Connected)
						{
							NetMessage.buffer[num11].spamCount++;
							Netplay.slots[num11].Send (bytes);
						}
					}
					
				}
				else if (Netplay.slots[remoteClient].Connected)
				{
					NetMessage.buffer[remoteClient].spamCount++;
					Netplay.slots[remoteClient].Send (bytes);
				}
				return bytes.Length;
			}
			catch (Exception e)
			{
				ProgramLog.Log (e, "SendData error");
			}
			return 0;
		}
		
		public static void CheckBytes (int i)
		{
			var msgBuf = NetMessage.buffer[i];
			CheckBytes (i, msgBuf.readBuffer, ref msgBuf.totalData, ref msgBuf.messageLength);
		}
		
		public static void CheckBytes (int i, byte[] readBuffer, ref int totalData, ref int msgLen)
		{
			var msgBuf = NetMessage.buffer[i];
			var slot = Netplay.slots[i];
			int processed = 0;
			
			if (totalData >= 4)
			{
				if (msgLen == 0)
				{
					msgLen = BitConverter.ToInt32 (readBuffer, 0) + 4;
					
					if (msgLen == 0 || msgLen > 4096)
						slot.Kick ("Client sent invalid network message (" + msgLen + ")");
				}
				while (totalData >= msgLen + processed && msgLen > 0)
				{
					if (slot.state == SlotState.PLAYER_AUTH && msgLen > 4
						&& (Packet) readBuffer[processed + 4] != Packet.PASSWORD_RESPONSE)
					{
						// put player packets aside until password response
						
						if (readBuffer == msgBuf.sideBuffer)
							throw new Exception ("Sidebuffer reads shouldn't be done during USER_AUTH.");
						
						if (msgBuf.sideBufferBytes + msgLen > 4096)
						{
							slot.Kick ("Player data too big.");
							return;
						}
						
						if (msgBuf.sideBuffer == null) msgBuf.sideBuffer = new byte [4096];
						
						Buffer.BlockCopy (readBuffer, processed, msgBuf.sideBuffer, msgBuf.sideBufferBytes, msgLen);
						
						msgBuf.sideBufferBytes += msgLen;
					}
					else
						msgBuf.GetData (readBuffer, processed + 4, msgLen - 4);

					processed += msgLen;
					if (totalData - processed >= 4)
					{
						msgLen = BitConverter.ToInt32 (readBuffer, processed) + 4;
						
						if (msgLen == 0 || msgLen > 4096)
							slot.Kick ("Client sent invalid network message (" + msgLen + ")");
					}
					else
					{
						msgLen = 0;
					}
				}
				if (processed == totalData)
				{
					totalData = 0;
				}
				else
				{
					if (processed > 0)
					{
						Buffer.BlockCopy (readBuffer, processed, readBuffer, 0, totalData - processed);
						totalData -= processed;
					}
				}
			}
		}
		
		public static void SendTileSquare(int whoAmi, int tileX, int tileY, int size)
		{
			int num = (size - 1) / 2;
			float x = tileX - num;
			float y = tileY - num;
			NetMessage.SendData(20, whoAmi, -1, "", size, x, y, 0f);
		}

		public static void SendSection(int whoAmi, int sectionX, int sectionY)
		{
			try
			{
				var msg = NetMessage.PrepareThreadInstance();

				if (sectionX >= 0 && sectionY >= 0 && sectionX < Main.maxSectionsX && sectionY < Main.maxSectionsY)
				{
					Netplay.slots[whoAmi].tileSection[sectionX, sectionY] = true;
					int num = sectionX * 200;
					int num2 = sectionY * 150;
					for (int i = num2; i < num2 + 150; i++)
					{
						//NetMessage.SendData(10, whoAmi, -1, "", 200, (float)num, (float)i, 0f);
						msg.Clear ();
						msg.SendTileRow (200, num, i);
						Netplay.slots[whoAmi].Send (msg.Output); // tried sending as one big message, but it didn't work
					}
					
					//Console.WriteLine ("SendSection: {0} bytes", ts.stream.Position);
					//Netplay.slots[whoAmi].Send (ts.buffer, 0, (int)ts.stream.Position);
				}
				
			}
			catch
			{
			}
		}
		
		public static void Broadcast (byte[] bytes)
		{
			for (int k = 0; k < 255; k++)
			{
				if (Netplay.slots[k].state >= SlotState.PLAYING && Netplay.slots[k].Connected)
				{
					NetMessage.buffer[k].spamCount++;
					Netplay.slots[k].Send (bytes);
				}
			}
		}
		
		public static void BroadcastExcept (byte[] bytes, int i)
		{
			for (int k = 0; k < 255; k++)
			{
				if (Netplay.slots[k].state >= SlotState.PLAYING && Netplay.slots[k].Connected && k != i)
				{
					NetMessage.buffer[k].spamCount++;
					Netplay.slots[k].Send (bytes);
				}
			}
		}
		
		public void Broadcast ()
		{
			Broadcast (Output);
		}
		
		public void BroadcastExcept (int i)
		{
			BroadcastExcept (Output, i);
		}
		
		public void Send (int i)
		{
			Netplay.slots[i].Send (Output);
		}
		
		public static void OnPlayerJoined (int plr)
		{
			var msg = NetMessage.PrepareThreadInstance ();

			var motd = Program.properties.Greeting.Split('@');
			for (int i = 0; i < motd.Length; i++)
			{
				if (motd[i] != null && motd[i].Trim().Length > 0)
				{
					msg.PlayerChat (255, motd[i], 0, 0, 255);
				}
			}

			string list = "";
			for (int i = 0; i < 255; i++)
			{
				if (Main.players[i].Active)
				{
					if (list == "")
						list += Main.players[i].Name;
					else
						list = list + ", " + Main.players[i].Name;
				}
			}
			
			msg.PlayerChat (255, "Current players: " + list + ".", 255, 240, 20);
			msg.Send (plr); // send these before the login event, so messages from plugins come after
			
			var slot = Netplay.slots[plr];
			var player = Main.players[plr];
			
			PlayerLoginEvent loginEvent = new PlayerLoginEvent();
			loginEvent.Slot = slot;
			loginEvent.Sender = player;
			Program.server.PluginManager.processHook(Plugin.Hooks.PLAYER_LOGIN, loginEvent);
			
			if ((loginEvent.Cancelled || loginEvent.Action == PlayerLoginAction.REJECT) && (slot.state & SlotState.DISCONNECTING) == 0)
			{
				slot.Kick ("Disconnected by server.");
			}
			else
			{
				slot.announced = true;
				
				// to player
				msg.Clear();
				msg.SendSyncOthersForPlayer (plr);
				
				var ann = player.Name + " has joined.";
				Program.tConsole.WriteLine (ann);
				
				// to other players
				msg.Clear();
				msg.PlayerChat (255, ann, 255, 240, 20);
				msg.ReceivingPlayerJoined (plr);
				msg.SendSyncPlayerForOthers (plr); // broadcasts the preceding message too
			}
		}
		
		public static void OnPlayerLeft (Player player, bool announced)
		{
			player.Active = false;
			
			if (announced)
			{
				var ann = player.Name + " has left.";
				
				Program.tConsole.WriteLine (ann);
				
				var msg = NetMessage.PrepareThreadInstance();
				
				msg.SynchBegin (player.whoAmi, 0 /*inactive*/);
				msg.PlayerChat (255, ann, 255, 240, 20);
				
				msg.BroadcastExcept (player.whoAmi);
			}
			
			PlayerLogoutEvent Event = new PlayerLogoutEvent();
			Event.Slot = null;
			Event.Sender = player;
			Program.server.PluginManager.processHook(Plugin.Hooks.PLAYER_LOGOUT, Event);
		}

		public static void SendWater(int x, int y)
		{
			var msg = NetMessage.PrepareThreadInstance();
			msg.FlowLiquid (x, y);
			var bytes = msg.Output;
			
			for (int i = 0; i < 255; i++)
			{
				if (Netplay.slots[i].state >= SlotState.PLAYING && Netplay.slots[i].Connected)
				{
					int num = x / 200;
					int num2 = y / 150;
					if (Netplay.slots[i].tileSection[num, num2])
					{
						Netplay.slots[i].Send (bytes);
					}
				}
			}
		}
		
		public void BuildPlayerUpdate (int i)
		{
			var player = Main.players[i];
			
			SynchBegin (i, 1); //active players only
			PlayerStateUpdate (i);
			PlayerHealthUpdate (i);
			PlayerPVPChange (i);
			PlayerJoinParty (i);
			PlayerManaUpdate (i);
			PlayerBuffs (i);
			PlayerData (i);
			
			for (int k = 0; k < 44 /*bar only*/; k++)
			{
				InventoryData (i, k, player.inventory[k].Name);
			}
			
			for (int k = 0; k < 11; k++)
			{
				InventoryData (i, k+44, player.armor[k].Name);
			}
		}
		
		public static void SyncPlayers() /* always sends all updates to all players */
		{
			var msg = NetMessage.PrepareThreadInstance();
			
			for (int i = 0; i < 255; i++)
			{
				if (Netplay.slots[i].state == SlotState.PLAYING)
				{
					msg.Clear();
					msg.BuildPlayerUpdate (i);
					msg.BroadcastExcept (i);
				}
			}
			
			msg.Clear();
			
			for (int i = 0; i < 255; i++)
				if (Netplay.slots[i].state != SlotState.PLAYING)
					msg.SynchBegin (i, 0);
			
			msg.Broadcast ();
		}
		
		public void SendSyncOthersForPlayer (int i)
		{
			for (int k = 0; k < 255; k++)
			{
				if (Netplay.slots[k].state == SlotState.PLAYING && i != k)
					BuildPlayerUpdate (k);
				else if (i != k)
					SynchBegin (k, 0);

				if (Written >= 4096)
				{
					Send (i);
					Clear ();
				}
			}
			
			if (Written > 0) Send (i);
		}
		
		public void SendSyncPlayerForOthers (int i)
		{
			// send info about this player to others
			BuildPlayerUpdate (i);
			BroadcastExcept (i);
		}
		
		//
		// PRIVATES
		//
		
		private readonly SealedMemoryStream sink;
		private readonly SealedBinaryWriter bin;
		private readonly byte[] buf;
		private int lenAt;
		
		[ThreadStatic]
		private static NetMessage threadInstance;

		sealed class SealedMemoryStream : System.IO.MemoryStream
		{
			public SealedMemoryStream (byte[] buf) : base(buf) {}
		}
		
		sealed class SealedBinaryWriter : System.IO.BinaryWriter
		{
			public SealedBinaryWriter (Stream stream) : base(stream, Encoding.ASCII) {}
		}

		private void Begin ()
		{
			lenAt = (int) sink.Position;
			sink.Position += 4;
		}

		private void Begin (Packet id)
		{
			lenAt = (int) sink.Position;
			sink.Position += 4;
			sink.WriteByte ((byte) id);
		}
		
		private void End ()
		{
			var pos = sink.Position;
			sink.Position = lenAt;
			bin.Write ((int) (pos - lenAt - 4));
			sink.Position = pos;
		}
		
		private void Header (Packet id, int length)
		{
			bin.Write (length + 1);
			sink.WriteByte ((byte) id);
		}
		
		private void Byte (byte data)
		{
			sink.WriteByte (data);
		}

		private void Byte (int data)
		{
			sink.WriteByte ((byte) data);
		}

		private void Byte (bool data)
		{
			sink.WriteByte ((byte) (data ? 1 : 0));
		}
		
		private void Short (short data)
		{
			//sink.Write (BitConverter.GetBytes(data), 0, 2);
			bin.Write (data);
		}

		private void Short (int data)
		{
			Short ((short) data);
		}

		private void Int (int data)
		{
			//sink.Write (BitConverter.GetBytes(data), 0, 4);
			bin.Write (data);
		}
		
		private void Int (double data)
		{
			Int ((int) data);
		}

#if UNSAFE
		private unsafe void Float (float data)
		{
			var bytes = (byte*) &data;
			sink.WriteByte (bytes[0]);
			sink.WriteByte (bytes[1]);
			sink.WriteByte (bytes[2]);
			sink.WriteByte (bytes[3]);
		}
#else
		private void Float (float data)
		{
			sink.Write (BitConverter.GetBytes(data), 0, 4);
		}
#endif
		
		private void String (string data)
		{
			foreach (char c in data)
			{
				if (c < 128)
					sink.WriteByte ((byte) c);
				else
					sink.WriteByte ((byte) '?');
			}
		}
		
	}
}
