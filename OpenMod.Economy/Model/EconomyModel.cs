using SqlSugar;

namespace OpenMod.Economy.Model
{
    public class EconomyModel
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int PlayerId { get; set; }
        public decimal Balance { get; set; }

        public EconomyModel() { }

        public EconomyModel(int PlayerId, decimal Balance) {
            this.PlayerId = PlayerId;
            this.Balance = Balance;
        }
    }
}
