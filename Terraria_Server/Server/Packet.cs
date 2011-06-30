﻿

namespace Terraria_Server
{
    public enum Packet
    {
        CONNECTION_REQUEST = 0x01,
        DISCONNECT = 0x02,
        CONNECTION_RESPONSE = 0x03,
        PLAYER_DATA = 0x04,
        INVENTORY_DATA = 0x05,
        WORLD_REQUEST = 0x06,
        WORLD_DATA = 0x07,
        REQUEST_TILE_BLOCK = 0x08,
        SEND_TILE_LOADING = 0x09,
        SEND_TILE_LOADING_MESSAGE = 0x0A,
        SEND_TILE_CONFIRM = 0x0B,
        RECEIVING_PLAYER_JOINED = 0x0C,
        PLAYER_STATE_UPDATE = 0x0D,
        SYNCH_BEGIN = 0x0E,
        UPDATE_PLAYERS = 0x0F,
        PLAYER_HEALTH_UPDATE = 0x10, //16
        TILE_BREAK = 0x11,
        TIME_SUN_MOON_UPDATE = 0x12,
        DOOR_UPDATE = 0x13,
        TILE_SQUARE = 0x14,
        ITEM_INFO = 0x15,
        ITEM_OWNER_INFO = 0x16,
        NPC_INFO = 0x17,
        STRIKE_NPC = 0x18,
        PLAYER_CHAT = 0x19,
        STRIKE_PLAYER = 0x1A,
        PROJECTILE = 0x1B,
        DAMAGE_NPC = 0x1C,
        KILL_PROJECTILE = 0x1D,
        PLAYER_PVP_CHANGE = 0x1E,
        OPEN_CHEST = 0x1F,
        CHEST_ITEM = 0x20, //32
        PLAYER_CHEST_UPDATE = 0x21,
        KILL_TILE = 0x22,
        HEAL_PLAYER = 0x23,
        ENTER_ZONE = 0x24,
        PASSWORD_REQUEST = 0x25,
        PASSWORD_RESPONSE = 0x26,
        ITEM_OWNER_UPDATE = 0x27,
        NPC_TALK = 0x28,
        PLAYER_BALLSWING = 0x29,
        PLAYER_MANA_UPDATE = 0x2A,
        PLAYER_USE_MANA_UPDATE = 0x2B,
        KILL_PLAYER_PVP = 0x2C,
        PLAYER_JOIN_PARTY = 0x2D,
        READ_SIGN = 0x2E,
        WRITE_SIGN = 0x2F,
        FLOW_LIQUID = 0x30,
        SEND_SPAWN = 0x31,
        PLAYER_BUFFS = 0x32,
        SUMMON_SKELETRON = 0x33,
    }
}
