namespace timrlink.net.SampleCSVDotNetCore
{
    class CsvRecord
    {
        public string Date { get; set; }
        public string User { get; set; }
        public string Task { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Break { get; set; }
        public string Notes { get; set; }
        public bool Billable { get; set; }

        public override string ToString()
        {
            return $"Record(Date={Date}, User={User}, Task={Task}, Start={Start}, End={End}, Break={Break}, Notes={Notes}, Billable={Billable}";
        }
    }
}
