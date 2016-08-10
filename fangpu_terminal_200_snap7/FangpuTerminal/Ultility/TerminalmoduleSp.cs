using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace fangpu_terminal
{


    
    public partial class FangpuTerminal
    {
        System.Object poslocker = new System.Object();
        String[] process_seq=new String[10] {"脱模机","刷油机","烤模炉1","烤模炉2","烤模炉3","浸料位",
            "空位","烤料炉1","烤料炉2","水箱"};
        int[] NointPos = new int[6];
        string[] NostringPos= new string[6];
        /// <summary>
        /// 模具ID判定
        /// </summary>
        /// <param name="data">模具位置状态寄存器值</param>
        /// <param name="no1pos">模具1的位置</param>
        /// <returns>6组模具对应的位置编号</returns>
       public void ModuleID(PlcDAQCommunicationObject data)
       {
           int VD80 = data.aream_data["VB83"] + (data.aream_data["VB82"] << 8) + (data.aream_data["VB81"] << 16) + (data.aream_data["VB80"] << 24);
           if (NumOf1inBit(VD80) != 6)
           {
               typelabel.Text = Convert.ToString(VD80, 2).PadLeft(32,'0');
               return;
               //throw new Exception("非6");
           }
           int no1pos=data.aream_data["VB84"];
           lock(poslocker)
           {
               no1pos--;
               NointPos[0] = no1pos;
               if (IsOdd(no1pos))
               {
                   NostringPos[0] = process_seq[no1pos / 2] + "-" + process_seq[(no1pos / 2 + 1) % 10];
               }
               else
               {
                   NostringPos[0] = process_seq[no1pos / 2];
               }
               int pos=no1pos+1;
               int counter=1;
               int max = 0;
               while(counter<=5 && max<19) //剩下的五个模
               {
                    if(IsBitTrue(VD80,pos))//如果为1
                    {
                        NointPos[counter] = pos;   //counter 1-5   
                        if(IsOdd(pos))
                        {
                            NostringPos[counter]=process_seq[pos/2]+"-"+process_seq[(pos/2+1)%10];
                        }
                        else
                        {
                            NostringPos[counter]=process_seq[pos/2];
                        }
                        counter++;
                    }
                    pos = (pos + 1) % 20; 
                    max++;
               }
               return;   //pos 0 -19      
           }

       }
        void SetPosColor()
       {
               List<int> modpos = new List<int>();
               foreach (int n in NointPos)
               {
                   positioner[(n / 2)].Font = new System.Drawing.Font("华文琥珀", 10.5F);
                   if (IsOdd(n))
                   {
                       positioner[(n / 2)].ForeColor = Color.Orange;
                   }
                   else
                   {
                       positioner[(n / 2)].ForeColor = Color.Red;
                   }
                   modpos.Add(n / 2);
               }
           for (int i = 0; i < positioner.Count(); i++)
           {
               if (!modpos.Contains(i))
               {
                   positioner[i].Font = new System.Drawing.Font("黑体", 10.5F);
                   positioner[i].ForeColor = Color.Black;
               }

           }
            
       }
        /// <summary>
        /// 判断字节第N位是否为1,这里使用字节数据,N>7将直接返回FALSE
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dig"></param>
        /// <returns></returns>
        bool IsBitTrue(int data,int bit)
        {
                return (data & (1<<bit))==(1<<bit);
        }
        int NumOf1inBit(int data)
        {
            int count = 0;
            for(int i =0 ;i<20;i++)
            {
                if ((data & (1 << i)) == (1 << i))
                    count++;
            }
            return count;
        }
        bool IsOdd(int n)
        {
            return Convert.ToBoolean(n & 1);
        }
    }

}