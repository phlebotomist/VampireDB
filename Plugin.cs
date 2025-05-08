using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace VampireDB;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public static ManualLogSource Logger;
    public override void Load()
    {
        Logger = Log;
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} version {PluginInfo.PLUGIN_VERSION} loaded!");
    }

    public override bool Unload()
    {
        Storage.Instance.Dispose();
        return true;
    }
}
