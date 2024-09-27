namespace GameCore.UI.Global.Currency
{
    public class GoldCurrency : CurrencyBase
    {
        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void UpdateValue()
        {
            if (!gameObject.activeSelf)
                return;

            long playerMoney = GetGold();

            StopValueUpdater();
            StartValueUpdater(playerMoney);
        }

        protected override void UpdateValueInstant()
        {
            LastCurrency = GetGold();
            UpdateValueText((int)LastCurrency);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private long GetGold()
        {
            return 0;
        }
    }
}