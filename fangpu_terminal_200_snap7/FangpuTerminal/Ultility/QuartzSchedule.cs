using System;
using System.Diagnostics;
using Quartz;
using Quartz.Impl;
using fangpu_terminal.Ultility.Nhibernate;

namespace fangpu_terminal.Ultility
{
    /// <summary>
    ///创建一个月内每天的表格
    /// </summary>
    public class MySqlTableUpdate : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                using (var entity = FluentNhibernateHelper.GetSession())
                {
                    string nextmonth = DateTime.Today.AddMonths(1).ToString("yyyyMM");
                    int days = DateTime.DaysInMonth(Convert.ToInt16(nextmonth.Substring(0, 4)),
                        Convert.ToInt16(nextmonth.Substring(4, 2)));
                    for (int i = 1; i <= days; i++)
                    {
                        string tablename = "historydata_" + nextmonth + i.ToString().PadLeft(2, '0');
                        string sqlstr = "CREATE TABLE IF NOT EXISTS " + tablename + " LIKE historydata";
                        var x = entity.Connection.CreateCommand();
                        x.CommandText = sqlstr;
                        x.ExecuteNonQuery();
                    }
                    TerminalLogWriter.WriteInfoLog(typeof (MySqlTableUpdate), "作业建立" + DateTime.Now);
                }
            }
            catch
            {
                TerminalLogWriter.WriteErroLog(typeof(MySqlTableUpdate), "创建表格出错");
  
            }

        }
    }
    /// <summary>
    /// 执行同步
    /// </summary>
    public class DataSync : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            //FangpuTerminal.DataAutoSync();
        }
    }
    /// <summary>
    /// 设置启动定时Quartz
    /// </summary>
    public class QuartzSchedule
    {
        IScheduler sche;
        public void StartSchedule()
        {
            ISchedulerFactory sf = new StdSchedulerFactory();//执行者  
            sche= sf.GetScheduler();
            IJobDetail job1 = JobBuilder.Create<MySqlTableUpdate>()  //创建一个作业
             .WithIdentity("job1", "group1")
             .Build();
            ITrigger trigger1 = TriggerBuilder.Create()
                                       .WithIdentity("trigger1", "gruop1")
                                       .StartNow()
                                       .WithCronSchedule("0 0 0 ? * *")//每天执行一次
                                       .Build();
            IJobDetail job2 = JobBuilder.Create<DataSync>()  //创建一个作业
            .WithIdentity("job2", "group1")
            .Build();
            ITrigger trigger2 = TriggerBuilder.Create()
                                       .WithIdentity("trigger2", "gruop1")
                                       .StartNow()
                                       .WithCronSchedule("0 0 * ? * *")//每小时执行一次
                                       .Build();
            sche.ScheduleJob(job1, trigger1);
            sche.ScheduleJob(job2, trigger2);
            sche.Start();
            
        }

        public void Dispose()
        {
            if (sche!=null&&!sche.IsShutdown)
            {
                sche.Clear();
                sche.Shutdown();                                           
            }
        }
    }
}
