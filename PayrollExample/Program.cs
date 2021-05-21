using PayrollLoad.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PayrollExample
{
    class Program
    {
        static string path = @"d:\PayrollFiles\";
        static void Main(string[] args)
        {
            //LoadExample();
            FiltredOnDateExample();
            //DownloadFileFromDBExample();
        }

        private static void LoadExample()
        {
            PayrollService ps = new PayrollService();
            
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            Console.WriteLine(dirInfo.GetFiles().Length);
            foreach (var fileInfo in dirInfo.GetFiles())
            {
                ps.AddPayrolsSaveToDB(fileInfo);
            }
        }
        private static void FiltredOnDateExample()
        {
            DateTime startDate = DateTime.Parse("01-01-2020");
            DateTime endDate = DateTime.Parse("31-12-2020");

            PayrollService ps = new PayrollService();
            var payrols = ps.FiltredOnPeriodPayrolsAsync(startDate, endDate).Result;
            Console.WriteLine($"Период \t\t Вычтено \t\t Выплачено \t Кол-во платежей");
            foreach (var payroll in payrols)
            {
                Console.WriteLine($"{payroll.Period.ToString("dd.MM.yyyy")}\t{payroll.Withheld}\t\t{payroll.Paid}\t\t{payroll.Payments.Count}");
            }
        }

        private static void DownloadFileFromDBExample()
        {
            PayrollService ps = new PayrollService();
            var file = ps.GetFileByPayrollId("1");
            using (FileStream fs = new FileStream(@$"{path}\Download_{file.FileName}", FileMode.CreateNew, FileAccess.Write))
            {
                fs.Write(file.LoadFile, 0, file.LoadFile.Length);
            };
        }
    }
}
