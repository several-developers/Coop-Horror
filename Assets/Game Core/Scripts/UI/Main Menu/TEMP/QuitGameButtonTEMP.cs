using GameCore.Gameplay.PubSub;
using GameCore.Infrastructure.Bootstrap;
using GameCore.UI.Global.Buttons;
using Zenject;

namespace GameCore.UI.MainMenu.TEMP
{
    public class QuitGameButtonTEMP : BaseButton
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        [Inject]
        private void Construct(IPublisher<QuitApplicationMessage> quitApplicationPublisher) =>
            _quitApplicationPublisher = quitApplicationPublisher;

        // FIELDS: --------------------------------------------------------------------------------
        
        private IPublisher<QuitApplicationMessage> _quitApplicationPublisher;

        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected override void ClickLogic()
        {
            Button.interactable = false;
            _quitApplicationPublisher.Publish(new QuitApplicationMessage());
        }
    }
}