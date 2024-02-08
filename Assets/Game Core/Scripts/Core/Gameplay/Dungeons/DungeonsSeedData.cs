namespace GameCore.Gameplay.Dungeons
{
    public struct DungeonsSeedData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public DungeonsSeedData(int seedOne, int seedTwo, int seedThree)
        {
            SeedOne = seedOne;
            SeedTwo = seedTwo;
            SeedThree = seedThree;
        }

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public int SeedOne { get; }
        public int SeedTwo { get; }
        public int SeedThree { get; }
    }
}