using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Runtime references to baked NSK2 vegetation prefabs.</summary>
    [CreateAssetMenu(fileName = "HighwayNaturePrefabLibrary", menuName = "Cho Tet/Highway Nature Prefab Library")]
    public class HighwayNaturePrefabLibrary : ScriptableObject
    {
        public GameObject[] grassTufts;
        public GameObject[] bushes;
        public GameObject[] trees;

        public bool HasContent =>
            (grassTufts != null && grassTufts.Length > 0)
            || (bushes != null && bushes.Length > 0)
            || (trees != null && trees.Length > 0);

        public GameObject PickGrass(int seed)
        {
            return PickFrom(grassTufts, seed);
        }

        public GameObject PickBush(int seed)
        {
            return PickFrom(bushes, seed);
        }

        public GameObject PickTree(int seed)
        {
            return PickFrom(trees, seed);
        }

        private static GameObject PickFrom(GameObject[] options, int seed)
        {
            if (options == null || options.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < options.Length; i++)
            {
                GameObject candidate = options[(Mathf.Abs(seed) + i) % options.Length];
                if (candidate != null)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
