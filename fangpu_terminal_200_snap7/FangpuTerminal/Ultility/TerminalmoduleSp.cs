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
        String[] process_seq=new String[] {"脱模机","刷油机","预热位1","预热位2","预热位3","浸料位",
            "空位","烤料位1","烤料位2","水箱位"};
        int[] NointPos = new int[6];
        String[] NoPos = new string[6];
        
       public String[] ModuleID(int data,int no1pos)
       {
           if(NumOf1inBit(data)!=6)
               throw new ArgumentOutOfRangeException();
           lock(poslocker)
           {
               if(IsOdd(no1pos))
               {
                   NoPos[0]=process_seq[no1pos-1]+"-"+process_seq[no1pos+1];
               }
               else
               {
                   NoPos[0]=process_seq[no1pos];
               }
               NointPos[0] = no1pos;
               int pos=no1pos-1;
               int counter=0;
               int max = 0;
               while(counter<5) //剩下的五个模
               {
                if(IsBitTrue(data,pos))//如果为1
                   {
                       if(IsOdd(pos))
                       {
                           NoPos[counter]=process_seq[pos-1]+"-"+process_seq[pos+1];
                       }
                       else
                       {
                           NoPos[counter]=process_seq[pos];
                       }
                       NointPos[counter] = pos;
                       pos=(pos-1)%20;
                       counter++;
                   }
                    max++;
                    if (max > 20)
                        break;
               }
               return NoPos;         
           }

       }
        void SetPosColor()
       {
           List<int> modpos=new List<int>();
           foreach (int n in NointPos)
           {
               if (IsOdd(n))
               {
                   positioner[(n / 2)].BackColor = Color.Orange ;
               }
               else
               {
                   positioner[(n / 2)].BackColor = Color.Red;
               }
               modpos.Add(n / 2);
           }
            for(int i=0;i<positioner.Count();i++)
            {
                if (!modpos.Contains(i))
                    positioner[i].BackColor = Color.Black;
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
            if(bit>32)
                return false;
            else
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