namespace PayrollLoad.ModelsDAL
{
    public class LoadedFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public byte[] LoadFile { get; set; }
        public Payroll Payroll { get; set; }
        public int PayrollId { get; set; }
    }
}
