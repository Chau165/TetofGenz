#if UNITY_EDITOR
using ChoTet.Level1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>Continuous walk floors + selective facade walls so the player can step onto sidewalks and enter open shops.</summary>
    public static class Level1OutdoorWalkCollisionBuilder
    {
        public const string CollidersRootName = "Level1_OutdoorWalkColliders";
        private const float WallHeight = 2.65f;

        private static readonly string[] StorefrontRoots =
        {
            Level1ConvenienceStoreFacadeBuilder.RootName,
            Level1VietnamesePawnShopBuilder.RootName,
            Level1TheGioiMobileStoreBuilder.RootName,
            Level1VietnameseCarWashBuilder.RootName,
            Level1VietnameseComTamShopBuilder.RootName,
            Level1VietnamesePharmacyBuilder.RootName,
        };

        public static void Rebuild(Transform outdoorRoot)
        {
            Transform existing = outdoorRoot.Find(CollidersRootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            Transform legacyFloor = outdoorRoot.Find("OutdoorWalkCollider");
            if (legacyFloor != null)
            {
                Object.DestroyImmediate(legacyFloor.gameObject);
            }

            GameObject root = new GameObject(CollidersRootName);
            root.transform.SetParent(outdoorRoot, false);

            BuildWalkFloors(root.transform);
            BuildCurbTransitions(root.transform);
            BuildBusStopApproach(root.transform);
            BuildBoundaryVoidScreenWalls(outdoorRoot, root.transform);
            BuildAmenityFacadeWalls(outdoorRoot, root.transform);
            BuildOpenShopAprons(outdoorRoot, root.transform);
            BuildShopInteriorObstacles(outdoorRoot, root.transform);
            BuildBusStopWalkFloor(root.transform);
        }

        private static readonly (string ShopRoot, string[] ObstacleNames)[] ShopInteriorObstacles =
        {
            (Level1VietnamesePawnShopBuilder.RootName, new[]
            {
                "Pawn_Wall_Left", "Pawn_Wall_Right", "Pawn_Wall_Back",
                "Interior_Counter", "Interior_Shelf_0", "Interior_Shelf_1", "Interior_Shelf_2", "Interior_Shelf_3",
                "Pawn_Scooter", "Prop_ABoard",
            }),
            (Level1VietnameseComTamShopBuilder.RootName, new[]
            {
                "ComTam_Wall_Left", "ComTam_Wall_Right", "ComTam_Wall_Back",
                "Prep_Counter", "Rice_Cooker", "Food_DisplayCabinet",
                "Table_0", "Table_1", "Stool_0_A", "Stool_0_B",
            }),
            (Level1VietnamesePharmacyBuilder.RootName, new[]
            {
                "Pharmacy_Wall_Left", "Pharmacy_Wall_Right", "Pharmacy_Wall_Back",
                "Cashier_Counter", "Shelf_Unit_0", "Shelf_Unit_1", "Shelf_Unit_2",
            }),
            (Level1TheGioiMobileStoreBuilder.RootName, new[]
            {
                "Mobile_Wall_Left", "Mobile_Wall_Right", "Mobile_Wall_Back",
                "Cashier_Counter", "DisplayCounter_0", "DisplayCounter_1",
            }),
            (Level1VietnameseCarWashBuilder.RootName, new[]
            {
                "Wash_Wall_Left", "Wash_Wall_Right", "Wash_Wall_Back",
                "Vehicle_Placeholder", "Prop_PressureWasher_Body", "Prop_Chair_Seat",
            }),
        };

        private static void BuildShopInteriorObstacles(Transform outdoorRoot, Transform parent)
        {
            for (int i = 0; i < ShopInteriorObstacles.Length; i++)
            {
                (string shopRoot, string[] obstacleNames) = ShopInteriorObstacles[i];
                Transform shop = FindDeep(outdoorRoot, shopRoot);
                if (shop == null || IsWalkEntranceBlocked(shop))
                {
                    continue;
                }

                for (int o = 0; o < obstacleNames.Length; o++)
                {
                    Transform obstacle = FindDeep(shop, obstacleNames[o]);
                    if (obstacle == null)
                    {
                        continue;
                    }

                    if (!TryComputeObstacleBox(obstacle, out Vector3 center, out Vector3 size))
                    {
                        continue;
                    }

                    CreateInvisibleWall(parent, $"ShopObstacle_{shop.name}_{obstacleNames[o]}", center, size);
                }
            }
        }

        private static bool TryComputeObstacleBox(Transform obstacle, out Vector3 center, out Vector3 size)
        {
            center = Vector3.zero;
            size = Vector3.zero;

            Renderer renderer = obstacle.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                center = bounds.center;
                size = bounds.size;
                size.x = Mathf.Max(0.12f, size.x);
                size.y = Mathf.Max(0.35f, size.y);
                size.z = Mathf.Max(0.12f, size.z);
                return true;
            }

            Collider collider = obstacle.GetComponent<Collider>();
            if (collider != null)
            {
                Bounds bounds = collider.bounds;
                center = bounds.center;
                size = bounds.size;
                return true;
            }

            center = obstacle.position;
            size = Vector3.Scale(obstacle.lossyScale, Vector3.one);
            if (size.sqrMagnitude < 0.01f)
            {
                size = new Vector3(0.5f, 1.2f, 0.5f);
            }

            return true;
        }

        private static void BuildWalkFloors(Transform parent)
        {
            float sidewalkFloorY = Level1OutdoorSettings.WalkSurfaceY - Level1OutdoorMovementSettings.WalkFloorThickness * 0.5f;
            float roadFloorY = Level1OutdoorMovementSettings.RoadSurfaceY - Level1OutdoorMovementSettings.WalkFloorThickness * 0.5f;
            float centerZ = Level1OutdoorMovementSettings.FloorSpanCenterZ;
            float depthZ = Level1OutdoorMovementSettings.FloorSpanDepth;
            float overlap = Level1OutdoorMovementSettings.CurbTransitionOverlap;

            float westEndX = Level1OutdoorMovementSettings.WestWalkInnerX + overlap;
            float westWidth = westEndX - Level1OutdoorMovementSettings.PlayMinX;
            float westCenterX = (Level1OutdoorMovementSettings.PlayMinX + westEndX) * 0.5f;
            CreateSolidBox(parent, "WalkFloor_West",
                new Vector3(westCenterX, sidewalkFloorY, centerZ),
                new Vector3(westWidth, Level1OutdoorMovementSettings.WalkFloorThickness, depthZ));

            float eastStartX = Level1OutdoorMovementSettings.EastWalkInnerX - overlap;
            float eastWidth = Level1OutdoorMovementSettings.PlayMaxX - eastStartX;
            float eastCenterX = (eastStartX + Level1OutdoorMovementSettings.PlayMaxX) * 0.5f;
            CreateSolidBox(parent, "WalkFloor_East",
                new Vector3(eastCenterX, sidewalkFloorY, centerZ),
                new Vector3(eastWidth, Level1OutdoorMovementSettings.WalkFloorThickness, depthZ));

            float roadWidth = Level1OutdoorMovementSettings.AlleyRoadHalfWidth * 2f;
            CreateSolidBox(parent, "WalkFloor_AlleyCenter",
                new Vector3(0f, roadFloorY, centerZ),
                new Vector3(roadWidth, Level1OutdoorMovementSettings.WalkFloorThickness, depthZ));
        }

        private static void BuildCurbTransitions(Transform parent)
        {
            float sidewalkFloorY = Level1OutdoorSettings.WalkSurfaceY - Level1OutdoorMovementSettings.WalkFloorThickness * 0.5f;
            float roadFloorY = Level1OutdoorMovementSettings.RoadSurfaceY - Level1OutdoorMovementSettings.WalkFloorThickness * 0.5f;
            float centerZ = Level1OutdoorMovementSettings.FloorSpanCenterZ;
            float depthZ = Level1OutdoorMovementSettings.FloorSpanDepth;
            float overlap = Level1OutdoorMovementSettings.CurbTransitionOverlap;
            float stripWidth = Level1OutdoorMovementSettings.AlleyCurbWidth + overlap * 2f;
            float stripHeight = Level1OutdoorMovementSettings.AlleyCurbHeight + Level1OutdoorMovementSettings.WalkFloorThickness;
            float stripCenterY = (sidewalkFloorY + roadFloorY) * 0.5f;

            float westStripCenterX = (Level1OutdoorMovementSettings.WestWalkInnerX + (-Level1OutdoorMovementSettings.AlleyRoadHalfWidth)) * 0.5f;
            CreateSolidBox(parent, "CurbTransition_West",
                new Vector3(westStripCenterX, stripCenterY, centerZ),
                new Vector3(stripWidth, stripHeight, depthZ));

            float eastStripCenterX = (Level1OutdoorMovementSettings.EastWalkInnerX + Level1OutdoorMovementSettings.AlleyRoadHalfWidth) * 0.5f;
            CreateSolidBox(parent, "CurbTransition_East",
                new Vector3(eastStripCenterX, stripCenterY, centerZ),
                new Vector3(stripWidth, stripHeight, depthZ));
        }

        private static void BuildBusStopApproach(Transform parent)
        {
            float floorY = Level1OutdoorSettings.WalkSurfaceY - Level1OutdoorMovementSettings.WalkFloorThickness * 0.5f;
            float northZ = Level1OutdoorMovementSettings.BusStopWalkMaxZ;
            float southZ = Level1OutdoorMovementSettings.BusStopWalkMinZ;
            float depth = Mathf.Abs(northZ - southZ);
            float centerZ = (northZ + southZ) * 0.5f;
            float maxX = Level1OutdoorMovementSettings.BusStopWalkMaxX;
            float minX = Level1OutdoorMovementSettings.BusStopWalkMinX;
            float width = maxX - minX;
            float centerX = (minX + maxX) * 0.5f;

            CreateSolidBox(parent, "WalkFloor_BusStopApproach",
                new Vector3(centerX, floorY, centerZ),
                new Vector3(width, Level1OutdoorMovementSettings.WalkFloorThickness, depth));

            float corridorMinX = Level1OutdoorMovementSettings.EastWalkInnerX - Level1OutdoorMovementSettings.CurbTransitionOverlap;
            float corridorMaxX = Level1OutdoorMovementSettings.BusStopWalkMinX - 0.05f;
            float corridorWidth = corridorMaxX - corridorMinX;
            if (corridorWidth > 0.35f)
            {
                float corridorMinZ = Level1OutdoorMovementSettings.BusStopWalkMinZ - 0.2f;
                float corridorMaxZ = Level1OutdoorMovementSettings.BusStopWalkMaxZ + 0.2f;
                CreateSolidBox(parent, "WalkFloor_BusStopEastCorridor",
                    new Vector3((corridorMinX + corridorMaxX) * 0.5f, floorY, (corridorMinZ + corridorMaxZ) * 0.5f),
                    new Vector3(corridorWidth, Level1OutdoorMovementSettings.WalkFloorThickness,
                        corridorMaxZ - corridorMinZ + 0.35f));
            }
        }

        private static void BuildBusStopWalkFloor(Transform parent)
        {
            float floorY = Level1OutdoorSettings.WalkSurfaceY - Level1OutdoorMovementSettings.WalkFloorThickness * 0.5f;
            float minX = Level1OutdoorMovementSettings.BusStopWalkMinX;
            float maxX = Level1OutdoorMovementSettings.BusStopWalkMaxX;
            float minZ = Level1OutdoorMovementSettings.BusStopWalkMinZ;
            float maxZ = Level1OutdoorMovementSettings.BusStopWalkMaxZ;
            float padWidth = maxX - minX;
            float padCenterX = (minX + maxX) * 0.5f;
            float padDepth = maxZ - minZ + 0.6f;
            float padCenterZ = (minZ + maxZ) * 0.5f;

            CreateSolidBox(parent, "WalkFloor_BusStopPad",
                new Vector3(padCenterX, floorY, padCenterZ),
                new Vector3(padWidth, Level1OutdoorMovementSettings.WalkFloorThickness, padDepth));

            // Stay on sidewalk + allow highway lanes (lives system) — floor covers asphalt too via RoadWalkFloor.
            float shoulderMinZ = Level1BusStopRoadSettings.SidewalkNearZ + 0.05f;
            float shoulderMaxZ = Level1BusStopRoadSettings.SidewalkNearZ + 0.85f;
            float shoulderMinX = Level1BusStopRoadSettings.BusShelterCenterX - 0.35f;
            float shoulderWidth = maxX - shoulderMinX;
            float shoulderDepth = shoulderMaxZ - shoulderMinZ;
            if (shoulderWidth > 0.35f && shoulderDepth > 0.2f)
            {
                CreateSolidBox(parent, "WalkFloor_BusStopPickupShoulder",
                    new Vector3(shoulderMinX + shoulderWidth * 0.5f, floorY, (shoulderMinZ + shoulderMaxZ) * 0.5f),
                    new Vector3(shoulderWidth, Level1OutdoorMovementSettings.WalkFloorThickness, shoulderDepth));
            }

            float connectorMinZ = Level1OutdoorMovementSettings.PlayEndZ - 0.35f;
            float connectorMaxZ = Level1OutdoorMovementSettings.BusStopWalkMaxZ;
            float connectorDepth = connectorMaxZ - connectorMinZ;
            float connectorCenterZ = (connectorMinZ + connectorMaxZ) * 0.5f;
            float connectorWidth = Level1OutdoorMovementSettings.WestWalkInnerX - Level1OutdoorMovementSettings.PlayMinX + 0.45f;
            float connectorCenterX = (Level1OutdoorMovementSettings.PlayMinX + Level1OutdoorMovementSettings.WestWalkInnerX) * 0.5f;

            CreateSolidBox(parent, "WalkFloor_BusStopConnector",
                new Vector3(connectorCenterX, floorY, connectorCenterZ),
                new Vector3(connectorWidth, Level1OutdoorMovementSettings.WalkFloorThickness, connectorDepth + 0.4f));
        }

        // Visual boundary screens intentionally have no mesh colliders. Mirror their bounds with
        // invisible walls so the player cannot leave the playable area through the bus-stop edge.
        private static void BuildBoundaryVoidScreenWalls(Transform outdoorRoot, Transform parent)
        {
            Transform boundaryRoot = FindDeep(outdoorRoot, "Boundary_VoidScreen");
            if (boundaryRoot == null)
            {
                return;
            }

            Renderer[] screens = boundaryRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < screens.Length; i++)
            {
                Renderer screen = screens[i];
                if (!screen.name.StartsWith("VoidScreen_"))
                {
                    continue;
                }

                Bounds bounds = screen.bounds;
                if (bounds.size.x <= 0.01f || bounds.size.y <= 0.01f || bounds.size.z <= 0.01f)
                {
                    continue;
                }

                CreateInvisibleWall(parent, $"BoundaryScreen_{i}", bounds.center,
                    bounds.size + new Vector3(0.06f, 0.06f, 0.06f));
            }
        }

        private static void BuildAmenityFacadeWalls(Transform outdoorRoot, Transform parent)
        {
            for (int i = 0; i < StorefrontRoots.Length; i++)
            {
                Transform shop = FindDeep(outdoorRoot, StorefrontRoots[i]);
                if (shop == null || !IsWalkEntranceBlocked(shop))
                {
                    continue;
                }

                if (!TryComputeFacadeWall(shop, out Vector3 center, out Vector3 size))
                {
                    continue;
                }

                CreateInvisibleWall(parent, $"AmenityFacade_{shop.name}", center, size);
            }
        }

        private static void BuildOpenShopAprons(Transform outdoorRoot, Transform parent)
        {
            float floorY = Level1OutdoorSettings.WalkSurfaceY - Level1OutdoorMovementSettings.WalkFloorThickness * 0.5f;

            for (int i = 0; i < StorefrontRoots.Length; i++)
            {
                Transform shop = FindDeep(outdoorRoot, StorefrontRoots[i]);
                if (shop == null || IsWalkEntranceBlocked(shop))
                {
                    continue;
                }

                if (!TryComputeOpenApron(shop, floorY, out Vector3 center, out Vector3 size))
                {
                    continue;
                }

                CreateSolidBox(parent, $"OpenApron_{shop.name}", center, size);
            }
        }

        private static bool IsWalkEntranceBlocked(Transform shop)
        {
            if (shop.Find("ClosedOverlay") != null)
            {
                return true;
            }

            SlidingGlassDoor[] doors = shop.GetComponentsInChildren<SlidingGlassDoor>(true);
            if (doors.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i] != null && doors[i].enabled)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryComputeOpenApron(Transform shop, float floorY, out Vector3 center, out Vector3 size)
        {
            center = Vector3.zero;
            size = Vector3.zero;

            Bounds bounds = ComputeWorldBounds(shop.gameObject);
            if (bounds.size.sqrMagnitude < 0.05f)
            {
                return false;
            }

            bool onWestSide = shop.position.x < -0.2f;
            bool onEastSide = shop.position.x > 0.2f;
            float apronDepth = Mathf.Clamp(Mathf.Max(bounds.size.x, bounds.size.z) * 0.42f, 2.2f, 4.6f);
            float apronWidth = Mathf.Max(2.4f, onWestSide || onEastSide ? bounds.size.z + 0.45f : bounds.size.x + 0.45f);

            if (onWestSide)
            {
                center = new Vector3(bounds.max.x - apronDepth * 0.5f, floorY, bounds.center.z);
                size = new Vector3(apronDepth, Level1OutdoorMovementSettings.WalkFloorThickness, apronWidth);
            }
            else if (onEastSide)
            {
                center = new Vector3(bounds.min.x + apronDepth * 0.5f, floorY, bounds.center.z);
                size = new Vector3(apronDepth, Level1OutdoorMovementSettings.WalkFloorThickness, apronWidth);
            }
            else
            {
                center = new Vector3(bounds.center.x, floorY, bounds.max.z - apronDepth * 0.5f);
                size = new Vector3(apronWidth, Level1OutdoorMovementSettings.WalkFloorThickness, apronDepth);
            }

            return true;
        }

        private static bool TryComputeFacadeWall(Transform shop, out Vector3 center, out Vector3 size)
        {
            center = Vector3.zero;
            size = Vector3.zero;

            Bounds bounds = ComputeWorldBounds(shop.gameObject);
            if (bounds.size.sqrMagnitude < 0.05f)
            {
                return false;
            }

            bool onWestSide = shop.position.x < -0.2f;
            bool onEastSide = shop.position.x > 0.2f;
            float facadeX;
            if (onWestSide)
            {
                facadeX = bounds.max.x + 0.1f;
            }
            else if (onEastSide)
            {
                facadeX = bounds.min.x - 0.1f;
            }
            else
            {
                facadeX = shop.position.z < -10f ? bounds.max.z + 0.1f : bounds.min.z - 0.1f;
            }

            const float depth = 0.22f;
            float widthZ = Mathf.Max(1.8f, bounds.size.z + 0.35f);
            if (!onWestSide && !onEastSide)
            {
                widthZ = Mathf.Max(1.8f, bounds.size.x + 0.35f);
            }

            center = new Vector3(facadeX, Level1OutdoorSettings.WalkSurfaceY + WallHeight * 0.5f, bounds.center.z);
            if (!onWestSide && !onEastSide)
            {
                center = new Vector3(bounds.center.x, center.y, facadeX);
                size = new Vector3(widthZ, WallHeight, depth);
            }
            else
            {
                size = new Vector3(depth, WallHeight, widthZ);
            }

            return true;
        }

        private static Bounds ComputeWorldBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = default;
            bool hasBounds = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
            {
                bounds = new Bounds(root.transform.position, Vector3.one);
            }

            return bounds;
        }

        private static Transform FindDeep(Transform root, string objectName)
        {
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

        private static void CreateSolidBox(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.position = position;
            box.transform.localScale = scale;
            Object.DestroyImmediate(box.GetComponent<Renderer>());
        }

        private static void CreateInvisibleWall(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, false);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            Object.DestroyImmediate(wall.GetComponent<Renderer>());
        }
    }
}
#endif
