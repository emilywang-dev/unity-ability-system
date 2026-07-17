using System;
using System.Collections.Generic;
using System.Reflection;
using Core;
using Core.Events;
using Gameplay.Combat;
using UnityEngine;
using Ability;
using Ability.Buff;
using Ability.Config;
using Ability.Events;
using Ability.Result;

namespace Tests.Ability
{
    internal sealed class AbilityRecordingEventBus : IEventBus
    {
        public List<CooldownStartedEvent> CooldownStartedEvents { get; } = new();

        public List<CooldownEndedEvent> CooldownEndedEvents { get; } = new();

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
        }

        public void Publish<T>(T evt) where T : IEvent
        {
            if (evt is CooldownStartedEvent started)
            {
                CooldownStartedEvents.Add(started);
            }
            else if (evt is CooldownEndedEvent ended)
            {
                CooldownEndedEvents.Add(ended);
            }
        }
    }

    internal sealed class FixedRandomProvider : IRandomProvider
    {
        private readonly float _nextFloat;

        public FixedRandomProvider(float nextFloat)
        {
            _nextFloat = nextFloat;
        }

        public float NextFloat() => _nextFloat;
    }

    internal sealed class RecordingBuffApplier : IBuffApplier
    {
        public List<(BuffConfig Config, ICombatEntity Instigator)> ApplyCalls { get; } = new();

        public void Apply(BuffConfig config, ICombatEntity instigator = null)
        {
            ApplyCalls.Add((config, instigator));
        }
    }

    internal sealed class OrderRecordingBuffApplier : IBuffApplier
    {
        private readonly Func<bool> _damageEventPublished;

        public List<(BuffConfig Config, ICombatEntity Instigator)> ApplyCalls { get; } = new();

        public bool DamageEventExistedBeforeApply { get; private set; }

        public OrderRecordingBuffApplier(Func<bool> damageEventPublished)
        {
            _damageEventPublished = damageEventPublished;
        }

        public void Apply(BuffConfig config, ICombatEntity instigator = null)
        {
            DamageEventExistedBeforeApply = _damageEventPublished();
            ApplyCalls.Add((config, instigator));
        }
    }

    internal sealed class MockAbility : IAbility
    {
        public bool CanExecuteResult = true;
        public AbilityExecuteResult ExecuteResult = AbilityExecuteResult.Success();
        public int CanExecuteCallCount;
        public int ExecuteCallCount;

        public bool CanExecute(in AbilityContext context)
        {
            CanExecuteCallCount++;
            return CanExecuteResult;
        }

        public AbilityExecuteResult Execute(in AbilityContext context)
        {
            ExecuteCallCount++;
            return ExecuteResult;
        }
    }

    // Returns a fixed flyweight so AbilitySystemTests can drive MockAbility without AbilityType.Damage.
    internal sealed class FixedAbilityFactory : IAbilityFactory
    {
        private readonly IAbility _ability;

        public FixedAbilityFactory(IAbility ability)
        {
            _ability = ability ?? throw new ArgumentNullException(nameof(ability));
        }

        public IAbility Create(AbilityType abilityType) => _ability;

        // Accept any AbilityConfig so mock slots are not blocked by type matching.
        public Type GetExpectedConfigType(AbilityType abilityType) => typeof(AbilityConfig);
    }

    internal static class AbilityConfigTestFactory
    {
        public static DamageAbilityConfig CreateDamageConfig(
            float cooldownDuration = 0f,
            float damageCoefficient = 1f,
            DamageType damageType = DamageType.Physical,
            BuffConfig onHitBuff = null)
        {
            var config = ScriptableObject.CreateInstance<DamageAbilityConfig>();
            SetBaseField(config, "_cooldownDuration", cooldownDuration);
            SetBaseField(config, "_onHitBuff", onHitBuff);
            SetField(config, "_damageCoefficient", damageCoefficient);
            SetField(config, "_damageType", damageType);
            return config;
        }

        public static BuffConfig CreateBuffConfig()
        {
            return ScriptableObject.CreateInstance<BuffConfig>();
        }

        public static AbilityConfig CreateStubConfig(float cooldownDuration = 0f)
        {
            var config = ScriptableObject.CreateInstance<StubAbilityConfig>();
            SetBaseField(config, "_cooldownDuration", cooldownDuration);
            return config;
        }

        private static void SetBaseField(AbilityConfig config, string fieldName, object value)
        {
            FieldInfo field = typeof(AbilityConfig).GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(config, value);
        }

        private static void SetField(DamageAbilityConfig config, string fieldName, object value)
        {
            FieldInfo field = typeof(DamageAbilityConfig).GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(config, value);
        }
    }

    // Non-DamageAbilityConfig used to assert typed config fail-fast in DamageAbility.
    internal sealed class StubAbilityConfig : AbilityConfig
    {
    }
}
