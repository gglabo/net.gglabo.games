using GGLabo.Games.Runtime;
using NUnit.Framework;

namespace GGLabo.Games.Tests.Runtime.Units
{
    public class ActionStateTest
    {
        private ActionState _actionState;

        [SetUp]
        public void SetUp()
        {
            _actionState = new ActionState();
        }

        [Test]
        public void SetUpTest()
        {
            Assert.That(_actionState, Is.Not.EqualTo(default(ActionState)));
        }

        [Test]
        public void UpdateTest()
        {
            var callCount = 0;

            _actionState.Define(0, (state) =>
            {
                callCount++;
                state.Change(1);
            });

            _actionState.Define(1, (state) =>
            {
                callCount++;
                state.Next(0);
            });

            _actionState.Next(0);
            _actionState.Update();

            Assert.That(callCount, Is.EqualTo(2));
        }
    }
}