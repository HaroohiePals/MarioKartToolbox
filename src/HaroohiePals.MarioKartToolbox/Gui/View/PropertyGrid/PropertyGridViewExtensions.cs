using HaroohiePals.Gui.View.PropertyGrid;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapObj;

namespace HaroohiePals.MarioKartToolbox.Gui.View.PropertyGrid
{
    public static class PropertyGridViewExtensions
    {
        public static void RegisterMapDataEditors(this PropertyGridView propertyGrid, MkdsMapData mapData, IMkdsMapObjDatabase mobjDatabase)
        {
            if (mapData == null)
                return;

            propertyGrid.RegisterEditor(new MapDataReferenceEditor<MkdsPath>(mapData, mapData.Paths));
            propertyGrid.RegisterEditor(new MapDataReferenceEditor<MkdsRespawnPoint>(mapData, mapData.RespawnPoints));
            propertyGrid.RegisterEditor(new MapDataReferenceEditor<MkdsEnemyPath>(mapData, mapData.EnemyPaths));
            propertyGrid.RegisterEditor(new MapDataReferenceEditor<MkdsItemPath>(mapData, mapData.ItemPaths));
            propertyGrid.RegisterEditor(new MapDataReferenceEditor<MkdsCamera>(mapData, mapData.Cameras));

            propertyGrid.RegisterEditor(new ConnectedPathPointReferenceEditor<MkdsEnemyPoint, MkdsEnemyPath>(mapData, mapData.EnemyPaths));
            propertyGrid.RegisterEditor(new ConnectedPathPointReferenceEditor<MkdsItemPoint, MkdsItemPath>(mapData, mapData.ItemPaths));

            propertyGrid.RegisterEditor(new MapDataReferenceCollectionEditor<MkdsEnemyPath>(mapData, mapData.EnemyPaths));
            propertyGrid.RegisterEditor(new MapDataReferenceCollectionEditor<MkdsItemPath>(mapData, mapData.ItemPaths));
            propertyGrid.RegisterEditor(new MapDataReferenceCollectionEditor<MkdsCheckPointPath>(mapData, mapData.CheckPointPaths));

            propertyGrid.RegisterEditor(new MgEnemyPointReferenceEditor(mapData, mapData.MgEnemyPaths));
            propertyGrid.RegisterEditor(new MgEnemyPointReferenceCollectionEditor(mapData, mapData.MgEnemyPaths));

            // Has priority over the enum editor and thus should be checked first
            propertyGrid.RegisterEditor(new MapObjectIdEditor(mobjDatabase), 0);
        }

        public static void RegisterCollisionEditors(this PropertyGridView propertyGrid, MkdsMapData mapData)
        {
            // Has priority over the enum editor and thus should be checked first
            propertyGrid.RegisterEditor<MkdsCollisionVariantEditor>(0);
            propertyGrid.RegisterEditor(new MkdsCollisionLightIdEditor(mapData?.StageInfo), 0);
        }
    }
}
