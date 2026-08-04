using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Loads default traffic paint materials when entering a scene without TrafficManager.</summary>
    public static class Level1TrafficVisualBootstrap
    {
        private static bool configured;

        public static void EnsureConfigured()
        {
            if (configured)
            {
                return;
            }

#if UNITY_EDITOR
            Material[] bodyColors =
            {
                LoadGameplayMaterial("Accent_Red.mat"),
                LoadGameplayMaterial("Wood_Warm.mat"),
                LoadGameplayMaterial("Foliage_Green.mat"),
                LoadGameplayMaterial("Produce_Orange.mat"),
                LoadGameplayMaterial("Market_Red.mat"),
                LoadGameplayMaterial("Coin_Gold.mat"),
                LoadGameplayMaterial("Produce_Yellow.mat"),
                LoadGameplayMaterial("House_Cream.mat")
            };

            Level1TrafficVisualUtility.Configure(
                bodyColors,
                LoadGameplayMaterial("Accent_Red.mat"),
                LoadGameplayMaterial("Dark_Brown.mat"),
                LoadGameplayMaterial("Glass_Blue.mat"),
                LoadMaterialAtPath(Level1KenneyCarKitPaths.ColormapMaterial));
#endif

            configured = true;
        }

#if UNITY_EDITOR
        private static Material LoadGameplayMaterial(string fileName)
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                ChoTetMarketEnvironment.MaterialsRoot + "/" + fileName);
        }

        private static Material LoadMaterialAtPath(string assetPath)
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        }
#endif
    }
}
