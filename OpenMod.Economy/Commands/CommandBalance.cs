using JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Permissions;
using OpenMod.Core.Users;
using OpenMod.Extensions.Economy.Abstractions;
using System;
using System.Threading.Tasks;

namespace OpenMod.Economy.Commands
{
    [Command("balance", Priority = Priority.Normal)]
    [CommandDescription("Shows the player's balance")]
    [CommandSyntax("[player]")]
    [RegisterCommandPermission(OthersPerm, Description = "Permission to see the balance of other players")]
    [UsedImplicitly]
    public class CommandBalance : Command
    {
        public const string OthersPerm = "others";

        private readonly IUserManager userManager;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IEconomyProvider economyProvider;

        public CommandBalance(IServiceProvider serviceProvider, IUserManager userManager, IStringLocalizer stringLocalizer, IEconomyProvider economyProvider) : base(serviceProvider)
        {
            this.userManager = userManager;
            this.stringLocalizer = stringLocalizer;
            this.economyProvider = economyProvider;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Actor.Type == KnownActorTypes.Console && Context.Parameters.Length == 0)
                throw new CommandWrongUsageException(Context);

            var other = false;

            IUser targetData = null;

            if (Context.Parameters.Length > 0)
            {
                var otherPermission = await CheckPermissionAsync(OthersPerm) == PermissionGrantResult.Grant;
                var target = await Context.Parameters.GetAsync<string>(0);
                targetData =
                    await userManager.FindUserAsync(KnownActorTypes.Player, target, UserSearchMode.FindByNameOrId);

                if (otherPermission)
                {
                    if (targetData == null)
                        throw new UserFriendlyException(stringLocalizer["economy:fail:user_not_found", new { target }]);

                    if (!Context.Actor.Id.Equals(targetData.Id, StringComparison.OrdinalIgnoreCase))
                        other = true;
                }
            }

            if (!other)
                targetData =
                    await userManager.FindUserAsync(Context.Actor.Type, Context.Actor.Id, UserSearchMode.FindById);

            var balance = await economyProvider.GetBalanceAsync(targetData.Id, targetData.Type);

            var message = other
                ? stringLocalizer["economy:success:show_balance_other",
                    new { Balance = balance, economyProvider.CurrencySymbol, targetData.DisplayName }]
                : stringLocalizer["economy:success:show_balance",
                    new { Balance = balance, economyProvider.CurrencySymbol }];

            await PrintAsync(message);
        }
    }
}
