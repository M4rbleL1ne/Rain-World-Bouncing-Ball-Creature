using MonoMod.RuntimeDetour.HookGen;
using static System.Reflection.BindingFlags;
using System.Reflection;
using System.ComponentModel;

namespace HK
{
    public static class Infos
    {
        public static MethodInfo? LAFER = typeof(Fisobs.Core.Ext).GetMethod("LoadAtlasFromEmbRes", Public | NonPublic | Instance | Static);

        internal static void Dispose() => LAFER = default;
    }

    namespace On.Fisobs.Core
    {
        public static class Ext
        {
            [EditorBrowsable(EditorBrowsableState.Never)]
            public delegate FAtlas? orig_LoadAtlasFromEmbRes(Assembly assembly, string resource);

            [EditorBrowsable(EditorBrowsableState.Never)]
            public delegate FAtlas? hook_LoadAtlasFromEmbRes(orig_LoadAtlasFromEmbRes orig, Assembly assembly, string resource);

            public static event hook_LoadAtlasFromEmbRes LoadAtlasFromEmbRes
            {
                add => HookEndpointManager.Add<hook_LoadAtlasFromEmbRes>(Infos.LAFER, value);
                remove => HookEndpointManager.Remove<hook_LoadAtlasFromEmbRes>(Infos.LAFER, value);
            }
        }
    }
}