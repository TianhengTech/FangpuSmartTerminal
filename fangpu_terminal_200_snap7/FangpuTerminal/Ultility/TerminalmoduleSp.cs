using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace fangpu_terminal.Ultility
{
    public partial class FangpuTerminal : Form
    {
        String[] ProcessList=new String[] {"脱模机","刷油机","预热位1","预热位2","预热位3","浸料位",
            "空位","烤料位1","烤料位2","水箱位"};
        String No1Pos=null,No2Pos=null,No3Pos=null,No4Pos=null,No5Pos=null;
       public void ModuleID(Dictionary<string,int> data)
       {
           if(IsBitTrue(data["M0"],0))//自动状态
           {
               if(IsBitTrue(data["I5"],5))
               {
                   No1Pos=ProcessList[0];
               }
           }

       }
        /// <summary>
        /// 判断字节第N位是否为1,这里使用字节数据,N>7将直接返回FALSE
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dig"></param>
        /// <returns></returns>
        bool IsBitTrue(int data,int dig)
        {
            if(dig>7)
                return false;
            else
                return (data & 1<<dig)==(1<<dig);
        }
    }
}
