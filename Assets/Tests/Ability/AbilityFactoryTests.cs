using System;
using NUnit.Framework;
using Ability;
using Ability.Abilities;
using Ability.Config;

namespace Tests.Ability
{
    [TestFixture]
    public class AbilityFactoryTests
    {
        private AbilityFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new AbilityFactory();
        }

        [Test]
        public void Create_Damage_ReturnsDamageAbility()
        {
            IAbility ability = _factory.Create(AbilityType.Damage);

            Assert.IsInstanceOf<DamageAbility>(ability);
        }

        [Test]
        public void GetExpectedConfigType_Damage_ReturnsDamageAbilityConfig()
        {
            Assert.AreEqual(typeof(DamageAbilityConfig), _factory.GetExpectedConfigType(AbilityType.Damage));
        }

        [Test]
        public void Create_None_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _factory.Create(AbilityType.None));
        }

        [Test]
        public void Create_UnknownValue_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _factory.Create((AbilityType)999));
        }

        [Test]
        public void GetExpectedConfigType_None_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _factory.GetExpectedConfigType(AbilityType.None));
        }

        [Test]
        public void GetExpectedConfigType_UnknownValue_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _factory.GetExpectedConfigType((AbilityType)999));
        }

        [Test]
        public void Register_ThenCreate_ReturnsRegisteredAbility()
        {
            var registered = new MockAbility();
            _factory.Register((AbilityType)100, () => registered, typeof(StubAbilityConfig));

            IAbility ability = _factory.Create((AbilityType)100);

            Assert.AreSame(registered, ability);
            Assert.AreEqual(typeof(StubAbilityConfig), _factory.GetExpectedConfigType((AbilityType)100));
        }

        [Test]
        public void Register_Duplicate_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _factory.Register(AbilityType.Damage, () => new DamageAbility(), typeof(DamageAbilityConfig)));
        }

        [Test]
        public void Register_None_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _factory.Register(AbilityType.None, () => new DamageAbility(), typeof(DamageAbilityConfig)));
        }

        [Test]
        public void Register_NullCreator_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _factory.Register((AbilityType)100, null, typeof(DamageAbilityConfig)));
        }

        [Test]
        public void Register_NullExpectedConfigType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _factory.Register((AbilityType)100, () => new DamageAbility(), null));
        }

        [Test]
        public void Register_NonAbilityConfigType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                _factory.Register((AbilityType)100, () => new DamageAbility(), typeof(string)));
        }
    }
}
