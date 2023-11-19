using System.Reflection;
// Using modweaver.api.Bootstrap
namespace modweaver.api.Utilities {
    public static class PluginUtilities
    {
        // REQUIRES CHAINLOADERHOOKS
        // public static Assembly[] GetPluginAssemblies()
        // {
        //     return ChainloaderHooks.Plugins.Select(plugin => plugin.Info.Instance.GetType().Assembly).ToArray();
        // }
    }
}