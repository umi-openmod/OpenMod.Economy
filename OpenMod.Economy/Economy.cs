using Microsoft.Extensions.Configuration;
using OpenMod.API.Eventing;
using OpenMod.API.Plugins;
using OpenMod.Core.Plugins;
using OpenMod.Economy.Model;
using OpenMod.Extensions.PlayerLibrary.Abstractions;
using OpenMod.SqlSugarCore;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Players.Connections.Events;
using SqlSugar;
using System;
using System.Threading.Tasks;

[assembly:
    PluginMetadata("Openmod.Economy", Author = "OpenMod,Rube200", DisplayName = "Openmod.Economy",
        Website = "https://github.com/openmodplugins/OpenMod.Economy")]

namespace OpenMod.Economy
{
    public class Economy : OpenModUniversalPlugin
    {
        private readonly IEventBus eventBus;
        private readonly ISqlSugarClient sqlSugarClient;
        private readonly IRepository<EconomyModel> economyRepository;
        private readonly IPlayerLibraryProvider playerLibraryProvider;

        private decimal Default_Balance;
        public Economy(IServiceProvider serviceProvider, IEventBus eventBus, ISqlSugarClient sqlSugarClient, IRepository<EconomyModel> economyRepository, IPlayerLibraryProvider playerLibraryProvider) : base(serviceProvider)
        {
            this.eventBus = eventBus;
            this.sqlSugarClient = sqlSugarClient;
            this.economyRepository = economyRepository;
            this.playerLibraryProvider = playerLibraryProvider;

            Default_Balance = Configuration.GetSection("Economy").GetSection("Default_Balance").Get<decimal>();
        }

        protected override async Task OnLoadAsync()
        {
            string EconomyTable = Configuration.GetSection("Database").GetSection("EconomyTable").Get<string>();
            if (!sqlSugarClient.DbMaintenance.IsAnyTable(EconomyTable))
            {
                sqlSugarClient.CodeFirst.InitTables<EconomyModel>();
                playerLibraryProvider.ForeignKey(EconomyTable, "PlayerId");
            }

            eventBus.Subscribe<UnturnedPlayerConnectedEvent>(this, async (serviceProvider, sender, @even) =>
            {
                UnturnedPlayer player = @even.Player;
                int PlayerId = playerLibraryProvider.FindPlayer(player.SteamId.m_SteamID.ToString());
                EconomyModel economyModel = await economyRepository.Queryable().InSingleAsync(PlayerId);
                if (economyModel is null)
                {
                    economyModel = new(PlayerId, Default_Balance);
                    await economyRepository.Insertable(economyModel).ExecuteCommandAsync(); ;
                }
            });
        }

        protected override async Task OnUnloadAsync()
        {
            eventBus.Unsubscribe<UnturnedPlayerConnectedEvent>(this);
        }
    }
}
