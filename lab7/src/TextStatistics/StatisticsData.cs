namespace TextStatistics
{
    public class StatisticsData
    {
        public int TextNum { get; set; }
        public int HighRankPart { get; set; }
        public double AvgRank { get; set; }
    }

    public class StatisticsEventData
    {
        public string TextId { get; set; }
        public double Rank { get; set; }
    }
}