using UnityEngine;

namespace MetaEditor
{
    public static class EditorDatabase
    {
        // COLORS: --------------------------------------------------------------------------------

        public static readonly Color SelectedColor = new(0.745f, 0.256f, 0.302f);
        public static readonly Color SelectedInactiveColor = new(0.205f, 0.205f, 0.205f);

        public const string NPCsMetaPath = "Assets/Core/RPG/Resources/Meta/AI/NPCs";
        public const string PresetsMetaPath = "Assets/Core/RPG/Resources/Meta/AI/Presets";
        public const string BehaviorsMetaPath = "Assets/Core/RPG/Resources/Meta/AI/Behaviors";
        
        public const string EffectsMetaPath = "Assets/Core/RPG/Resources/Meta/Combat/Effects";
        public const string StatsMetaPath = "Assets/Core/RPG/Resources/Meta/Combat/Stats";


        public const string GuildHeroesPath = "Assets/Core/Mechanics/Main Menu/Guild/Meta";
        public const string HeroSkinsPath = "Assets/Core/Game Data/Resources/Hero Skins";
        public const string LocationsPath = "Assets/Resources/Game Data/Battle/Locations";
        public const string NewLocationsPath = "Assets/Resources/Game Data/Battle/New Locations";
        public const string HeadgearItemsPath = "Assets/Core/Game Data/Resources/Items/Wearable/Headgear";
        public const string ArmorItemsPath = "Assets/Core/Game Data/Resources/Items/Wearable/Armor";
        public const string WeaponItemsPath = "Assets/Core/Game Data/Resources/Items/Wearable/Weapon";
        public const string CharmItemsPath = "Assets/Core/Game Data/Resources/Items/Wearable/Charm";
        public const string RingItemsPath = "Assets/Core/Game Data/Resources/Items/Wearable/Ring";
        public const string FootwearItemsPath = "Assets/Core/Game Data/Resources/Items/Wearable/Footwear";
        public const string NonWearableItemsPath = "Assets/Core/Game Data/Resources/Items/Non Wearable";
        public const string HeroShardsItemsPath = "Assets/Core/Game Data/Resources/Items/Non Wearable/Hero Shards";
        
        public const string EntitiesMetaPath = "Assets/Resources/Game Data/Entities";
        public const string SettingsMetaPath = "Assets/Resources/Game Data/Settings";

        // ICONS: ---------------------------------------------------------------------------------

        public const string NPCsIconPath = "/Ubica Editor/NPCs icon.png";
        public const string PresetsIconPath = "/Ubica Editor/Presets icon.png";
        public const string BehaviorsIconPath = "/Ubica Editor/Behaviors icon.png";

        public const string AbilitiesIconPath = "/Ubica Editor/Abilities icon.png";
        public const string EffectsIconPath = "/Ubica Editor/Effects icon.png";
        public const string StatsIconPath = "/Ubica Editor/Stats icon.png";
        

        public const string NPCsSubIconPath = "/Ubica Editor/NPCs sub icon.png";
        public const string PresetsSubIconPath = "/Ubica Editor/Presets sub icon.png";
        public const string BehaviorsSubIconPath = "/Ubica Editor/Behaviors sub icon.png";

        public const string AbilitiesSubIconPath = "/Ubica Editor/Abilities sub icon.png";
        public const string EffectsSubIconPath = "/Ubica Editor/Effects sub icon.png";
        public const string StatsSubIconPath = "/Ubica Editor/Stats sub icon.png";
    }
}