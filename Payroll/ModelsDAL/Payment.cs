using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PayrollLoad.ModelsDAL
{
    public class Payment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Period { get; set; }
        public string SP { get; set; }
        public int Days { get; set; }
        public int Hours { get; set; }
        public double Summ { get; set; }
        public PaymentType PaymentType { get; set; }
        [Required(ErrorMessage = "Не существующий тип платежа")]
        public int PaymentTypeId { get; set; }
        public Payroll Payroll { get; set; }
        public int PayrollId { get; set; }

    }
}
