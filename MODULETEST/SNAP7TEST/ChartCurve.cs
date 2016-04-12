using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;

namespace SNAP7TEST
{
    class ChartCurve
    {
        public static void ChartCurveUpdate(DataTable CurveData,Mainform form)
        {
            form.Chart_temp.Series["温度"].Points.DataBind(CurveData.AsEnumerable(),"时间","温度","");
        }
    }
}
