using Autofac;
using OpenMod.API.Plugins;
using OpenMod.SqlSugarCore;
using OpenMod.SqlSugarCore.Extensions;
using SqlSugar;

namespace OpenMod.Economy
{
    public class ContainerConfigurator : IPluginContainerConfigurator
    {
        public void ConfigureContainer(IPluginServiceConfigurationContext context)
        {
            MappingTableList mappingTables = new MappingTableList();
            context.ContainerBuilder.AddSqlSugarClient(mappingTables);
            context.ContainerBuilder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerDependency();
        }
    }
}
