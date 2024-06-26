using System;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElf.Contracts.Vote;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace AElf.Contracts.NftSale
{
    // This class is unit test class, and it inherit TestBase. Write your unit test code inside it
    public class NftSaleTests : TestBase
    {
        [Fact]
        public async Task InitializeContract_Success()
        {
            // Act
            var result = await NftSaleStub.Initialize.SendAsync(new Empty());
            
            // Assert
            result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);

            var owner = await NftSaleStub.GetOwner.CallAsync(new Empty());
            owner.Value.ShouldBe(DefaultAccount.Address.ToBase58());
        }

        [Fact]
        public async Task InitializeContract_Fail_AlreadyInitialized()
        {
            // Arrange
            await NftSaleStub.Initialize.SendAsync(new Empty());

            // Act & Assert
            Should.Throw<Exception>(async () => await NftSaleStub.Initialize.SendAsync(new Empty()));
        }

        [Fact]
        public async Task TradeNft_Success()
        {
            await NftSaleStub.Initialize.SendAsync(new Empty());
            await ApproveSpendingAsync(10000_0000000);
            await TokenContractStub.Approve.SendAsync(new ApproveInput
            {
                Spender = Accounts[1].Address,
                Symbol = "ELF",
                Amount = 100_00000000
            });
            await TokenContractStub.Approve.SendAsync(new ApproveInput
            {
                Spender = Accounts[0].Address,
                Symbol = "ELF",
                Amount = 100_00000000
            });
            //await SendTokenTo(Accounts[2].Address);
            await SendTokenTo(ContractAddress);
            //await SendTokenTo(Accounts[0].Address);
            await SendTokenTo(Accounts[1].Address);
            var initialContractBalance = await GetContractBalanceAsync(Accounts[0].Address);
            
            var initialContractBalance1 = await GetContractBalanceAsync(Accounts[1].Address);
            var price = new Price
            {
                Amount = 3,
                Symbol = "ELF"
            };
            await NftSaleStub.Purchase.SendAsync(new PurchaseInput
            {
                Amount = 1,
                Symbol = "ELF",
                Memo = "Test get resource",
                Price = price
            });
            
            var finalContractBalance = await GetContractBalanceAsync(Accounts[0].Address);
            var finalContractBalance2 = await GetContractBalanceAsync(Accounts[1].Address);
            finalContractBalance.ShouldBe(initialContractBalance + 3 -1);
        }
        
        private async Task ApproveSpendingAsync(long amount)
        {
            await TokenContractStub.Approve.SendAsync(new ApproveInput
            {
                Spender = ContractAddress,
                Symbol = "ELF",
                Amount = amount
            });
        }
        
        [Fact]
        public async Task Deposit_Success()
        {
            // Arrange
            await NftSaleStub.Initialize.SendAsync(new Empty());
            
            // Approve spending on the lottery contract
            await ApproveSpendingAsync(100_00000000);

            const long depositAmount = 10_000_000; // 0.1 ELF
            var depositInput = new Int64Value() { Value = depositAmount };

            var initialContractBalance = await GetContractBalanceAsync(ContractAddress);
            
            // Act
            var result = await NftSaleStub.Deposit.SendAsync(depositInput);

            // Assert
            result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);

            // Check balance update
            var finalContractBalance = await GetContractBalanceAsync(ContractAddress);
            finalContractBalance.ShouldBe(initialContractBalance + depositAmount);

            // Check if the event is emitted
            var events = result.TransactionResult.Logs;
            events.ShouldContain(log => log.Name == nameof(DepositEvent));
        }

        [Fact]
        public async Task Withdraw_Success()
        {
            // Arrange
            await NftSaleStub.Initialize.SendAsync(new Empty());
            
            // Approve spending on the lottery contract
            await ApproveSpendingAsync(100_00000000);

            const long depositAmount = 10_000_000; // 0.1 ELF
            var depositInput = new Int64Value() { Value = depositAmount };
            await NftSaleStub.Deposit.SendAsync(depositInput);

            const long withdrawAmount = 5_000_000; // 0.05 ELF
            var withdrawInput = new Int64Value() { Value = withdrawAmount };

            var initialSenderBalance = await GetTokenBalanceAsync(DefaultAccount.Address);
            var initialContractBalance = await GetContractBalanceAsync(ContractAddress);
            
            // Act
            var result = await NftSaleStub.Withdraw.SendAsync(withdrawInput);

            // Assert
            result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);

            // Check balance update
            var finalSenderBalance = await GetTokenBalanceAsync(DefaultAccount.Address);
            var finalContractBalance = await GetContractBalanceAsync(ContractAddress);

            finalSenderBalance.ShouldBe(initialSenderBalance + withdrawAmount);
            finalContractBalance.ShouldBe(initialContractBalance - withdrawAmount);

            // Check if the event is emitted
            var events = result.TransactionResult.Logs;
            events.ShouldContain(log => log.Name == nameof(WithdrawEvent));
        }

        [Fact]
        public async Task Withdraw_InsufficientBalance_Fail()
        {
            // Arrange
            await NftSaleStub.Initialize.SendAsync(new Empty());

            long withdrawAmount = 5_000_000; // 0.05 ELF
            var withdrawInput = new Int64Value() { Value = withdrawAmount };

            // Act & Assert
            Should.Throw<Exception>(async () => await NftSaleStub.Withdraw.SendAsync(withdrawInput));
        }

        private async Task<long> GetTokenBalanceAsync(Address owner)
        {
            return (await TokenContractStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = owner,
                Symbol = "ELF"
            })).Balance;
        }

        private async Task<long> GetContractBalanceAsync(Address address)
        {
            var input = new GetContractBalanceInput {Address = address};
            return (await NftSaleStub.GetContractBalance.CallAsync(input)).Value;
        }
        
        private async Task SendTokenTo(Address address)
        {
            await TokenContractStub.Transfer.SendAsync(new TransferInput
            {
                To = address,
                Symbol = "ELF",
                Amount = 100
            });
        }
    }
}