using System.Collections.Generic;
using System.Numerics;
using Neo;
using Neo.Persistence;
using NeoTestHarness;
using Xunit;

using static test.Common;

namespace test
{

    public class Tests : IClassFixture<Tests.Fixture>
    {
        // Fixture is used to share checkpoint across multiple tests
        public class Fixture : CheckpointFixture
        {
            const string PATH = "checkpoints/contract-deployed.nxp3-checkpoint";
            public Fixture() : base(PATH) { }
        }

        readonly Fixture fixture;

        public Tests(Fixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void test_symbol_and_decimals()
        {
            using var store = fixture.GetCheckpointStore();
            using var snapshot = new SnapshotView(store);

            using var engine = new TestApplicationEngine(snapshot);
            engine.AssertScript<ApocToken>(c => c.symbol(), c => c.decimals());

            Assert.Equal(8, engine.ResultStack.Pop().GetInteger());
            Assert.Equal("APOC", engine.ResultStack.Pop().GetString());
            Assert.Empty(engine.ResultStack);
        }

        [Fact]
        public void test_owen_is_owner()
        {
            using var store = fixture.GetCheckpointStore();
            using var snapshot = new SnapshotView(store);

            using var engine = new TestApplicationEngine(snapshot, OWEN);
            engine.AssertScript<ApocToken>(c => c.verify());

            Assert.True(engine.ResultStack.Pop().GetBoolean());
            Assert.Empty(engine.ResultStack);
        }

        [Fact]
        public void test_alice_is_not_owner()
        {
            using var store = fixture.GetCheckpointStore();
            using var snapshot = new SnapshotView(store);

            using var engine = new TestApplicationEngine(snapshot, ALICE);
            engine.AssertScript<ApocToken>(c => c.verify());

            Assert.False(engine.ResultStack.Pop().GetBoolean());
            Assert.Empty(engine.ResultStack);
        }

        [Fact]
        public void test_initial_total_supply()
        {
            using var store = fixture.GetCheckpointStore();
            using var snapshot = new SnapshotView(store);

            using var engine = new TestApplicationEngine(snapshot, ALICE);
            engine.AssertScript<ApocToken>(c => c.totalSupply());

            Assert.Equal(2_000_000_000_000_000, engine.ResultStack.Pop().GetInteger());
            Assert.Empty(engine.ResultStack);
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
            using var snapshot = new SnapshotView(store);

            using var engine = new TestApplicationEngine(snapshot);
            engine.AssertScript<ApocToken>(c => c.balanceOf(account));

            Assert.Equal(amount, engine.ResultStack.Pop().GetInteger());
            Assert.Empty(engine.ResultStack);
        }
    }
}
