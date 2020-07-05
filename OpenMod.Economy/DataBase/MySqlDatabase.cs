﻿#region

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using MySql.Data.MySqlClient;
using OpenMod.Core.Commands;
using OpenMod.Economy.API;
using MySqlHelper = OpenMod.Database.Helper.MySqlHelper;

#endregion

namespace OpenMod.Economy.DataBase
{
    internal sealed class MySqlDatabase : MySqlHelper, IEconomyInternalDatabase
    {
        private readonly decimal m_DefaultBalance;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly string m_TableName;

        public MySqlDatabase(IConfiguration configuration, decimal defaultBalance, IStringLocalizer stringLocalizer,
            string tableName) : base(configuration["MySql_Connection_String"])
        {
            m_DefaultBalance = defaultBalance;
            m_StringLocalizer = stringLocalizer;
            m_TableName = tableName;
        }

        public Task<bool> CreateUserAccountAsync(string userId, string userType)
        {
            var uniqueId = $"{userType}_{userId}";
            return CreateUserAccountInternalAsync(uniqueId);
        }

        public Task<decimal> GetBalanceAsync(string userId, string userType)
        {
            var uniqueId = $"{userType}_{userId}";
            return GetBalanceInternalAsync(uniqueId);
        }

        public Task<decimal> IncreaseBalanceAsync(string userId, string userType, decimal amount)
        {
            var uniqueId = $"{userType}_{userId}";
            return IncreaseBalanceInternalAsync(uniqueId, amount);
        }

        public Task<decimal> DecreaseBalanceAsync(string userId, string userType, decimal amount,
            bool allowNegativeBalance)
        {
            var uniqueId = $"{userType}_{userId}";
            return DecreaseBalanceInternalAsync(uniqueId, amount, allowNegativeBalance);
        }

        internal Task CheckShemasAsync()
        {
            return ExecuteMySqlContextAsync(async command =>
            {
                command.CommandText = $"SHOW TABLES LIKE '{m_TableName}';";
                if (await command.ExecuteScalarAsync() != null)
                    return;

                command.CommandText = $"CREATE TABLE `{m_TableName}` (" +
                                      "`UniqueId` VARCHAR(25), " + //The size of 25 can be small, it work for unturned player_STEAMID or discord_STEAMID 
                                      "`Balance` DECIMAL NOT NULL, " +
                                      "PRIMARY KEY(`UniqueId`)) " +
                                      "COLLATE='utf32_general_ci';";

                await command.ExecuteNonQueryAsync();
            });
        }

        private Task<bool> CreateUserAccountInternalAsync(string uniqueId)
        {
            return ExecuteMySqlContextAsync(async command =>
            {
                command.Parameters.Add("@uniqueId", MySqlDbType.VarChar).Value = uniqueId;
                command.Parameters.Add("@defaultBalance", MySqlDbType.Decimal).Value = m_DefaultBalance;
                command.CommandText = $"INSERT IGNORE INTO `{m_TableName}`" +
                                      "(`UniqueId`, `Balance`) VALUES " +
                                      "(@uniqueId, @defaultBalance);";

                return await command.ExecuteNonQueryAsync() > 0;
            });
        }

        private Task<decimal> GetBalanceInternalAsync(string uniqueId)
        {
            return ExecuteMySqlContextAsync(async command =>
            {
                command.Parameters.Add("@uniqueId", MySqlDbType.VarChar).Value = uniqueId;
                command.CommandText = "SELECT `Balance` " +
                                      $"FROM `{m_TableName}` " +
                                      "WHERE `UniqueId`=@uniqueId;";

                if (await command.ExecuteScalarAsync() is decimal balance) return balance;

                await CreateUserAccountInternalAsync(uniqueId);
                return m_DefaultBalance;
            });
        }

        private Task<decimal> IncreaseBalanceInternalAsync(string uniqueId, decimal amount)
        {
            return ExecuteMySqlContextAsync(async command =>
            {
                command.Parameters.Add("@uniqueId", MySqlDbType.VarChar).Value = uniqueId;
                command.Parameters.Add("@amount", MySqlDbType.Decimal).Value = amount;
                command.CommandText = $"UPDATE `{m_TableName}` " +
                                      "SET `Balance`=`Balance` + @amount " +
                                      "WHERE `UniqueId` = @uniqueId;";

                if (await command.ExecuteNonQueryAsync() > 0) return await GetBalanceInternalAsync(uniqueId);

                await CreateUserAccountInternalAsync(uniqueId);
                return await IncreaseBalanceInternalAsync(uniqueId, amount);
            });
        }

        public Task<decimal> DecreaseBalanceInternalAsync(string uniqueId, decimal amount, bool allowNegativeBalance)
        {
            return ExecuteMySqlContextAsync(async command =>
            {
                command.Parameters.Add("@uniqueId", MySqlDbType.VarChar).Value = uniqueId;
                command.Parameters.Add("@amount", MySqlDbType.Decimal).Value = amount;
                command.CommandText = $"UPDATE `{m_TableName}` " +
                                      "SET `Balance` = `Balance` - @amount " +
                                      "WHERE `UniqueId` = @uniqueId;";

                if (await command.ExecuteNonQueryAsync() >= 1)
                {
                    var balance = await GetBalanceInternalAsync(uniqueId);
                    if (allowNegativeBalance || balance >= 0) return balance;

                    balance = await IncreaseBalanceInternalAsync(uniqueId, amount);
                    throw new UserFriendlyException(m_StringLocalizer["uconomy:fail:not_enough_balance", balance]);
                }

                await CreateUserAccountInternalAsync(uniqueId);
                return await DecreaseBalanceInternalAsync(uniqueId, amount, allowNegativeBalance);
            });
        }
    }
}