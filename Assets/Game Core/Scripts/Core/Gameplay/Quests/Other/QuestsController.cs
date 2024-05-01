namespace GameCore.Gameplay.Quests
{
    public class QuestsController
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuestsController(QuestsStorage questsStorage) =>
            _questsStorage = questsStorage;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly QuestsStorage _questsStorage;
    }
}