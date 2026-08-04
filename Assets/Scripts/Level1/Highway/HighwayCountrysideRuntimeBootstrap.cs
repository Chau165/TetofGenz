using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChoTet.Level1.Highway
{
    /// <summary>Repairs countryside visuals at runtime when the editor rebuild was not applied.</summary>
    [DefaultExecutionOrder(-120)]
    public class HighwayCountrysideRuntimeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrapHighwayScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.name.IndexOf("Highway", System.StringComparison.OrdinalIgnoreCase) < 0)
            {
                return;
            }

            if (FindAnyObjectByType<HighwayCountrysideRuntimeBootstrap>() != null)
            {
                return;
            }

            GameObject host = new GameObject("HighwayCountrysideBootstrap");
            host.AddComponent<HighwayCountrysideRuntimeBootstrap>();
        }

        private void Awake()
        {
            Time.timeScale = 1f;
            ClearStuckFadeOverlay();
            DisableUrbanSystems();
            EnableCountrysideSystems();
        }

        private static void ClearStuckFadeOverlay()
        {
            GameObject fade = GameObject.Find("HighwayFadeOverlay");
            if (fade != null)
            {
                Destroy(fade);
            }
        }

        private static void DisableUrbanSystems()
        {
            SetActiveIfFound<HighwayStaticUrbanNeighborhood>(false);
            SetActiveIfFound<HighwayQuaterniusTownBlockPool>(false);
            SetActiveIfFound<HighwayQuaterniusSidewalkPool>(false);
            SetActiveIfFound<HighwayEnvironmentSpawner>(false);
            SetActiveIfFound<HighwayNatureRoadsideSpawner>(false);
        }

        private static void EnableCountrysideSystems()
        {
            HighwaySideGroundPool sideGround = FindAnyObjectByType<HighwaySideGroundPool>(FindObjectsInactive.Include);
            if (sideGround != null)
            {
                sideGround.gameObject.SetActive(true);
            }

            HighwayHorizonLayer horizon = FindAnyObjectByType<HighwayHorizonLayer>(FindObjectsInactive.Include);
            if (horizon != null)
            {
                horizon.gameObject.SetActive(true);
            }

            EnsureCountrysideScenery(sideGround);
            EnsurePropsSpawner();
        }

        private static void EnsureCountrysideScenery(HighwaySideGroundPool sideGround)
        {
            HighwayCountrysideSceneryPool scenery =
                FindAnyObjectByType<HighwayCountrysideSceneryPool>(FindObjectsInactive.Include);
            if (scenery == null)
            {
                GameObject world = GameObject.Find("HighwayWorld");
                GameObject go = new GameObject("HighwayCountrysideScenery");
                if (world != null)
                {
                    go.transform.SetParent(world.transform, false);
                }

                scenery = go.AddComponent<HighwayCountrysideSceneryPool>();
            }

            scenery.gameObject.SetActive(true);
            scenery.Initialize(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                HighwayCountrysideSceneryPool.CountrysideBiome.ForestRiverAlternating);
        }

        private static void EnsurePropsSpawner()
        {
            HighwayCountrysidePropsSpawner props =
                FindAnyObjectByType<HighwayCountrysidePropsSpawner>(FindObjectsInactive.Include);
            if (props != null)
            {
                props.gameObject.SetActive(true);
                return;
            }

            GameObject world = GameObject.Find("HighwayWorld");
            GameObject go = new GameObject("HighwayCountrysideProps");
            if (world != null)
            {
                go.transform.SetParent(world.transform, false);
            }

            props = go.AddComponent<HighwayCountrysidePropsSpawner>();
            props.Initialize(null, null);
            props.gameObject.SetActive(true);
        }

        private static void SetActiveIfFound<T>(bool active) where T : Component
        {
            T component = FindAnyObjectByType<T>(FindObjectsInactive.Include);
            if (component != null)
            {
                component.gameObject.SetActive(active);
            }
        }
    }
}
