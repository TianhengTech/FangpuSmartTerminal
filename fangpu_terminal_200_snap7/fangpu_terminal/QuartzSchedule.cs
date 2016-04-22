using Quartz;
using Quartz.Impl;
using System;
using System.Diagnostics;
namespace fangpu_terminal
{
    public class HelloJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var entity = new FangpuDatacenterModelEntities();
            string nextmonth = DateTime.Today.AddMonths(1).ToString("yyyyMM");
            int days = DateTime.DaysInMonth(Convert.ToInt16(nextmonth.Substring(0, 4)), Convert.ToInt16(nextmonth.Substring(4, 2)));
            for (int i = 1; i <= days; i++)
            {
                string tablename = "historydata_" + nextmonth + i.ToString().PadLeft(2, '0');
                string sqlstr = "CREATE TABLE IF NOT EXISTS " + tablename + " LIKE historydata";
                int x = entity.Database.ExecuteSqlCommand(sqlstr); 
            }                    
            Trace.WriteLine("作业执行!"+DateTime.Now.ToString());
        }
    }
    class QuartzSchedule
    {
        public static void StartSchedule()
        {
            ISchedulerFactory sf = new StdSchedulerFactory();//执行者  
            IScheduler sche= sf.GetScheduler();
            sche.Start();

            IJobDetail job1 = JobBuilder.Create<HelloJob>()  //创建一个作业
             .WithIdentity("job1", "group1")
             .Build();
            ITrigger trigger1 = TriggerBuilder.Create()
                                       .WithIdentity("trigger1", "grop1")
                                       .StartNow()
                                       .WithCronSchedule("0 0 0 ? * *")
                                       .Build();
            sche.ScheduleJob(job1, trigger1);  
        }
    }
}
