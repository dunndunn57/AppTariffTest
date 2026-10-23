using RecordApp.Models;
using System.Reflection.PortableExecutable;

namespace RecordApp.Tests
{
    [TestClass]
    public class TariffFsmTests
    {
        /// <summary>
        /// TEACHING AID THEORY - THE STATE-TRANSITION MATRIX
        /// --------------------------------------------------------------------------
        /// FROM \ TO   | Draft       | Published   | Suspended   | Superseded  |
        /// ------------|-------------|-------------|-------------|-------------|
        /// Draft       | Ignore/Self | VALID       | INVALID     | INVALID     |
        /// Published   | INVALID     | Ignore/Self | VALID       | VALID       |
        /// Suspended   | INVALID     | VALID       | Ignore/Self | VALID       |
        /// Superseded  | INVALID     | INVALID     | INVALID     | Ignore/Self |
        /// --------------------------------------------------------------------------
        /// 
        /// EXIT CRITERIA METRICS:
        /// 1. 100% Transition Matrix Coverage (All valid explicitly checked).
        /// 2. Fault Injection Verification (All illegal permutations catch expected exception).
        /// </summary>

        #region 1. Test Condition: Initial Construction State Grounding

        [TestMethod]
        [Description("Verifies that a newly instantiated Tariff initializes safely to Draft and allows direct Setting for persistence.")]


        public void InitialConstruction_ShouldDefaultToDraft_AndAllowArbitraryStatePersistence()
        {
            // 1. Arrange & Act
            var tariff = new Tariff();

            // 2. Assert (Checking for Entry Exit Criteria)
            Assert.AreEqual(TariffStatus.Draft, tariff.Status, "A new tariff must default to Draft.");

            // 3. Verify XML Deserialization Guard (Bypassing validation loops on load)
            // Students can see here why separating internal persistence from method actions prevents load crashes.
            var historicalTariff = new Tariff(Guid.NewGuid(), 1.50m, DateOnly.FromDateTime(DateTime.Today), null,
                                TariffStatus.Suspended);

            Assert.AreEqual(TariffStatus.Suspended, historicalTariff.Status, "Constructor direct backing-field assignment must allow non - Draft values.");
        }
        #endregion

        #region 2. Test Condition: Valid State Transitions (Positive Test Matrix)

        [DataTestMethod]
        [DataRow(TariffStatus.Draft, TariffStatus.Published, "Publishing a draft")]
        // Test Needed
        // Test Needed
        // Test Needed       
        // Test Needed
        [Description("Executes every positive path edge defined by the business requirement matrix.")]
        
        public void ValidTransitions_ShouldSucceed_AndRaisePropertyNotification(TariffStatus initialState,
            TariffStatus targetState, string scenarioDescription)
        {
            // 1. Arrange: Establish system in its known baseline state via backing constructor
            var tariff = new Tariff(Guid.NewGuid(), 0.45m, DateOnly.FromDateTime(DateTime.Today), null,
                                    initialState);
            bool wasNotificationRaised = false;

            tariff.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(Tariff.Status)) wasNotificationRaised = true;
            };

            // 2. Act: Execute the business action under test
            ExecuteTransitionAction(tariff, targetState);

            // 3. Assert: Verify the condition meets the theoretical exit criteria
            Assert.AreEqual(targetState, tariff.Status, $"Failed Action: '{scenarioDescription}'. State change was blocked.");
            Assert.IsTrue(wasNotificationRaised, $"FSM Change Event: '{scenarioDescription}' failed to notify WPF binding subsystems.");
        }
        #endregion


        #region 3. Test Condition: Invalid State Transitions (Negative Test Matrix / Guard Rails)

        [DataTestMethod]
        // From Draft Errors
        [DataRow(TariffStatus.Draft, TariffStatus.Suspended)]
        // TEST NEEDED

        // From Superseded Errors
        [DataRow(TariffStatus.Superseded, TariffStatus.Published)]
        // TEST NEEDED 
        [Description("Ensures business rules throw InvalidOperationException whenever illegal FSM rules are violated.")]


        public void InvalidTransitions_ShouldThrowInvalidOperationException(TariffStatus initialState,
                                    TariffStatus forbiddenTargetState)
        {
            // 1. Arrange
            var tariff = new Tariff(Guid.NewGuid(), 0.45m, DateOnly.FromDateTime(DateTime.Today), null,
                                    initialState);

            // 2. Act & Assert: Exception validation acts as checking for exit criteria
            Assert.Throws<InvalidOperationException>( () =>
            {
                ExecuteTransitionAction(tariff, forbiddenTargetState);
            },

            $"FSM Vulnerability Found! Allowed illegal transition from {initialState} to {forbiddenTargetState}.");
        }
        #endregion

        #region 4. Test Condition: Self-Transitions (Idempotency)

        [DataTestMethod]
        [DataRow(TariffStatus.Draft)]
        // TEST NEEDED
        // TEST NEEDED
        // TEST NEEDED
        [Description("Checks that executing a self-transition does not throw an error and is quietly ignored.")]

        public void SelfTransitions_ShouldBeIdempotent_AndNotTriggerNotifications(TariffStatus state)
        {

            // 1. Arrange
            var tariff = new Tariff(Guid.NewGuid(), 0.45m, DateOnly.FromDateTime(DateTime.Today), null, state);
            bool wasNotificationRaised = false;
            tariff.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(Tariff.Status)) wasNotificationRaised
                                        = true;
            };

            // 2. Act
            ExecuteTransitionAction(tariff, state);

            // 3. Assert
            Assert.AreEqual(state, tariff.Status);
            Assert.IsFalse(wasNotificationRaised, "Performance defect: Re-applying the identical state fired redundant WPF UI notifications.");

        }
        #endregion





        #region Test Execution Orchestrator

        /// <summary>
        /// HELPER MAPPER: Maps targets dynamically to explicit transition methods.
        /// Demonstrates why production methods replace direct property modification.
        /// </summary>

        private static void ExecuteTransitionAction(Tariff tariff, TariffStatus target)
        {
            // If checking a direct property setting assignment (like self-assignment rules)
            if (tariff.Status == target)
            {
                tariff.Status = target;
                return;
            }

            switch (target)
            {
                case TariffStatus.Published:
                    tariff.Publish();
                    break;
                case TariffStatus.Suspended:
                    tariff.Suspend();
                    break;
                case TariffStatus.Superseded:
                    tariff.Supersede();
                    break;
                case TariffStatus.Draft:
                    // Forced assignment to simulate direct property tampering or flawed extensions
                    tariff.Status = target;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), "Unknown enum variant state mapping targeted.");
            }
        }

        #endregion


    }
}