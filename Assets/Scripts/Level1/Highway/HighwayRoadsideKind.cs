namespace ChoTet.Level1.Highway
{
    /// <summary>Ordered bands from road edge outward: props → buildings → trees → backdrop.</summary>
    public enum HighwayRoadsideZone
    {
        Props = 0,
        Buildings = 1,
        Trees = 2,
        Backdrop = 3
    }

    /// <summary>Roadside prop categories for the scrolling highway environment.</summary>
    public enum HighwayRoadsideKind
    {
        House,
        Shop,
        TreeCluster,
        StreetLight,
        UtilityPole,
        Fence,
        Vegetation,
        RoadsideProp,
        UrbanBlock
    }

    public enum HighwayRoadsideSection
    {
        Urban,
        Rural,
        Mixed
    }
}
