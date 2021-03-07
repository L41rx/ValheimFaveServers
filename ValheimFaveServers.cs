using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Net;
using System;
using System.Collections;
using System.Linq;
using System.ComponentModel;
using Steamworks;
using BepInEx.Logging;
using System.Globalization;

namespace ValheimFaveServers
{
    [BepInPlugin(ValheimFaveServers.pluginGuid, ValheimFaveServers.pluginName, ValheimFaveServers.pluginVersion)]
    [BepInProcess("valheim.exe")]
    public class ValheimFaveServers : BaseUnityPlugin
    {
        // plugin info
        public const string pluginGuid = "net.l41rx.valheimfaveservers";
        public const string pluginName = "ValheimFaveServers";
        public const string pluginVersion = "1.0.1";
        // config entries
        public string configVersion;
        public string configPrefix;
        public string[] configServerKeys;
        public ValheimFaveServerData[] configServers;
        // Track actions taken by game
        public SteamNetworkingIPAddr gameServerIp;
        // Static reference to plugin instance
        public static ValheimFaveServers instance;
        // Expose logger instance
        public ManualLogSource _Logger;

        void Awake()
        {
            // Config
            ValheimFaveServers.instance = this; // used to refer to the plugins settings in the patches
            this._Logger = this.Logger; // what is protected becomes public
            this.updateConfig();

            // Patching
            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
        }

        private void updateConfig()
        {
            // this.Config.Reload(); // I do this because I saw CameraMod do it or something idk
            this.configVersion = this.Config.Bind("General.ConfigVersion", "Version", ValheimFaveServers.pluginVersion, "Compatibility for older versions of the mod. Defaults to current mod version (" + ValheimFaveServers.pluginVersion + ") on new install. Update this if you notice new settings don't take effect.").Value;
            this.configPrefix = this.Config.Bind("General.Prefix", "Server name prefix", "! ", "Text to show before server name in browser. Defaults to ! to exploit sort order").Value;
            ConfigEntry<string> skeys = this.Config.Bind("General.ServerKeys", "Keys of enabled servers (comma separated, no spaces)", "cc", "How many servers do you want to favourite? Enter a 'key' for each. Save and launch+close Valheim to automatically generate the configuration for each, then fill them in.");
            if (skeys.Value.Trim() != "")
                this.configServerKeys = skeys.Value.Split(',');
            else
                this.configServerKeys = new string[] { }; ;

            this.configServers = new ValheimFaveServerData[this.configServerKeys.Length];
            int i = 0;
            foreach (string skey in this.configServerKeys)
            {
                bool error = false;
                ValheimFaveServerData vfsd = new ValheimFaveServerData();
                // key
                vfsd.Key = skey;
                // name
                ConfigEntry<string> nameEntry = this.Config.Bind("Server." + vfsd.Key + ".name", "Server name", "Comradely Conquest", "Server name to show in browser.");
                vfsd.Name = nameEntry.Value.Trim();
                // host
                ConfigEntry<string> hostEntry = this.Config.Bind("Server." + vfsd.Key + ".host", "Server host", "marxist.ca", "Hostname for the server (IPv4, IPv6, or domain names supported)");
                vfsd.Host = hostEntry.Value.Trim();

                // Is the host valid?
                bool isDomain = false;
                bool isIpv4 = false;
                bool isIpv6 = false;
                IPAddress address;
                if (IPAddress.TryParse(vfsd.Host, out address))
                {
                    switch (address.AddressFamily)
                    {
                        case System.Net.Sockets.AddressFamily.InterNetwork:
                            isIpv4 = true;
                            vfsd.Ip = IPAddress.Parse(vfsd.Host).MapToIPv6();
                            break;
                        case System.Net.Sockets.AddressFamily.InterNetworkV6:
                            isIpv6 = true;
                            vfsd.Ip = IPAddress.Parse(vfsd.Host);
                            break;
                        default:
                            break;
                    }
                }
                if (!isIpv4 && !isIpv6)
                {
                    if (Uri.CheckHostName(vfsd.Host) != UriHostNameType.Unknown)
                    {
                        isDomain = true;
                        IPAddress[] ipaddress = Dns.GetHostAddresses(vfsd.Host);
                        foreach (IPAddress ip4 in ipaddress.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
                        {
                            vfsd.Ip = ip4.MapToIPv6();
                        }
                        foreach (IPAddress ip6 in ipaddress.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6))
                        {
                            vfsd.Ip = ip6;
                        }
                    }
                }
                if (!isIpv4 && !isIpv6 && !isDomain)
                {
                    this.Logger.LogError("Host for server with key '" + vfsd.Key + "' cannot be understood, please use a valid domain name or IP address");
                    error = true;
                }

                // port
                ConfigEntry<int> portEntry = this.Config.Bind("Server." + vfsd.Key + ".port", "Server port", 2456, "Server port (default 2456)");
                vfsd.Port = portEntry.Value;
                // password
                ConfigEntry<string> passEntry = this.Config.Bind("Server." + vfsd.Key + ".password", "Server password", "1917", "Prefill server password");
                vfsd.Password = passEntry.Value.Trim();
                if (!error)
                {
                    this.configServers[i] = vfsd;
                    this.Logger.LogInfo("Loaded server from config with data " + vfsd.toString());
                } else
                {
                    this.Logger.LogError("Did not load server from config with key '" + vfsd.Key + "', there was an error");
                }
                i++;
            }
            this.Config.Save();
        }
    }

