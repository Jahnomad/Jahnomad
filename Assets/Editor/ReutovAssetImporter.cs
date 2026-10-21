using UnityEditor;
using UnityEngine;
namespace JahNomad.Reutov.Editor {
    public sealed class ReutovAssetImporter : AssetPostprocessor {
        private void OnPreprocessTexture() {
            if (!assetPath.StartsWith("Assets/Resources/") || !assetPath.EndsWith(".png")) return;
            var settings = (TextureImporter)assetImporter;
            settings.textureType = TextureImporterType.Default;
            settings.isReadable = true;
            settings.mipmapEnabled = false;
            settings.alphaIsTransparency = true;
            settings.filterMode = FilterMode.Point;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            settings.npotScale = TextureImporterNPOTScale.None;
        }
    }
}