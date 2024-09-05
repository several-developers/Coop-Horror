using GameCore.Data;
using GameCore.Infrastructure.Providers.Global.Data;
using UnityEngine;

namespace GameCore.Infrastructure.Services.Global.Data
{
    public class GamesDataService : DataService, IGamesDataService
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GamesDataService(ISaveLoadService saveLoadService, IDataProvider dataProvider) : base(saveLoadService)
        {
            _gamesData = dataProvider.GetData<GamesData>();

            ValidateGamesData();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GamesData _gamesData;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ValidateGamesData()
        {
            const int iterations = Constants.SaveCellsAmount;
            bool saveData = false;

            for (int i = 0; i < iterations; i++)
            {
                bool isGameDataExists = _gamesData.TryGetGameData(i, out GameData gameData);

                if (isGameDataExists)
                {
                    bool isSaveCellIndexMatches = gameData.SaveCellIndex == i;

                    if (isSaveCellIndexMatches)
                        continue;

                    gameData.SetSaveCellIndex(index: i);
                    saveData = true;
                }
                else
                {
                    Debug.LogError($"Game Data with index ({i}) not found!");
                }
            }

            SaveLocalData(saveData);
        }
    }
}