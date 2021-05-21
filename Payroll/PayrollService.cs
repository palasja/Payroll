using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Data;
using PayrollLoad.ModelsDAL;
using PayrollLoad.Reader;
using System.Text;
using ExcelDataReader;

namespace PayrollLoad.Services
{
    public class PayrollService
    {
        public void AddPayrolsSaveToDB(FileInfo fileInfo)
        {
            List<Payroll> payrolls = new List<Payroll>();
            var paymentTypes = getTypesPayment();
            PayrollReader payrollReader = new PayrollReader(paymentTypes);
            DataTable dt;
            LoadedFile loadedFile = new LoadedFile()
            {
                FileName = fileInfo.Name
            };
            using (FileStream fstream = fileInfo.OpenRead())
            {
                loadedFile.LoadFile = new byte[fstream.Length];
                fstream.Read(loadedFile.LoadFile, 0, (int)fstream.Length);
                 dt = GetTableFromFileStream(fstream, fileInfo.Name.Split('.').Last());
            };

                Payroll Payroll = payrollReader.ReadPayroll(dt);

                Payroll.File = loadedFile;
                payrolls.Add(Payroll);

            using (PayrollContext context = new PayrollContext())
            {
                context.Payrolls.AddRange(payrolls);
                context.SaveChanges();
            }
        }
        
        public async Task<List<Payroll>> FiltredOnPeriodPayrolsAsync(DateTime start, DateTime end)
        {
            List<Payroll> payrols;

            using (PayrollContext context = new PayrollContext())
            {
                payrols = await context.Payrolls.OrderByDescending(p => p.Period).Include(p => p.Payments).Where(p => p.Period >= start && p.Period <= end).AsNoTracking().ToListAsync();
            }

            return payrols;
        }
       
        public LoadedFile GetFileByPayrollId(string payrollId)
        {
            LoadedFile loadedFile;
            using (PayrollContext context = new PayrollContext())
            {
                loadedFile = context.LoadedFile.Where(lf => lf.PayrollId == int.Parse(payrollId)).AsNoTracking().FirstOrDefault();
            }

            return loadedFile;
        }
        /// <summary>
        /// Получить DataTable из FileStream
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private DataTable GetTableFromFileStream(FileStream fs, string type)
        {
            //Установить кодировку таблицы
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            IExcelDataReader excelReader;
            if (type.ToUpper() == "XLS")
            {
                //1.1 Reading from a binary Excel file ('97-2003 format; *.xls)
                excelReader = ExcelReaderFactory.CreateReader(fs);
            }
            else
            {
                //1.2 Reading from a OpenXml Excel file (2007 format; *.xlsx)
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
            }
            //2. DataSet - The result of each spreadsheet will be created in the result.Tables
            DataSet result = excelReader.AsDataSet();
            DataTable dt = result.Tables[0];
            return dt;
        }
        /// <summary>
        /// Получить справочник с типами платежей
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, string> getTypesPayment()
        {
            Dictionary<int, string> types = new Dictionary<int, string>();
            using (PayrollContext context = new PayrollContext())
            {
                types = context.PaymentType.AsNoTracking().ToDictionary(pt => pt.Id, pt => pt.TypeName);
            }
            return types;
        }
    }
}
