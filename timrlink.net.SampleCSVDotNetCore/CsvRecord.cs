namespace timrlink.net.SampleCSVDotNetCore
{
    class CsvRecord
    {
        public string User { get; set; }
        public string Task { get; set; }
        public string StartDateTime { get; set; }
        public string EndDateTime { get; set; }
        public string Break { get; set; }
        public string Notes { get; set; }
        public bool Billable { get; set; }

        public override string ToString()
        {
            return $"Record(User={User}, Task={Task}, StartDateTime={StartDateTime}, EndDateTime={EndDateTime}, Break={Break}, Notes={Notes}, Billable={Billable}";
        }
    }
}
