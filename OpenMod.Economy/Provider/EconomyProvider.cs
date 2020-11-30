using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Ioc;
using OpenMod.Extensions.Economy.Abstractions;
using OpenMod.Extensions.PlayerLibrary.Abstractions;
using System.Threading.Tasks;

namespace OpenMod.Economy.Controllers
{
    [PluginServiceImplementation]
    [UsedImplicitly]
    public class EconomyProvider : IEconomyProvider
    {
        private readonly IConfiguration configuration;
        private readonly IEconomyRepository economyRepository;
        private readonly IPlayerLibraryProvider playerLibraryProvider;
        public string CurrencyName { get; set; }
        public string CurrencySymbol { get; set; }

        public EconomyProvider(IConfiguration configuration,
            IEconomyRepository economyRepository,
            IPlayerLibraryProvider playerLibraryProvider)
        {
            this.configuration = configuration;
            this.economyRepository = economyRepository;
            this.playerLibraryProvider = playerLibraryProvider;

            CurrencyName = configuration.GetSection("Economy:CurrencyName").Get<string>();
            CurrencySymbol = configuration.GetSection("Economy:CurrencySymbol").Get<string>();
        }

        public async Task<decimal> GetBalanceAsync(string ownerId, string ownerType)
        {
            int PlayerId = playerLibraryProvider.FindPlayer(ownerId);
            return await economyRepository.GetBalanceAsync(PlayerId);
        }

        public async Task SetBalanceAsync(string ownerId, string ownerType, decimal balance)
        {
            int PlayerId = playerLibraryProvider.FindPlayer(ownerId);
            await economyRepository.SetBalanceAsync(PlayerId, balance);
        }

        public async Task<decimal> UpdateBalanceAsync(string ownerId, string ownerType, decimal changeAmount, string reason)
        {
            int PlayerId = playerLibraryProvider.FindPlayer(ownerId);
            return await economyRepository.UpdateBalanceAsync(PlayerId, changeAmount);
        }
    }
}
