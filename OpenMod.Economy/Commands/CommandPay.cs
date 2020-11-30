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
    [Command("pay", Priority = Priority.Normal)]
    [CommandDescription("Pay to a user")]
    [CommandSyntax("<player> <amount> [reason]")]
    [RegisterCommandPermission(PayToSelf, Description = "Permission to increase/decrease the own balance")]
    [UsedImplicitly]
    public class CommandPay : Command
    {
        public const string PayToSelf = "self";

        private readonly IUserManager userManager;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IEconomyProvider economyProvider;

        public CommandPay(IServiceProvider serviceProvider, IUserManager userManager, IStringLocalizer stringLocalizer, IEconomyProvider economyProvider) : base(serviceProvider)
        {
            this.userManager = userManager;
            this.stringLocalizer = stringLocalizer;
            this.economyProvider = economyProvider;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Length < 2) throw new CommandWrongUsageException(Context);

            decimal amount = await Context.Parameters.GetAsync<decimal>(1);
            bool isConsole = Context.Actor.Type == KnownActorTypes.Console;
            bool isNegative = amount < 0;

            var target = await Context.Parameters.GetAsync<string>(0);
            var targetPlayer =
                await userManager.FindUserAsync(KnownActorTypes.Player, target, UserSearchMode.FindByNameOrId);

            if (targetPlayer == null)
                throw new UserFriendlyException(stringLocalizer["economy:fail:user_not_found", new { target }]);

            var selfPermission = false;
            if (targetPlayer.Id.Equals(Context.Actor.Id))
            {
                selfPermission = await CheckPermissionAsync(PayToSelf) == PermissionGrantResult.Grant;
                if (selfPermission)
                    throw new UserFriendlyException(stringLocalizer["economy:fail:self_pay"]);
            }

            if (isNegative && !selfPermission && !isConsole || amount == 0)
                throw new UserFriendlyException(stringLocalizer["economy:fail:invalid_amount",
                    new { Amount = amount }]);

            var reason = (string)stringLocalizer["economy:default:payment_reason"];
            if (Context.Parameters.Length > 2)
                reason = Context.Parameters.GetArgumentLine(2);

            var contextActorBalance = (decimal?)null;
            if (!isConsole)
                contextActorBalance =
                    await economyProvider.UpdateBalanceAsync(Context.Actor.Id, Context.Actor.Type, -amount, reason);

            var targetBalance =
                await economyProvider.UpdateBalanceAsync(targetPlayer.Id, targetPlayer.Type, amount, reason);
            var printToCaller = stringLocalizer[
                contextActorBalance.HasValue ? "economy:success:pay_player" : "economy:success:pay_console",
                new
                {
                    Amount = amount,
                    Balance = contextActorBalance ?? targetBalance,
                    economyProvider.CurrencyName,
                    economyProvider.CurrencySymbol,
                    targetPlayer.DisplayName
                }];
            await PrintAsync(printToCaller);

            amount = Math.Abs(amount);
            var printToTarget = stringLocalizer[
                isNegative ? "economy:success:payed_negative" : "economy:success:payed",
                new
                {
                    Amount = amount,
                    Balance = targetBalance,
                    economyProvider.CurrencyName,
                    economyProvider.CurrencySymbol,
                    Context.Actor.DisplayName
                }];
            await targetPlayer.PrintMessageAsync(printToTarget);
        }
    }
}
