using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping;
using NHibernate.Tool.hbm2ddl;

namespace fangpu_terminal.Ultility.Nhibernate
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
                        c => c.Server(Properties.TerminalParameters.Default.terminal_server_ip)
                            .Password("tianheng123")
                            .Username("root")
                            .Database("fangpu_datacenter")
                    ) //是否显示sql
                //.ShowSql()
                )
                //映射程序集

                .Mappings(
                    m => m.FluentMappings
                        .AddFromAssembly(System.Reflection.Assembly.Load("fangpu_terminal")));
        }
        public static FluentConfiguration GetLiteSessionConfig()
        {
            return FluentNHibernate.Cfg.Fluently.Configure()
                //数据库配置
                .Database(FluentNHibernate.Cfg.Db.SQLiteConfiguration.Standard
                .UsingFile(Properties.TerminalParameters.Default.localdatapath)
                )
                //映射程序集
                .Mappings(
                    m => m.FluentMappings.AddFromAssembly(System.Reflection.Assembly.Load("fangpu_terminal")))
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
