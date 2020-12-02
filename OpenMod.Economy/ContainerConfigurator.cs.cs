using Autofac;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Plugins;
using OpenMod.Economy.Model;
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
            mappingTables.Add(nameof(EconomyModel), context.Configuration.GetSection("Database").GetSection("EconomyTable").Get<string>());
            context.ContainerBuilder.AddSqlSugarClient(mappingTables);
            context.ContainerBuilder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerDependency().OwnedByLifetimeScope();
            context.ContainerBuilder.RegisterType<EconomyRepository>().As<IEconomyRepository>().InstancePerDependency().OwnedByLifetimeScope();
        }
    }
}
