using BepInEx;
using Fisobs.Core;
using BepInEx.Logging;
using UnityEngine;

namespace BouncingBallCreature;

[BepInPlugin("lb-fgf-m4r-ik.bouncing-ball-creature", nameof(BouncingBallCreature), "0.1.0")]
sealed class BouncingBallPlugin : BaseUnityPlugin
{
    internal static ManualLogSource? logger;

    public void OnEnable()
    {
        logger = Logger;
        // To change the loaded texture's properties...
        HK.On.Fisobs.Core.Ext.LoadAtlasFromEmbRes += (orig, assembly, resource) =>
        {
            if (assembly.FullName.Contains("BouncingBall"))
            {
                using var stream = assembly.GetManifestResourceStream(resource);
                if (stream is null) return null;
                byte[] array = new byte[stream.Length];
                stream.Read(array, 0, array.Length);
                Texture2D texture2D = new(0, 0, TextureFormat.ARGB32, false) { anisoLevel = 1, filterMode = 0 };
                texture2D.LoadImage(array);
                return Futile.atlasManager.LoadAtlasFromTexture(resource, texture2D);
            }
            else return orig(assembly, resource);
        };
        Content.Register(new BouncingBallCritob());
    }

    public void OnDisable()
    {
        HK.Infos.Dispose();
        logger = default;
    }
}