#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>Finds outdoor scene objects even when Level1_OutdoorArea is inactive in edit mode.</summary>
    internal static class Level1OutdoorSceneLookup
    {
        public static Transform ResolveOutdoorRoot()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform found = FindDeep(roots[i].transform, "Level1_OutdoorArea");
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public static Transform ResolveBusPickupArea()
        {
            return ResolveBusPickupArea(ResolveOutdoorRoot());
        }

        public static Transform ResolveBusPickupArea(Transform outdoorRoot)
        {
            return outdoorRoot != null ? FindDeep(outdoorRoot, "Level1_BusPickupArea") : null;
        }

        public static Transform FindDeep(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == objectName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
#endif
