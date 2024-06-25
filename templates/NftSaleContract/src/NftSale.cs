using AElf.Contracts.MultiToken;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.NftSale
{
    // Contract class must inherit the base class generated from the proto file
    public class NftSale : NftSaleContainer.NftSaleBase
    {
        private const string TokenContractAddress = "ASh2Wt7nSEmYqnGxPPzp4pnVDU4uhj1XW9Se5VeZcX2UDdyjx"; // tDVW token contract address
        private const string TokenSymbol = "ELF";
        private const long MinimumPlayAmount = 1_000_000; // 0.01 ELF
        private const long MaximumPlayAmount = 1_000_000_000; // 10 ELF
        
        // Initializes the contract
        public override Empty Initialize(Empty input)
        {
            // Check if the contract is already initialized
            Assert(State.Initialized.Value == false, "Already initialized.");
            // Set the contract state
            State.Initialized.Value = true;
            // Set the owner address
            State.Owner.Value = Context.Sender;
            
            // Initialize the token contract
            State.TokenContract.Value = Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
            // The below code can be used to replace the above line. The below is a showcase of how you can reference to any contracts.
            // State.TokenContract.Value = Address.FromBase58(TokenContractAddress);
            State.ConsensusContract.Value = Context.GetContractAddressByName(SmartContractConstants.ConsensusContractSystemName);
            
            return new Empty();
        }
        
        // transfer nft
        public override Empty SaleNft(SaleNftInput input)
        {
            State.TokenContract.Transfer.Send(new TransferInput
            {
                To = Context.Sender,
                Symbol = input.Symbol,
                Amount = input.Amount,
                Memo = $"Transfer {input.Symbol}."
            });
            
            Context.Fire(new SaleNft
            {
                To = input,
                Symbol = input.Symbol,
                Amount = input.Amount,
            });
            
            return new Empty();
        }

        
        // A method that read the contract's current balance
        public override Int64Value GetContractBalance(Empty input)
        {
            // Get the balance of the contract
            var balance = State.TokenContract.GetBalance.Call(new GetBalanceInput
            {
                Owner = Context.Self,
                Symbol = TokenSymbol
            }).Balance;
            
            // Wrap the value in the return type
            return new Int64Value
            {
                Value = balance
            };
        }

        // A method that read the contract's owner
        public override StringValue GetOwner(Empty input)
        {
            return State.Owner.Value == null ? new StringValue() : new StringValue {Value = State.Owner.Value.ToBase58()};
        }
    }
    
}