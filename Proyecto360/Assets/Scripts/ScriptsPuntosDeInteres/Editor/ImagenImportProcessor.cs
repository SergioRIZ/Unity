using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class ImagenImportProcessor : AssetPostprocessor
{
    // Lista de extensiones válidas
    private static readonly string[] extensionesPermitidas = 
        { ".png", ".jpg", ".jpeg", ".bmp", ".tiff", ".tif" };

    static void OnPostprocessAllAssets(
        string[] importedAssets, string[] deletedAssets,
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string assetPath in importedAssets)
        {
            // Verifica que sea una imagen en la carpeta de origen
            if (assetPath.StartsWith("Assets/PuntosDeInteres/ImagenesPuntosDeInteres"))
            {
                string extension = Path.GetExtension(assetPath).ToLower();

                if (extensionesPermitidas.Contains(extension))
                {
                    TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                    if (importer != null)
                    {
                        // Configura como Sprite
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.SaveAndReimport();

                        // Imprime un log informativo
                        Debug.Log($"Imagen '{Path.GetFileName(assetPath)}' convertida a Sprite en la misma carpeta.");
                    }
                }
            }
        }
    }
}
