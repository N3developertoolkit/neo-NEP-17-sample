using System.Numerics;
using FluentAssertions;
using Neo;
using Neo.VM;
using NeoTestHarness;
using Xunit;
using Neo.Assertions;
using Neo.BlockchainToolkit.SmartContract;
using Neo.BlockchainToolkit.Models;
using Neo.BlockchainToolkit;
using Neo.SmartContract;

namespace ApocTokenTests
{
    [CheckpointPath("checkpoints/contract-deployed.neoxp-checkpoint")]
    public class TestApocToken : IClassFixture<CheckpointFixture<TestApocToken>>
    {
        const long TOTAL_SUPPLY = 2_000_000_000_000_000;
        readonly CheckpointFixture fixture;
        readonly ExpressChain chain;

        public TestApocToken(CheckpointFixture<TestApocToken> fixture)
        {
            this.fixture = fixture;
            this.chain = fixture.FindChain();
        }

        [Fact]
        public void test_symbol_and_decimals()
        {
            using var snapshot = fixture.GetSnapshot();
            var contract = snapshot.GetContract<ApocToken>();

            using var engine = new TestApplicationEngine(snapshot, ProtocolSettings.Default);
            engine.ExecuteScript<ApocToken>(c => c.symbol(), c => c.decimals());

            engine.State.Should().Be(VMState.HALT);
            engine.ResultStack.Should().HaveCount(2);
            engine.ResultStack.Peek(0).Should().BeEquivalentTo(8);
            engine.ResultStack.Peek(1).Should().BeEquivalentTo("APOC");
        }

        [Fact]
        public void test_initial_total_supply()
        {
            using var snapshot = fixture.GetSnapshot();
            var contract = snapshot.GetContract<ApocToken>();

            using var engine = new TestApplicationEngine(snapshot, ProtocolSettings.Default);
            engine.ExecuteScript<ApocToken>(c => c.totalSupply());

            engine.State.Should().Be(VMState.HALT);
            engine.ResultStack.Should().HaveCount(1);
            engine.ResultStack.Peek(0).Should().BeEquivalentTo(TOTAL_SUPPLY);
        }

        [Theory]
        [InlineData("owen", TOTAL_SUPPLY)]
        [InlineData("alice", 0)]
        public void test_balances(string accountName, long amount)
        {
            var settings = chain.GetProtocolSettings();
            var account = chain.GetDefaultAccount(accountName).ToScriptHash(chain.AddressVersion);

            using var snapshot = fixture.GetSnapshot();
            var contract = snapshot.GetContract<ApocToken>();

            using var engine = new TestApplicationEngine(snapshot, settings);
            engine.ExecuteScript<ApocToken>(c => c.balanceOf(account));

            engine.State.Should().Be(VMState.HALT);
            engine.ResultStack.Should().HaveCount(1);
            engine.ResultStack.Peek(0).Should().BeEquivalentTo(amount);
        }

        [Fact]
        public void test_transfer()
        {
            var sender = chain.GetDefaultAccount("owen").ToScriptHash(chain.AddressVersion);
            var receiver = chain.GetDefaultAccount("alice").ToScriptHash(chain.AddressVersion);
            var amount = 1000;

            using var snapshot = fixture.GetSnapshot();
            var contract = snapshot.GetContract<ApocToken>();

            using var engine = new TestApplicationEngine(snapshot, chain.GetProtocolSettings(), sender);
            engine.ExecuteScript<ApocToken>(c => c.transfer(sender, receiver, amount, null));

            engine.State.Should().Be(VMState.HALT);
            engine.ResultStack.Should().HaveCount(1);
            engine.ResultStack.Peek(0).Should().BeTrue();
            engine.Notifications.Should().HaveCount(1);
            engine.Notifications[0].Should()
                .BeSentBy(contract);
                // .And
                // .BeEquivalentTo<ApocToken.Events>(c => c.Transfer(sender, receiver, amount));
        }

        // [Fact]
        // public void test_storage()
        // {
        //     var owen = chain.GetDefaultAccount("owen").ToScriptHash(chain.AddressVersion);
        //     var alice = chain.GetDefaultAccount("alice").ToScriptHash(chain.AddressVersion);

        //     using var snapshot = fixture.GetSnapshot();

        //     var storages = snapshot.GetContractStorages<ApocToken>();
        //     storages.Should().HaveCount(2);

        //     var assets = storages.StorageMap("asset");
        //     assets.Should().HaveCount(2);
        //     assets.TryGetValue("enable", out var enable).Should().BeTrue();
        //     enable.Should().Be(1);
        //     assets.TryGetValue(owen, out var owenBalance).Should().BeTrue();
        //     owenBalance.Should().Be(TOTAL_SUPPLY);
        //     assets.TryGetValue(alice, out var _).Should().BeFalse();

        //     var contracts = storages.StorageMap("contract");
        //     contracts.Should().HaveCount(1);
        //     contracts.TryGetValue("totalSupply", out var totalSupply).Should().BeTrue();
        //     totalSupply.Should().Be(TOTAL_SUPPLY);
        // }
    }
}
