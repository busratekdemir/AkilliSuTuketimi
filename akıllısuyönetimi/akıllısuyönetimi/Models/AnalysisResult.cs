public class AnalysisResult
{
    public List<double> WeeklyPredictions { get; set; }
    public List<AnomalyDetail> Anomalies { get; set; }
}

public class AnomalyDetail
{
    public string Code { get; set; }
    public string Type { get; set; }
    public string Location { get; set; }
    public string Time { get; set; }
}