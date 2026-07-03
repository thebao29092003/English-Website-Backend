namespace whOperation.API.APIPayload
{
    /// <summary>
    /// Base class for API responses.
    /// </summary>
    public class APIResponseBase
    {

        /// <summary>
        /// Gets or sets the status of the response.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the message of the response.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the response identifier.
        /// </summary>
        public string ResponseId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the endpoint code.
        /// </summary>
        public string EndPointCode { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether the request was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the value of the response.
        /// </summary>
        public object? Value { get; set; }
        /// <summary>
        /// Gets or sets the value of the error
        /// </summary>
    }
}
