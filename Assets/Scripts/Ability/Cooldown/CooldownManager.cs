using System;
using System.Collections.Generic;
using Core.Events;
using Ability.Events;

namespace Ability.Cooldown
{
    /// <summary>
    /// Tracks per-slot cooldown state for a single <see cref="AbilitySystem"/>.
    /// </summary>
    public sealed class CooldownManager : ICooldownQuery
    {
        private struct CooldownState
        {
            public float Remaining;
            public float Duration;
        }

        private readonly IEventBus _eventBus;
        private readonly Dictionary<int, CooldownState> _stateByIndex = new();
        private readonly List<int> _endedIndices = new();
        private readonly List<int> _tickIndices = new();

        public CooldownManager(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public bool IsOnCooldown(int abilityIndex) => GetRemaining(abilityIndex) > 0f;

        public float GetRemaining(int abilityIndex) =>
            _stateByIndex.TryGetValue(abilityIndex, out CooldownState state) ? state.Remaining : 0f;

        public float GetDuration(int abilityIndex) =>
            _stateByIndex.TryGetValue(abilityIndex, out CooldownState state) ? state.Duration : 0f;

        public float GetNormalized(int abilityIndex)
        {
            float duration = GetDuration(abilityIndex);
            if (duration <= 0f)
            {
                return 0f;
            }

            return GetRemaining(abilityIndex) / duration;
        }

        public void StartCooldown(int abilityIndex, float duration)
        {
            if (duration < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "Cooldown duration must be non-negative.");
            }

            if (duration == 0f)
            {
                return;
            }

            _stateByIndex[abilityIndex] = new CooldownState
            {
                Remaining = duration,
                Duration = duration
            };
            _eventBus.Publish(new CooldownStartedEvent(abilityIndex, duration));
        }

        // Trusts engine-driven non-negative deltaTime; negative values rewind remaining without ending.
        public void Tick(float deltaTime)
        {
            if (_stateByIndex.Count == 0)
            {
                return;
            }

            _endedIndices.Clear();
            _tickIndices.Clear();

            foreach (int abilityIndex in _stateByIndex.Keys)
            {
                _tickIndices.Add(abilityIndex);
            }

            for (int i = 0; i < _tickIndices.Count; i++)
            {
                int abilityIndex = _tickIndices[i];
                CooldownState state = _stateByIndex[abilityIndex];
                float remaining = state.Remaining - deltaTime;

                if (remaining <= 0f)
                {
                    _endedIndices.Add(abilityIndex);
                }
                else
                {
                    state.Remaining = remaining;
                    _stateByIndex[abilityIndex] = state;
                }
            }

            foreach (int abilityIndex in _endedIndices)
            {
                _stateByIndex.Remove(abilityIndex);
                _eventBus.Publish(new CooldownEndedEvent(abilityIndex));
            }
        }
    }
}
