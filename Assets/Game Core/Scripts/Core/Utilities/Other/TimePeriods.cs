using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Utilities
{
    [Serializable]
    public class TimePeriods
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public TimePeriods() =>
            _timePeriods = new List<TimePeriod>();

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        [ListDrawerSettings(Expanded = true)]
        private List<TimePeriod> _timePeriods;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void UpdateTimeTexts()
        {
            foreach (TimePeriod timePeriod in _timePeriods)
                timePeriod.UpdateTimeText();
        }
        
        public bool IsTimeValid(int currentTimeInMinutes)
        {
            bool isListValid = _timePeriods.Count > 0;

            if (!isListValid)
            {
                Debug.LogError(message: "Empty list.");
                return false;
            }

            foreach (TimePeriod timePeriod in _timePeriods)
            {
                bool isTimeValid = timePeriod.IsTimeValid(currentTimeInMinutes);

                if (isTimeValid)
                    return true;
            }

            return false;
        }
    }
}