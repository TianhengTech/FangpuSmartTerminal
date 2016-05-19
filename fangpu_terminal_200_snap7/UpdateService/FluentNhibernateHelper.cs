using System;
using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Mapping;

namespace UpdateService
{
    public class FluentNhibernateHelper
    {
        private static ISessionFactory _sessionFactory;
        private static ISession _session;
        private static object _objLock = new object();
        private FluentConfiguration cfg;
        public FluentNhibernateHelper()
        {

        }

        public static FluentConfiguration GetSessionConfig()
        {
            return FluentNHibernate.Cfg.Fluently.Configure()
                //数据库配置
                .Database(FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
                    //连接字符串
                    .ConnectionString(
                        c => c.Server(Mysettings.Default.terminal_server_ip)
                            .Password("tianheng123")
                            .Username(Mysettings.Default.terminal_server_id)
                            .Database(Mysettings.Default.terminal_server_db)                          
                    ) //是否显示sql
                //.ShowSql()
                )
                //映射程序集

                .Mappings(
                    m => m.FluentMappings
                        .AddFromAssembly(System.Reflection.Assembly.Load("UpdateService")));
        }
        public static FluentConfiguration GetLiteSessionConfig()
        {
            return FluentNHibernate.Cfg.Fluently.Configure()
                //数据库配置
                .Database(FluentNHibernate.Cfg.Db.SQLiteConfiguration.Standard
                .UsingFile(Mysettings.Default.localdatapath+"/data/datalocal.db")
                )
                //映射程序集
                .Mappings(
                    m => m.FluentMappings.AddFromAssembly(System.Reflection.Assembly.Load("UpdateService")))
                    ;
        }
        /// <summary>
        /// 创建ISessionFactory
        /// </summary>
        /// <returns></returns>
        public static ISessionFactory GetSessionFactory()
        {
            if (_sessionFactory == null)
            {
                lock (_objLock)
                {
                    if (_sessionFactory == null)
                    {
                        //配置ISessionFactory
                        _sessionFactory = GetSessionConfig()
                        .BuildSessionFactory();
                    }
                }
            }
            return _sessionFactory;

        }
        /// <summary>
        /// 重置Session
        /// </summary>
        /// <returns></returns>
        public static ISession ResetSession()
        {
            if (_session.IsOpen)
                _session.Close();
            _session = _sessionFactory.OpenSession();
            return _session;
        }
        /// <summary>
        /// 打开ISession
        /// </summary>
        /// <returns></returns>
        public static ISession GetSession()
        {
            GetSessionFactory();
            if (_session == null)
            {
                lock (_objLock)
                {
                    if (_session == null)
                    {
                        _session = _sessionFactory.OpenSession();
                    }
                }
            }
            return _session;
        }
        public static void MappingTablenames(FluentConfiguration cfg, Type t, string newtablename = "")
        {
            if (newtablename == "")
            {
                return;
            }
            foreach (PersistentClass persistClass in cfg.BuildConfiguration().ClassMappings)
            {
                if (persistClass.MappedClass == t)
                    persistClass.Table.Name = newtablename;
            }
        }

    }
}