    /**
     * Inject favourite servers into server list
     */
    [HarmonyPatch(typeof(ZSteamMatchmaking))]
    [HarmonyPatch("GetServers")]
    public class PatchUpdateServerListGui
    {
        static void Postfix(ref List<ServerData> allServers)
        {
            int i = 0;
            bool shouldInsert = true;
            foreach (ValheimFaveServerData vsd in ValheimFaveServers.instance.configServers)
            {
                if (i < allServers.Count)
                {
                    if (allServers[i++].m_host == vsd.Host)
                    {
                        shouldInsert = false;
                    }
                }
            }

            if (shouldInsert)
            {
                foreach (ValheimFaveServerData server in ValheimFaveServers.instance.configServers)
                {
                    ServerData s = new ServerData();
                    s.m_host = server.Host;
                    s.m_name = ValheimFaveServers.instance.configPrefix + " " + server.Name;
                    s.m_password = true;
                    s.m_players = 0;
                    s.m_port = server.Port;
                    s.m_steamHostID = 0UL;
                    s.m_steamHostAddr = new Steamworks.SteamNetworkingIPAddr();
                    s.m_steamHostAddr.SetIPv6(server.Ip.GetAddressBytes(), Convert.ToUInt16(server.Port));
                    s.m_upnp = true;
                    s.m_version = "?";

                    allServers.Insert(0, s);
                }
            }
        }
    }

    /**
     * Save a copy of the selected server IP to compare it to our favourites when prefilling password
     */
    [HarmonyPatch(typeof(ZNet))]
    [HarmonyPatch("SetServerHost")]
    [HarmonyPatch(new Type[] { typeof(SteamNetworkingIPAddr) })]
    public class PatchSetServerToSaveIp
    {
        static void Postfix(SteamNetworkingIPAddr serverAddr)
        {
            ValheimFaveServers.instance._Logger.LogInfo("Saved IP address in patch on set server host");
            ValheimFaveServers.instance.gameServerIp = serverAddr;
        }
    }

    /**
     * Prefill the server password
     */
    [HarmonyPatch(typeof(ZNet))]
    [HarmonyPatch("UpdatePassword")]
    public class PatchPasswordPrefill
    {
        static void Postfix(ref ZNet __instance)
        {
            if (__instance.m_passwordDialog.gameObject.activeSelf)
            {
                if (__instance.m_passwordDialog.GetComponentInChildren<UnityEngine.UI.InputField>().text.Trim() == "")
                {
                    Byte[] serverIpv6 = ValheimFaveServers.instance.gameServerIp.m_ipv6;
                    IPAddress serverIp = new IPAddress(serverIpv6);
                    foreach (ValheimFaveServerData sd in ValheimFaveServers.instance.configServers)
                    {
                        if (sd.Password.Trim() != "")
                        {
                            if (serverIp.Equals(sd.Ip))
                            {
                                ValheimFaveServers.instance._Logger.LogInfo("Prefilling server password in update password patch");
                                __instance.m_passwordDialog.GetComponentInChildren<UnityEngine.UI.InputField>().text = sd.Password;
                            }
                        }
                    }
                }
            }
        }
    }
}

