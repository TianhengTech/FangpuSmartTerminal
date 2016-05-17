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

    }
}