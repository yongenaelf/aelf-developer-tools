using AElf.Cryptography.ECDSA;
using AElf.Testing.TestBase;

namespace AElf.Contracts.SimpleDAO
{
    // The Module class load the context required for unit testing
    public class Module : ContractTestModule<SimpleDAO>
    {
        
    }
    
    // The TestBase class inherit ContractTestBase class, it defines Stub classes and gets instances required for unit testing
    public class TestBase : ContractTestBase<Module>
    {
        // The Stub class for unit testing
        internal readonly SimpleDAOContainer.SimpleDAOStub SimpleDAOStub;
        // A key pair that can be used to interact with the contract instance
        private ECKeyPair DefaultKeyPair => Accounts[0].KeyPair;

        public TestBase()
        {
            SimpleDAOStub = GetSimpleDAOContractStub(DefaultKeyPair);
        }

        private SimpleDAOContainer.SimpleDAOStub GetSimpleDAOContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<SimpleDAOContainer.SimpleDAOStub>(ContractAddress, senderKeyPair);
        }
    }
    
}