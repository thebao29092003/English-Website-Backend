namespace English.Website.Api.Extensions.Helpers
{
    public static class MessageConstants
    {
        public static string GetInsertMessage(bool isSuccess, string model)
        {
            return $"Insert {model} {(isSuccess ? "succeeded" : "failed")}.";
        }

        public static string GetUpdateMessage(bool isSuccess, string model)
        {
            return $"Update {model} {(isSuccess ? "succeeded" : "failed")}.";
        }

        public static string GetDeleteMessage(bool isSuccess, string model)
        {
            return $"Delete {model} {(isSuccess ? "succeeded" : "failed")}.";
        }

        public static string GetDataMessage(bool isSuccess, string model)
        {
            return $"Get {model} {(isSuccess ? "succeeded" : "failed")}.";
        }

        public static string GetFoundMessage(bool isSuccess, string model)
        {
            return isSuccess
                ? $"{model} found."
                : $"{model} not found.";
        }

        public static string GetExistMessage(bool isSuccess, string model)
        {
            return isSuccess
                ? $"{model} exist."
                : $"{model} not exist.";
        }
    }
}
