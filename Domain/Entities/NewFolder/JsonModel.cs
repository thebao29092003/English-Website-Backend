namespace English.Website.Domain.Entities.NewFolder
{

    // Chi tiết phát âm từng từ từ Azure
    //public class WordDetail
    //{
    //    public string Word { get; set; } = string.Empty;
    //    public decimal AccuracyScore { get; set; }
    //    public string ErrorType { get; set; } = "None"; // None, Omission, Mispronunciation, Insertion
    //    public int Offset { get; set; } // Mili-giây bắt đầu
    //    public int Duration { get; set; } // Thời lượng phát âm (mili-giây)
    //}

    // Nhận xét chi tiết ngữ pháp và từ vựng từ deepseek
    public class AiFeedback
    {
        public List<GrammarError> GrammarErrors { get; set; } = new();
        public List<VocabSuggestion> VocabSuggestions { get; set; } = new();
    }

    public class GrammarError
    {
        public string Original { get; set; } = string.Empty;
        public string Corrected { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }

    public class VocabSuggestion
    {
        public string OriginalWord { get; set; } = string.Empty;
        public string SuggestedAlternative { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }

    // Các phương án viết lại câu hay hơn từ Gemini
    public class RephrasedResponse
    {
        public string ImprovedText { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty; // Professional, Natural, v.v.
        public string Explanation { get; set; } = string.Empty;
    }
}
