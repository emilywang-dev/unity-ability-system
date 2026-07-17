using System;
using NUnit.Framework;
using Ability.Result;

namespace Tests.Ability.Result
{
    [TestFixture]
    public class AbilityResultTests
    {
        [Test]
        public void TryExecuteResult_Success_HasExpectedState()
        {
            TryExecuteResult result = TryExecuteResult.Success();

            Assert.IsTrue(result.Succeeded);
            Assert.IsFalse(result.IsOnCooldown);
            Assert.IsFalse(result.Failed);
            Assert.AreEqual(AbilityFailReason.None, result.FailReason);
        }

        [Test]
        public void TryExecuteResult_OnCooldown_HasExpectedState()
        {
            TryExecuteResult result = TryExecuteResult.OnCooldown();

            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.IsOnCooldown);
            Assert.IsFalse(result.Failed);
            Assert.AreEqual(AbilityFailReason.None, result.FailReason);
        }

        [Test]
        public void TryExecuteResult_FailWithBlocked_HasExpectedState()
        {
            TryExecuteResult result = TryExecuteResult.Fail(AbilityFailReason.Blocked);

            Assert.IsFalse(result.Succeeded);
            Assert.IsFalse(result.IsOnCooldown);
            Assert.IsTrue(result.Failed);
            Assert.AreEqual(AbilityFailReason.Blocked, result.FailReason);
        }

        [Test]
        public void TryExecuteResult_FailWithInterrupted_HasExpectedState()
        {
            TryExecuteResult result = TryExecuteResult.Fail(AbilityFailReason.Interrupted);

            Assert.IsFalse(result.Succeeded);
            Assert.IsFalse(result.IsOnCooldown);
            Assert.IsTrue(result.Failed);
            Assert.AreEqual(AbilityFailReason.Interrupted, result.FailReason);
        }

        [Test]
        public void TryExecuteResult_FailWithNone_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                TryExecuteResult.Fail(AbilityFailReason.None));
        }

        [Test]
        public void AbilityExecuteResult_Success_HasExpectedState()
        {
            AbilityExecuteResult result = AbilityExecuteResult.Success();

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(AbilityFailReason.None, result.FailReason);
        }

        [Test]
        public void AbilityExecuteResult_FailWithValidReason_HasExpectedState()
        {
            AbilityExecuteResult result = AbilityExecuteResult.Fail(AbilityFailReason.Interrupted);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(AbilityFailReason.Interrupted, result.FailReason);
        }

        [Test]
        public void AbilityExecuteResult_FailWithNone_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AbilityExecuteResult.Fail(AbilityFailReason.None));
        }
    }
}
