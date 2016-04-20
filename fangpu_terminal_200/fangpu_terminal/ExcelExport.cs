using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HPSF;
using System.Data;

namespace fangpu_terminal
{

    class ExcelExport

    {
    /// <summary>
    /// DataTable To MemoryStream
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public static MemoryStream RenderToExcel(DataTable table )
        {

            MemoryStream ms = new MemoryStream();

            using (table)
            {

                IWorkbook workbook = new HSSFWorkbook();
                {
                    SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
                    ISheet sheet = workbook.CreateSheet();
                    {

                        IRow headerRow = sheet.CreateRow(0);
                        

                        // handling header.

                        foreach (DataColumn column in table.Columns)

                            headerRow.CreateCell(column.Ordinal).SetCellValue(column.Caption);//If Caption not set, returns the ColumnName value



                        // handling value.

                        int rowIndex = 1;



                        foreach (DataRow row in table.Rows)
                        {

                            IRow dataRow = sheet.CreateRow(rowIndex);
                            


                            foreach (DataColumn column in table.Columns)
                            {

                                dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());

                            }
                            rowIndex++;
                        }
                        workbook.Write(ms);
                        ms.Flush();
                        ms.Position = 0;
                    }
                }
            }
            return ms;
        }
    /// <summary>
    /// Save Stream to File
    /// </summary>
    /// <param name="ms"></param>
    /// <param name="fileName"></param>
    public static void SaveToFile(MemoryStream ms, string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                byte[] data = ms.ToArray();
                fs.Write(data,0, data.Length);
                fs.Flush();
                data = null;
            }
        }
    }

}
