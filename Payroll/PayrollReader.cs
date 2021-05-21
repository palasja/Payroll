using ExcelDataReader;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using PayrollLoad.ModelsDAL;

namespace PayrollLoad.Reader
{
    class PayrollReader
    {
        Dictionary<int, string> paymentTypes;

        public PayrollReader(Dictionary<int, string> paymentTypes)
        {
            this.paymentTypes = paymentTypes;
        }

        public Payroll ReadPayroll(DataTable dt)
        {
            Payroll payroll = ReadPayrollPayment(dt);
            payroll.Period = ReadPeriod(dt.Rows[0][0]);
            payroll.Worker = ReadFullName(dt.Rows[2][0]);

            return payroll;
        }

        public async Task<Payroll> ReadPayrollAsync(DataTable dt)
        {
            Payroll payroll = await Task.Run(() => ReadPayrollPayment(dt));
            payroll.Period = ReadPeriod(dt.Rows[0][0]);
            payroll.Worker = ReadFullName(dt.Rows[2][0]);

            return payroll;
        }


        /// <summary>
        /// Получить дату за которую расчётный лист из строки таблицы
        /// </summary>
        /// <param name="period"></param>
        /// <returns>DateTime из таблице</returns>
        private DateTime ReadPeriod(object period)
        {
            const int NUM_MONTH = 3;
            const int NUM_YEAR = 4;
            var periodArr = period.ToString().Split(" ");
            int month = DateTime.ParseExact(periodArr[NUM_MONTH], "MMMM", new CultureInfo("ru-RU", false)).Month;
            DateTime date = DateTime.Parse($"{month}.{periodArr[NUM_YEAR]}");
            return date;
        }
        /// <summary>
        /// Получить ФИО работника из таблицы
        /// </summary>
        /// <param name="celVal"></param>
        /// <returns></returns>
        private string ReadFullName(object celVal)
        {
            const int NUM_FULLNAME = 1;
            return celVal.ToString().Split(":")[NUM_FULLNAME];
        }
        /// <summary>
        /// Получить данные о платежах в расчётнике
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>Расчётный лист со всеми платежами из xls</returns>
        private Payroll ReadPayrollPayment(DataTable dt)
        {
            Payroll payroll = new Payroll();

           
            /// <value>Маркер конца платежей:
            /// После платежей есть поля "Долг за организацией на начало месяца:" в платежах они не нужны.
            ///  Для определения что дошли до конца платежей используется маркер</value>
            bool hasNextPayment = false;
            int startPaymentInfo = GetStartPaymentInfo(dt);
            Dictionary<string, double> Itog = new Dictionary<string, double>();
            int type = 0;
            int type1 = 0;
            for (int i = startPaymentInfo; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                //В начале платежа 1 строка не пустая, а сумма отсутствует. В итоговых полях с "Всего" сумма тоже может отсутствовать
                if (!String.IsNullOrEmpty(row[0].ToString()) && String.IsNullOrEmpty(row[6].ToString()) && !row[0].ToString().Contains("Всего"))
                {
                    type = paymentTypes.FirstOrDefault(pt => row[0].ToString().ToUpper().Contains(pt.Value.ToUpper())).Key;
                    type1 = paymentTypes.FirstOrDefault(pt => row[8].ToString().ToUpper().Contains(pt.Value.ToUpper())).Key;
                    hasNextPayment = false;
                    continue;
                }
                //Если в первой ячейке есть всего то это итоговая строка платежа
                if (row[0].ToString().Contains("Всего"))
                {
                    if (!String.IsNullOrEmpty(row[6].ToString()))
                    {
                        ItogOnType(payroll, row[0].ToString(), row[6].ToString());
                    }

                    if (!String.IsNullOrEmpty(row[14].ToString()))
                    {
                        ItogOnType(payroll, row[8].ToString(), row[14].ToString());
                    }

                    hasNextPayment = true;
                }
                else
                {
                    if (hasNextPayment) break;
                    if (!String.IsNullOrEmpty(row[0].ToString()))
                    {
                        Payment payment1 = ReadPayment(row.ItemArray, type);
                        payroll.Payments.Add(payment1);
                    }
                    if (!String.IsNullOrEmpty(row[8].ToString()))
                    {
                        Payment payment2 = ReadPayment1(row.ItemArray, type1);
                        payroll.Payments.Add(payment2);
                    }
                }
            }
            return payroll;
        }

