using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping;

namespace fangpu_terminal.Ultility.Nhibernate
{
    public class NhibernateHelper
    {
        private ISessionFactory _sessionFactory;

        public NhibernateHelper()
        {
            //创建ISessionFactory
            _sessionFactory = GetSessionFactory();
        }

        /// <summary>
        /// 创建ISessionFactory
        /// </summary>
        /// <returns></returns>
        public ISessionFactory GetSessionFactory()
        {

            //配置ISessionFactory
            return (new Configuration()).Configure().BuildSessionFactory();
        }

        public Configuration GetSessionConfig()
        {
            Configuration cfg = new Configuration().Configure();
            return cfg;
        }
        /// <summary>
        /// 更换表名
        /// </summary>
        /// <param name="_cfg"></param>
        /// <param name="t"></param>
        /// <param name="newtablename"></param>
        public void MappingTablenames(Configuration _cfg,Type t,string newtablename="")
        {
            if (newtablename == "")
            {
                return;  
            }
            foreach (PersistentClass persistClass in _cfg.ClassMappings)
            {
                if (persistClass.MappedClass == t)
                    persistClass.Table.Name = newtablename;
            }
        }

        /// <summary>
        /// 打开ISession
        /// </summary>
        /// <returns></returns>
        public ISession GetSession()
        {
            return _sessionFactory.OpenSession();
        }

        public void test()
        {
            Configuration cfg = new Configuration().Configure();
            Mappings mappings = cfg.CreateMappings(null);
            foreach (PersistentClass persistClass in cfg.ClassMappings)
            {
                if(persistClass.MappedClass == typeof(historydata_hiber))
                persistClass.Table.Name = "historydata_20160426";
            }
            var sessionFactory = cfg.BuildSessionFactory();
            warn_info wa = new warn_info();
            wa.device_name = "dynamic";
            wa.storetime = DateTime.Now;
            wa.warn_message = "dynamic test";
            historydata_hiber his =new historydata_hiber
            {
                deviceid="DXX",value="te",storetime=DateTime.Now
            };
            ISession session = sessionFactory.OpenSession();
            session.Save(his);
            session.Save(wa);
            
            session.Flush();

        }

        public void test2()
        {
            Configuration cfg = new Configuration().Configure();
            Mappings mappings = cfg.CreateMappings(null);
            ISession session = cfg.BuildSessionFactory().OpenSession();
            warn_info wa = new warn_info();
            wa.warn_message = "fluent";
            wa.storetime = DateTime.Now;
            wa.device_name = "test fluent";
            using (var trans = session.BeginTransaction())
            {
                try
                {
                    session.Save(wa);
                    session.Flush();
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                }
            }
        }


    }
}