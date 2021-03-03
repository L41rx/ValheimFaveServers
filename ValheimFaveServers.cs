using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Net;
using System;

namespace ValheimFaveServers
{
    [BepInPlugin(vfsGuid, "Valheim Fave Servers", "0.0.1")]
    [BepInProcess("valheim.exe")]
    public class ValheimFaveServers : BaseUnityPlugin
    {
        public const string vfsGuid = "net.l41rx.valheimfaveservers";
        public static ConfigEntry<string> faveServerIps;
        public static string[] faveServerIpsArray;
        public static ConfigFile config;
        public static string gameVersion;

        void Awake()
        {
            // Config
            ValheimFaveServers.faveServerIps = Config.Bind("General", "Server IP Addresses", "", "List of IP addresses to add as favourite servers, comma separated, no spaces");
            ValheimFaveServers.config = Config;
            ValheimFaveServers.updateFromConfig();

            // Patching
            Harmony harmony = new Harmony(vfsGuid);
            harmony.PatchAll();
        }
        public static void updateFromConfig()
        {
            ValheimFaveServers.config.Reload();
            if (ValheimFaveServers.faveServerIps.Value != "" && ValheimFaveServers.faveServerIps.Value != " ")
                ValheimFaveServers.faveServerIpsArray = faveServerIps.Value.Split(',');
            else
                ValheimFaveServers.faveServerIpsArray = new string[] { };
        }
        
    }

    [HarmonyPatch(typeof(FejdStartup))]
    [HarmonyPatch("Awake")]
    public class GetGameVersion
    {
        static void Postfix(FejdStartup __instance)
        {
            ValheimFaveServers.gameVersion = __instance.m_versionLabel.text.Remove(0, 7); // cheating lol
            UnityEngine.Debug.Log("Saving game version as" + ValheimFaveServers.gameVersion);
        }
    }

    [HarmonyPatch(typeof(ZSteamMatchmaking))]
    [HarmonyPatch("GetServers")]
    public class PatchUpdateServerListGui
    {
        static void Postfix(ref List<ServerData> allServers)
        {
            UnityEngine.Debug.Log("Executing post patch for GetServers as part of Valheim Fave Servers");
            int i = 0;
            bool shouldInsert = true;
            foreach (string serverIp in ValheimFaveServers.faveServerIpsArray)
            {
                if (i < allServers.Count)
                {
                    if (allServers[i].m_host == serverIp)
                    {
                        shouldInsert = false;
                    }
                }
            }

            if (shouldInsert)
            {
                foreach (string serverIp in ValheimFaveServers.faveServerIpsArray)
                {
                    ServerData s = new ServerData();
                    s.m_host = serverIp;
                    s.m_name = "!!!VFS: " + serverIp;
                    s.m_password = true;
                    s.m_players = 0;
                    s.m_port = 2456;
                    s.m_steamHostAddr = new Steamworks.SteamNetworkingIPAddr();
                    s.m_steamHostAddr.SetIPv4(Ip.toInt(serverIp), 2456);
                    s.m_upnp = true;
                    s.m_version = ValheimFaveServers.gameVersion;

                    allServers.Insert(0, s);
                    UnityEngine.Debug.Log("Inserting VFS with host " + s.m_host);
                }
            } else
            {
                UnityEngine.Debug.Log("Skipped inserts as it appears complete");
            }
        }
    }

    public class Ip
    {
        public static uint toInt(string ipAddress)
        {
            var address = IPAddress.Parse(ipAddress);
            byte[] bytes = address.GetAddressBytes();

            // flip big-endian(network order) to little-endian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static string toString(uint ipAddress)
        {
            byte[] bytes = BitConverter.GetBytes(ipAddress);

            // flip little-endian to big-endian(network order)
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return new IPAddress(bytes).ToString();
        }
    }
}