        /// <summary>
        /// Получить ячейки из первых платежей строки
        /// </summary>
        /// <param name="cels"></param>
        /// <returns></returns>
        private Payment ReadPayment(object[] cels, int type)
        {
            Payment payment1 = new Payment();
            const int COL_NUM_NAME = 0;
            const int COL_NUM_PERIOD = 1;
            const int COL_NUM_Percent = 3;
            const int COL_NUM_DAYS = 4;
            const int COL_NUM_HOURS = 5;
            const int COL_NUM_SUMM = 6;

            if (!String.IsNullOrEmpty(cels[0].ToString()))
            {
                payment1.Name = cels[COL_NUM_NAME].ToString().Trim().Replace("  ", " ");
                payment1.Period = cels[COL_NUM_PERIOD].ToString().Trim();
                payment1.SP = cels[COL_NUM_Percent].ToString().Trim();
                if (!String.IsNullOrEmpty(cels[COL_NUM_DAYS].ToString())) payment1.Days = Convert.ToInt32(cels[COL_NUM_DAYS].ToString());
                if (!String.IsNullOrEmpty(cels[COL_NUM_HOURS].ToString())) payment1.Hours = Convert.ToInt32(cels[COL_NUM_HOURS].ToString());

                if (!String.IsNullOrEmpty(cels[6].ToString()))
                {
                    payment1.Summ = Convert.ToDouble(cels[COL_NUM_SUMM].ToString());
                }
                payment1.PaymentTypeId = type;
            }
            return payment1;
        }
        /// <summary>
        /// Получить ячейки из вторых платежей строки
        /// </summary>
        /// <param name="cels"></param>
        /// <returns></returns>
        private Payment ReadPayment1(object[] cels, int type)
        {
            Payment payment1 = new Payment();
            const int COL_NUM_NAME = 8;
            const int COL_NUM_PERIOD = 11;
            const int COL_NUM_Percent = 13;
            const int COL_NUM_SUMM = 14;

            if (!String.IsNullOrEmpty(cels[8].ToString()))
            {
                payment1.Name = cels[COL_NUM_NAME].ToString().Trim().Replace("  ", " "); ;
                payment1.Period = cels[COL_NUM_PERIOD].ToString().Trim();
                if (!String.IsNullOrEmpty(cels[COL_NUM_Percent].ToString())) payment1.SP = cels[COL_NUM_Percent].ToString().Trim();

                if (!String.IsNullOrEmpty(cels[COL_NUM_SUMM].ToString()))
                {
                    payment1.Summ = Convert.ToDouble(cels[COL_NUM_SUMM].ToString());
                }
            }
            payment1.PaymentTypeId = type;
            return payment1;
        }
        /// <summary>
        /// Найти шапку таблицы с данными расчётного
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>Номер строки с началом данных</returns>
        private int GetStartPaymentInfo(DataTable dt)
        {
            int NumStringColemnName = 0;
            DataRowCollection rows = dt.Rows;
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i][0].ToString().Contains("Вид"))
                {
                    NumStringColemnName = i + 1;
                    break;
                }
            }
            return NumStringColemnName;
        }
        /// <summary>
        /// Добавить итоговые значение по расчётному. 
        /// </summary>
        /// <param name="payroll"></param>
        /// <param name="strType"></param>
        /// <param name="strSumm"></param>
        // Если брать из базы то не понятно как привязать свойство payroll к названию
        private void ItogOnType(Payroll payroll, string strType, string strSumm)
        {
            double summ = Convert.ToDouble(strSumm);
            if (strType.ToUpper().Contains("НАЧИСЛЕНО")) payroll.Accrued = summ;
            if (strType.ToUpper().Contains("УДЕРЖАНО")) payroll.Withheld = summ;
            if (strType.ToUpper().Contains("НАТУРАЛЬНЫХ ДОХОДОВ")) payroll.Natural = summ;
            if (strType.ToUpper().Contains("ВЫПЛАТ")) payroll.Paid = summ;
        }
    }
}
