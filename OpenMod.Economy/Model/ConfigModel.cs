namespace OpenMod.Economy.Model
{
    public class DataBaseConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public int ConfigId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DbType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string EconomyTable { get; set; }
    }
    public class EconomyConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public string CurrencyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CurrencySymbol { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Default_Balance { get; set; }
    }

    public class RootConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public Database Database { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Economy Economy { get; set; }
    }
}
