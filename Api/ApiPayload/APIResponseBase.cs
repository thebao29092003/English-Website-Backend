namespace whOperation.API.APIPayload
{
    /// <summary>
    /// Base class for API responses.
    /// </summary>
    public class APIResponseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="APIResponseBase"/> class.
        /// </summary>
        public APIResponseBase() { }

        /// <summary>
        /// Gets or sets the status of the response.
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// Gets or sets the message of the response.
        /// </summary>
        public object message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a response result.
        /// </summary>
        public bool isResponseResult { get; set; }

        /// <summary>
        /// Gets or sets the response identifier.
        /// </summary>
        public string responseId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the endpoint code.
        /// </summary>
        public string endPointCode { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether the request was successful.
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// Gets or sets the value of the response.
        /// </summary>
        public object? value { get; set; }
        /// <summary>
        /// Gets or sets the value of the error
        /// </summary>
    }
}
