namespace English.Website.Api.Dtos.CloudinaryDtos
{
    public class UploadResponseDto
    {
        public string? PublicId { get; set; }
        public string? SecureUrl { get; set; }
        public string? DisplayName { get; set; }
        public string? AssetFolder { get; set; }
        public string Format { get; set; } = string.Empty;
        public long Bytes { get; set; }
    }
}
