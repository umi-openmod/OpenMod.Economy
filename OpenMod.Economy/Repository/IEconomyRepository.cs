using OpenMod.API.Ioc;
using OpenMod.Economy.Model;
using OpenMod.SqlSugarCore;
using System.Threading.Tasks;

namespace OpenMod.Economy
{
    [Service]
    public interface IEconomyRepository : IRepository<EconomyModel>
    {
        Task<decimal> GetBalanceAsync(int PlayerId);
        Task<decimal> UpdateBalanceAsync(int PlayerId, decimal Balance);
        Task SetBalanceAsync(int PlayerId, decimal Balance);
    }
}
