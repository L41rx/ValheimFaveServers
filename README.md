All this mod does it let you specify servers to show up at the top of the server browser.

This mod requires Bepinex installed in your copy of Valheim for it to work. See instructions here https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/

In the instructions below, {valheim install} refers to a location on your hard drive, likely "C:\Program Files (x86)\Steam\steamapps\common\Valheim" if you are on windows and installed to default location.

# INSTALL
ValheimFaveServers is just a .dll file that you drop into your {valheim install}/Bepinex/plugins directory (like other bepinex mods).

# CONFIGURATION:
Launch Valheim and exit. You will get a new config file at:

{valheim install}/Bepinex/config

Called "net.l41rx.valheimfaveservers.cfg"

Edit this file to include the IP addresses or hostnames of your favourite servers. E.g.

Server IP Addresses = 195.162.137.30,173.162.137.28
Then, relaunch the game and go to the server browser. You will see the IP Addresses or hostnames you added at the (assuming) top of the list, prefixed with "!!!VFS:" (so it sorts to the top, for easier access)

Thats all it does! Takes the hosts/IPs in the config file and puts them at the top of the server browser.


# LIMITATIONS
I am not a c# dev or have used unity before etc (PHP only), so this was really only for me to get to a friends server without having to go back to our messages to get the IP. Because of this, the mod does not support:

* Ports other than 2456
* Actual server names (just shows host/ip)
* Player counts (hardcoded to 0)
* server game version display in list (shows x.yy or something)

hope someone else finds it useful maybe!
