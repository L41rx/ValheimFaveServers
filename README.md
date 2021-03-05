This mod makes getting to your friends servers faster. It lets you specify custom servers to show up at the top of the server browser, and optionally can prefill passwords too.

This mod requires Bepinex installed in your copy of Valheim for it to work. See instructions here: https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/

In the instructions below, {valheim install} refers to a location on your hard drive, likely "C:\Program Files (x86)\Steam\steamapps\common\Valheim" if you are on windows and installed to default location.

# INSTALL

ValheimFaveServers is just a .dll file that you drop into your {valheim install}/Bepinex/plugins directory (like other bepinex mods).

# CONFIGURATION

Launch Valheim and exit. You will get a new config file at:

{valheim install}/Bepinex/config

Called "net.l41rx.valheimfaveservers.cfg".

It will contain a couple general options and then server info.

It comes with an example configuration for a fake server "Comradely Conquest". 

## You can add more servers by:

1. Appending a new server key (lowercase, no spaces) to the config entry "`[General.ServerKeys]`"

2. Copying the Comradely Conquest example config blocks (host, name, password, port) *(or relaunching/quitting, see notes)* and filling in the appropriate information

## You can remove a server by:

1. Removing its key

2. Removing its server blocks

## Note

If you launch the game with a new server key, but haven't created the blocks for the server properties, the new server property blocks will be automatically generated for you with the correct key, and default values. This can be faster/safer than copying the example.

# LIMITATIONS

I'm not sure how to query Valheim servers for their version or player counts, so:

* Player counts are hardcoded to 0
* Server version shows as '?'

I find the configuration is a bit clunky using the default bepinex config classes. Thats why:

* it takes a couple restarts to get a complete config.

Also, **if you specify the wrong prefill password**, you cant change it in game, you have to quit and change the config. I could fix this so you can backspace and put the right one it, but it would take me time and I dont think thats very useful anyway.

# SUPPORT

If the mod isnt working like you think it should or theres something I can add you can message me on Twitter: [L41rx](https://twitter.com/L41rx)

I hope someone else finds it useful! The source can be found at Github: [L41rx/ValheimFaveServers](https://github.com/L41rx/ValheimFaveServers)
