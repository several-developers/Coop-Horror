using GameCore.Gameplay.Events;

namespace GameCore.Infrastructure.Services.Global.Data
{
    public abstract class DataService
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        protected DataService(ISaveLoadService saveLoadService) =>
            _saveLoadService = saveLoadService;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly ISaveLoadService _saveLoadService;

        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected void SaveLocalData(bool autoSave = true)
        {
#if UNITY_EDITOR
            SendDataChangedEvent();
#endif
            
            if (!autoSave)
                return;
            
            _saveLoadService.Save();
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

#if UNITY_EDITOR
        private static void SendDataChangedEvent() =>
            EditorEvents.SendDataChanged();
#endif
    }
}