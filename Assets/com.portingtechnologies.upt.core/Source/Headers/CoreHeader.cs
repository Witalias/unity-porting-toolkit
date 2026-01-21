namespace UPT.Services
{
    public class UptResult
    {
        public bool IsSuccess => ErrorCode == ErrorCode.Success;
        public ErrorCode ErrorCode { get; }
        public string InnerMessage { get; }

        public UptResult(ErrorCode error)
        {
            ErrorCode = error;
        }

        public UptResult(ErrorCode error, string innerMessage)
        {
            ErrorCode = error;
            InnerMessage = innerMessage;
        }
    }

    public enum ErrorCode
    {
        Success,
        UntypedError,
        UnsupportedFeature,
        SdkNotInitialized,
    }
}
