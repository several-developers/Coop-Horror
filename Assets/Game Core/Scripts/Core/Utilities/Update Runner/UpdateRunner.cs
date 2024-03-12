using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Utilities
{
    /// <summary>
    /// Some objects might need to be on a slower update loop than the usual MonoBehaviour Update and
    /// without precise timing, e.g. to refresh data from services.
    /// Some might also not want to be coupled to a Unity object at all but still need an update loop.
    /// </summary>
    public class UpdateRunner : MonoBehaviour, IUpdateRunner
    {
        // FIELDS: --------------------------------------------------------------------------------

        private readonly Queue<Action> _pendingHandlers = new();
        private readonly HashSet<Action<float>> _subscribers = new();
        private readonly Dictionary<Action<float>, SubscriberData> _subscriberData = new();

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => DontDestroyOnLoad(gameObject);

        /// <summary>
        /// Each frame, advance all subscribers. Any that have hit their period should then act,
        /// though if they take too long they could be removed.
        /// </summary>
        private void Update()
        {
            while (_pendingHandlers.Count > 0)
                _pendingHandlers.Dequeue()?.Invoke();

            foreach (Action<float> subscriber in _subscribers)
            {
                SubscriberData subscriberData = _subscriberData[subscriber];

                if (Time.time < subscriberData.NextCallTime)
                    continue;

                subscriber.Invoke(Time.deltaTime);
                subscriberData.NextCallTime = Time.time + subscriberData.Period;
            }
        }

        private void OnDestroy()
        {
            _pendingHandlers.Clear();
            _subscribers.Clear();
            _subscriberData.Clear();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        /// <summary>
        /// Subscribe in order to have onUpdate called approximately every period seconds (or every frame, if period <= 0).
        /// Don't assume that onUpdate will be called in any particular order compared to other subscribers.
        /// </summary>
        public void Subscribe(Action<float> onUpdate, float updatePeriod)
        {
            if (onUpdate == null)
                return;

            // Detect a local function that cannot be Unsubscribed since it could go out of scope.
            if (onUpdate.Target == null)
            {
                Debug.LogError("Can't subscribe to a local function that can go out of scope and " +
                               "can't be unsubscribed from");
                return;
            }

            // Detect
            if (onUpdate.Method.ToString().Contains("<"))
            {
                Debug.LogError("Can't subscribe with an anonymous function that cannot be Unsubscribed, " +
                               "by checking for a character that can't exist in a declared method name.");
                return;
            }

            if (_subscribers.Contains(onUpdate))
                return;
            
            _pendingHandlers.Enqueue(item: () =>
            {
                if (!_subscribers.Add(onUpdate))
                    return;
                
                _subscriberData.Add(onUpdate, new SubscriberData { Period = updatePeriod, NextCallTime = 0 });
            });
        }

        /// <summary>
        /// Safe to call even if onUpdate was not previously Subscribed.
        /// </summary>
        public void Unsubscribe(Action<float> onUpdate)
        {
            _pendingHandlers.Enqueue(item: () =>
            {
                _subscribers.Remove(onUpdate);
                _subscriberData.Remove(onUpdate);
            });
        }

        // OTHER: ---------------------------------------------------------------------------------

        private class SubscriberData
        {
            public float Period;
            public float NextCallTime;
        }
    }
}