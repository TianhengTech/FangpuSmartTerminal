using FluentNHibernate.Mapping;

namespace UpdateService
{
    public class update_infoMapping : ClassMap<update_info>
    {
        public update_infoMapping()
        {
            Table("update_info");
            Id<int>("idupdate_info").GeneratedBy.Identity();
            Map(m => m.device_name).Nullable();
            Map(m => m.ID).Nullable();
            Map(m => m.idupdate_info).Unique();
            Map(m => m.update_message).Nullable();
            Map(m => m.update_version).Nullable();
            Map(m => m.updatetime).Nullable();
        }
    }
    public class terminalcmdMapping : ClassMap<terminalcmd>
    {
        public terminalcmdMapping()
        {
            Table("terminalcmd");
            Id<int>("idterminalcmd").GeneratedBy.Identity();
            Map(m => m.idterminalcmd).Unique();
            Map(m => m.command).Nullable();
            Map(m => m.status).Nullable();
            Map(m => m.device_name).Nullable();
            Map(m => m.time).Nullable();
        }
    }
}
