using GameCore.Infrastructure.Data;
using GameCore.Infrastructure.Providers.Global.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Other
{
    public class DataManagerDisplay : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IDataProvider dataProvider) =>
            _dataManager = dataProvider.GetDataManager();

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, InlineProperty, HideLabel]
        private DataManager _dataManager;
    }
}