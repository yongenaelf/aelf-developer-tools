using System.Collections.Generic;
using System.IO;
using AElf.ContractTestBase;
using AElf.ContractTestBase.ContractTestKit;
using AElf.Kernel.Consensus.AEDPoS;
using AElf.Kernel.Miner.Application;
using AElf.Kernel.SmartContract.Application;
using AElf.OS.Node.Application;
using AElf.Runtime.CSharp;
using AElf.Sdk.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace AElf.Testing.TestBase
{
    [DependsOn(typeof(MainChainDAppContractTestModule))]
    public class ContractTestModule<T> : MainChainDAppContractTestModule where T : CSharpSmartContractAbstract 
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<IContractInitializationProvider, ContractInitializationProvider<T>>();

            context.Services.RemoveAll<IPreExecutionPlugin>();
            context.Services.RemoveAll<IPostExecutionPlugin>();
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            var contractCodeProvider = context.ServiceProvider.GetService<IContractCodeProvider>();
            var contractDllLocation = typeof(T).Assembly.Location;
            var contractCodes = new Dictionary<string, byte[]>(contractCodeProvider.Codes)
            {
                {
                    new ContractInitializationProvider<T>().ContractCodeName,
                    File.ReadAllBytes(contractDllLocation)
                }
            };
            contractCodeProvider.Codes = contractCodes;
        }
    }
    
    [DependsOn(typeof(SideChainContractTestModule))]
    public class SideChainDAppContractTestModule : SideChainContractTestModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            base.ConfigureServices(context);
            Configure<RunnerOptions>(o =>
            {
                o.SdkDir = Path.GetDirectoryName(typeof(SideChainContractTestModule).Assembly.Location);
            });
            context.Services.AddSingleton<IRefBlockInfoProvider, RefBlockInfoProvider>();
            context.Services.AddSingleton<IGenesisSmartContractDtoProvider, GenesisSmartContractDtoProvider>();
            context.Services.AddSingleton<IContractCodeProvider, ContractCodeProvider>();
            context.Services.AddSingleton<IContractDeploymentListProvider, SideChainDAppContractTestDeploymentListProvider>();
            context.Services.AddSingleton<IContractCodeProvider, ContractCodeProvider>();

            Configure<ConsensusOptions>(options =>
            {
                options.MiningInterval = 4000;
                options.InitialMinerList = new List<string> {SampleAccount.Accounts[0].KeyPair.PublicKey.ToHex()};
            });
            
            context.Services.RemoveAll<ISystemTransactionGenerator>();
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
        }
    }

    [DependsOn(typeof(MainChainContractTestModule))]
    public class MainChainDAppContractTestModule : MainChainContractTestModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            base.ConfigureServices(context);
            Configure<RunnerOptions>(o =>
            {
                o.SdkDir = Path.GetDirectoryName(typeof(MainChainDAppContractTestModule).Assembly.Location);
            });
            context.Services.AddSingleton<IRefBlockInfoProvider, RefBlockInfoProvider>();
            context.Services.AddSingleton<IGenesisSmartContractDtoProvider, GenesisSmartContractDtoProvider>();
            context.Services.AddSingleton<IContractCodeProvider, ContractCodeProvider>();
            context.Services.AddSingleton<IContractDeploymentListProvider, MainChainDAppContractTestDeploymentListProvider>();
            
            Configure<ConsensusOptions>(options =>
            {
                options.MiningInterval = 4000;
                options.InitialMinerList = new List<string> {SampleAccount.Accounts[0].KeyPair.PublicKey.ToHex()};
            });

            context.Services.RemoveAll<ISystemTransactionGenerator>();
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
        }
    }
    
}