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
        public async Task SaleNft_Success()
        {
            await NftSaleStub.Initialize.SendAsync(new Empty());
            await ApproveSpendingAsync(100_00000000);
            await SendTokenTo(ContractAddress);
            var initialContractBalance = await GetContractBalanceAsync(ContractAddress);
            
            var initialContractBalance1 = await GetContractBalanceAsync(Accounts[1].Address);
            await NftSaleStub.SaleNft.SendAsync(new SaleNftInput
            {
                Amount = 1,
                Symbol = "ELF",
                To = Accounts[1].Address,
                Memo = "Test get resource"
            });
            
            var finalContractBalance = await GetContractBalanceAsync(ContractAddress);
            var finalContractBalance2 = await GetContractBalanceAsync(Accounts[1].Address);
            finalContractBalance2.ShouldBe(initialContractBalance1 + 1);
            
            finalContractBalance.ShouldBe(initialContractBalance - 1);
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