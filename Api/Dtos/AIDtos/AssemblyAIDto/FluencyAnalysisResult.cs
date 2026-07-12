using System.Collections.Generic;

namespace English.Website.Api.Dtos.AIDtos.AssemblyAIDto
{
    public class FluencyError
    {
        public string Type { get; set; } = null!;
        public string Message { get; set; } = null!;
        public double? StartTime { get; set; } // start time in seconds
        public double? EndTime { get; set; }   // end time in seconds
        public double? Duration { get; set; }  // duration of pause/issue in seconds
    }

    public class FluencyAnalysisResult
    {
        public double Score { get; set; }
        public List<FluencyError> Errors { get; set; } = new();
    }
}
