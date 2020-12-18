using System.Collections.Generic;
using System.Numerics;
using FluentAssertions;
using Neo;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.VM;
using NeoTestHarness;
using Xunit;
using NeoAssertions;

using static test.Common;

namespace test
{
    public class ApocTokenTests : IClassFixture<ApocTokenTests.Fixture>
    {
        // Fixture is used to share checkpoint across multiple tests
        public class Fixture : CheckpointFixture
        {
            const string PATH = "checkpoints/contract-deployed.nxp3-checkpoint";
            public Fixture() : base(PATH) { }
        }

        readonly Fixture fixture;

        public ApocTokenTests(Fixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void test_symbol_and_decimals()
        {
            using var store = fixture.GetCheckpointStore();
            using var snapshot = new SnapshotView(store);

            using var engine = new TestApplicationEngine(snapshot);
            engine.ExecuteScript<ApocToken>(c => c.symbol(), c => c.decimals());

            engine.State.Should().Be(VMState.HALT);
            engine.ResultStack.Should().HaveCount(2);
            engine.ResultStack.Peek(0).Should().BeEquivalentTo(8);
            engine.ResultStack.Peek(1).Should().BeEquivalentTo("APOC");
        }

        [Fact]
        public void test_owen_is_owner()
        {
            using var store = fixture.GetCheckpointStore();
            using var snapshot = new SnapshotView(store);

            using var engine = new TestApplicationEngine(snapshot, OWEN);
            engine.ExecuteScript<ApocToken>(c => c.verify());

            engine.State.Should().Be(VMState.HALT);
            engine.ResultStack.Should().HaveCount(1);
            engine.ResultStack.Peek(0).Should().BeTrue();
        }

        [Fact]
        public void test_alice_is_not_owner()
        {
            using var store = fixture.GetCheckpointStore();
            using var snapshot = new SnapshotView(store);

            using var engine = new TestApplicationEngine(snapshot, ALICE);
            engine.ExecuteScript<ApocToken>(c => c.verify());

            engine.State.Should().Be(VMState.HALT);
            engine.ResultStack.Should().HaveCount(1);
            engine.ResultStack.Peek(0).Should().BeFalse();
        }

        [Fact]
        public void test_initial_total_supply()
        {
            using var store = fixture.GetCheckpointStore();
            using var snapshot = new SnapshotView(store);

            using var engine = new TestApplicationEngine(snapshot, ALICE);
            engine.ExecuteScript<ApocToken>(c => c.totalSupply());

            engine.State.Should().Be(VMState.HALT);
            engine.ResultStack.Should().HaveCount(1);
            engine.ResultStack.Peek(0).Should().BeEquivalentTo(2_000_000_000_000_000);
        }

        public static IEnumerable<object[]> GetBalances()
        {
            yield return new object[] { OWEN, 2_000_000_000_000_000 };
            yield return new object[] { ALICE, 0 };
        }

        [Theory]
        [MemberData(nameof(GetBalances))]
        public void test_balances(UInt160 account, BigInteger amount)
        {
            using var store = fixture.GetCheckpointStore();
            using var snapshot = store.CreateSnapshot();

            using var engine = new TestApplicationEngine(snapshot);
            engine.ExecuteScript<ApocToken>(c => c.balanceOf(account));

            engine.State.Should().Be(VMState.HALT);
            engine.ResultStack.Should().HaveCount(1);
            engine.ResultStack.Peek(0).Should().BeEquivalentTo(amount);
        }

        [Fact]
        public void test_transfer()
        {
            using var store = fixture.GetCheckpointStore();
            using var snapshot = store.CreateSnapshot(new Block());

            var sender = OWEN;
            var receiver = ALICE;
            var amount = 1000;

            using var engine = new TestApplicationEngine(snapshot, sender);
            engine.ExecuteScript<ApocToken>(c => c.transfer(sender, receiver, amount, null));

            engine.State.Should().Be(VMState.HALT);
            engine.ResultStack.Should().HaveCount(1);
            engine.ResultStack.Peek(0).Should().BeTrue();


            // Assert.True(engine.ResultStack.Pop().GetBoolean());
            // Assert.Empty(engine.ResultStack);

            // Assert.Single(engine.Notifications);
            // engine.AssertNotification<ApocToken.Events>(0, c => c.Transfer(sender, receiver, amount));
        }
    }
}
