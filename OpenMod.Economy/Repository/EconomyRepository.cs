using JetBrains.Annotations;
using OpenMod.API.Ioc;
using OpenMod.Economy.Model;
using OpenMod.SqlSugarCore;
using SqlSugar;
using System.Threading.Tasks;

namespace OpenMod.Economy
{
    [PluginServiceImplementation]
    [UsedImplicitly]
    public class EconomyRepository : Repository<EconomyModel>, IEconomyRepository
    {
        public EconomyRepository(ISqlSugarClient sqlSugarClient) : base(sqlSugarClient)
        {
        }
        public async Task<decimal> GetBalanceAsync(int PlayerId)
        {
            return await Queryable().Select(it => it.Balance).InSingleAsync(PlayerId);
        }
        public async Task SetBalanceAsync(int PlayerId, decimal Balance)
        {
            await Updateable().SetColumns(it => it.Balance == Balance).Where(it => it.PlayerId == PlayerId).ExecuteCommandAsync();
        }
        public async Task<decimal> UpdateBalanceAsync(int PlayerId, decimal Balance)
        {
            await Updateable().SetColumns(it => it.Balance == it.Balance + Balance).Where(it => it.PlayerId == PlayerId).ExecuteCommandAsync();
            return await Queryable().Select(it => it.Balance).InSingleAsync(PlayerId);
        }
    }
}
