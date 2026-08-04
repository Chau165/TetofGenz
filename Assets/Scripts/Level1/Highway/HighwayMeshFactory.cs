using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Procedural low-poly 3D props for Vietnamese highway scenery.</summary>
    public static class HighwayMeshFactory
    {
        public struct Palette
        {
            public Material Wall;
            public Material WallAccent;
            public Material Roof;
            public Material Wood;
            public Material Foliage;
            public Material Sign;
            public Material Metal;
            public Material Dirt;
        }

        public enum PropKind
        {
            Shop,
            House,
            UtilityPole,
            StreetLight,
            SignBoard,
            Tree,
            BushCluster,
            SmallStore,
            CityBlock
        }

        public static void Build(Transform parent, PropKind kind, Palette palette, bool leftSide)
        {
            switch (kind)
            {
                case PropKind.Shop:
                    BuildShop(parent, palette, leftSide);
                    break;
                case PropKind.House:
                    BuildHouse(parent, palette, !leftSide);
                    break;
                case PropKind.UtilityPole:
                    BuildUtilityPole(parent, palette);
                    break;
                case PropKind.StreetLight:
                    BuildStreetLight(parent, palette);
                    break;
                case PropKind.SignBoard:
                    BuildSignBoard(parent, palette, leftSide);
                    break;
                case PropKind.Tree:
                    BuildTree(parent, palette);
                    break;
                case PropKind.BushCluster:
                    BuildBushCluster(parent, palette);
                    break;
                case PropKind.SmallStore:
                    BuildSmallStore(parent, palette, leftSide);
                    break;
                case PropKind.CityBlock:
                    BuildCityBlock(parent, palette);
                    break;
            }
        }

        public static void BuildShop(Transform parent, Palette p, bool leftSide)
        {
            float face = leftSide ? 90f : -90f;
            parent.localRotation = Quaternion.Euler(0f, face, 0f);
            Cube(parent, "ShopBase", p.Wall, new Vector3(0f, 1.4f, 0f), new Vector3(5.5f, 2.8f, 4.2f));
            Cube(parent, "ShopRoof", p.Roof, new Vector3(0f, 3.1f, 0f), new Vector3(6f, 0.45f, 4.8f));
            Cube(parent, "ShopAwning", p.WallAccent, new Vector3(0f, 2.2f, 2.3f), new Vector3(5.2f, 0.12f, 1.4f));
            Cube(parent, "ShopDoor", p.Wood, new Vector3(0f, 0.9f, 2.12f), new Vector3(1.4f, 1.8f, 0.12f));
            Cube(parent, "ShopSign", p.Sign, new Vector3(0f, 2.6f, 2.15f), new Vector3(2.8f, 0.55f, 0.1f));
        }

        public static void BuildSmallStore(Transform parent, Palette p, bool leftSide)
        {
            float face = leftSide ? 85f : -85f;
            parent.localRotation = Quaternion.Euler(0f, face, 0f);
            Cube(parent, "StoreBase", p.WallAccent, new Vector3(0f, 1.1f, 0f), new Vector3(4f, 2.2f, 3.5f));
            Cube(parent, "StoreRoof", p.Roof, new Vector3(0f, 2.5f, 0f), new Vector3(4.5f, 0.35f, 4f));
            Cube(parent, "StoreCounter", p.Wood, new Vector3(0f, 0.55f, 1.5f), new Vector3(3f, 1.1f, 0.8f));
        }

        public static void BuildHouse(Transform parent, Palette p, bool rural)
        {
            float w = rural ? 4.2f : 5f;
            Cube(parent, "HouseBase", p.Wall, new Vector3(0f, 1.2f, 0f), new Vector3(w, 2.4f, 3.8f));
            Cube(parent, "HouseRoof", p.Roof, new Vector3(0f, 2.85f, 0f), new Vector3(w + 0.6f, 0.5f, 4.4f));
            if (rural)
            {
                Cube(parent, "Porch", p.Wood, new Vector3(0f, 0.35f, 2.2f), new Vector3(2.5f, 0.25f, 1.2f));
            }
        }

        public static void BuildUtilityPole(Transform parent, Palette p)
        {
            Cube(parent, "Pole", p.Wood, new Vector3(0f, 3.5f, 0f), new Vector3(0.28f, 7f, 0.28f));
            Cube(parent, "CrossArm", p.Wood, new Vector3(0f, 6.2f, 0f), new Vector3(2.8f, 0.18f, 0.18f));
            Cube(parent, "Wire", p.Metal, new Vector3(1.2f, 5.8f, 0f), new Vector3(4.5f, 0.05f, 0.05f));
            Cube(parent, "Transformer", p.Metal, new Vector3(0.35f, 4.8f, 0f), new Vector3(0.5f, 0.7f, 0.4f));
        }

        public static void BuildStreetLight(Transform parent, Palette p)
        {
            Cube(parent, "LightPole", p.Metal, new Vector3(0f, 2.8f, 0f), new Vector3(0.2f, 5.6f, 0.2f));
            Cube(parent, "LightArm", p.Metal, new Vector3(0.8f, 5.2f, 0f), new Vector3(1.6f, 0.12f, 0.12f));
            Cube(parent, "Lamp", p.Sign, new Vector3(1.5f, 5.05f, 0f), new Vector3(0.45f, 0.25f, 0.35f));
        }

        public static void BuildSignBoard(Transform parent, Palette p, bool leftSide)
        {
            parent.localRotation = Quaternion.Euler(0f, leftSide ? 75f : -75f, 0f);
            Cube(parent, "SignPost", p.Metal, new Vector3(0f, 1.5f, 0f), new Vector3(0.18f, 3f, 0.18f));
            Cube(parent, "SignPanel", p.Sign, new Vector3(0f, 2.8f, 0.35f), new Vector3(2.6f, 1.2f, 0.12f));
            Cube(parent, "SignArrow", p.WallAccent, new Vector3(0f, 2.1f, 0.38f), new Vector3(0.8f, 0.15f, 0.1f));
        }

        public static void BuildTree(Transform parent, Palette p)
        {
            Cube(parent, "Trunk", p.Wood, new Vector3(0f, 1.1f, 0f), new Vector3(0.4f, 2.2f, 0.4f));
            Cube(parent, "Canopy", p.Foliage, new Vector3(0f, 3.2f, 0f), new Vector3(2.8f, 2.6f, 2.8f));
            Cube(parent, "CanopyTop", p.Foliage, new Vector3(0f, 4.4f, 0f), new Vector3(1.8f, 1.4f, 1.8f));
        }

        public static void BuildBushCluster(Transform parent, Palette p)
        {
            Cube(parent, "BushA", p.Foliage, new Vector3(-0.5f, 0.45f, 0.2f), new Vector3(1.4f, 0.9f, 1.2f));
            Cube(parent, "BushB", p.Foliage, new Vector3(0.6f, 0.35f, -0.3f), new Vector3(1.1f, 0.7f, 1f));
            Cube(parent, "BushC", p.Foliage, new Vector3(0.1f, 0.55f, 0.5f), new Vector3(1.2f, 1f, 1.1f));
        }

        public static void BuildCityBlock(Transform parent, Palette p)
        {
            float w = Random.Range(8f, 14f);
            float h = Random.Range(6f, 16f);
            Cube(parent, "Block", p.Wall, new Vector3(0f, h * 0.5f, 0f), new Vector3(w, h, w * 0.75f));
            Cube(parent, "BlockTrim", p.WallAccent, new Vector3(0f, h * 0.35f, w * 0.38f), new Vector3(w * 0.9f, h * 0.5f, 0.15f));
        }

        public static void BuildHill(Transform parent, Palette p, float width, float height)
        {
            Cube(parent, "Hill", p.Foliage, new Vector3(0f, height * 0.35f, 0f), new Vector3(width, height, width * 0.65f));
        }

        public static void BuildVergeStrip(Transform parent, Palette p, float length, bool leftSide)
        {
            float x = leftSide
                ? -(HighwayLaneSettings.RoadHalfWidth + HighwayLaneSettings.ShoulderWidth + HighwayLaneSettings.VergeWidth * 0.5f)
                : HighwayLaneSettings.RoadHalfWidth + HighwayLaneSettings.ShoulderWidth + HighwayLaneSettings.VergeWidth * 0.5f;
            Cube(parent, "Verge", p.Dirt, new Vector3(x, -0.02f, length * 0.5f),
                new Vector3(HighwayLaneSettings.VergeWidth, 0.08f, length));
        }

        private static void Cube(Transform parent, string name, Material mat, Vector3 pos, Vector3 scale)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            Collider col = go.GetComponent<Collider>();
            if (col != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Object.DestroyImmediate(col);
                }
                else
#endif
                {
                    Object.Destroy(col);
                }
            }

            if (mat != null)
            {
                go.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }
    }
}
