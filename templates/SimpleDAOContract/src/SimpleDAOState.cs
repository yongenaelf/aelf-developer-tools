using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AElf.Contracts.SimpleDAO;
using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace AElf.Contracts.SimpleDAO
{
    // The state class is access the blockchain state
    public class SimpleDAOState : ContractState
    {
        public BoolState Initialized { get; set; }
        public MappedState<Address, bool> Members { get; set; }
        public MappedState<string, Proposal> Proposals { get; set; }
        public Int32State MemberCount { get; set; }
        public Int32State NextProposalId { get; set; }
    }
}