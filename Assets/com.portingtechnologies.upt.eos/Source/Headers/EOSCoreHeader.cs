namespace UPT.Services
{
    public class UptEOSResult : UptResult
    {
#if !EOS_DISABLE
        public Epic.OnlineServices.Result EOSErrorCode { get; set; }
#endif

        public UptEOSResult(ErrorCode error) : base(error) { }
        public UptEOSResult(ErrorCode error, string innerMessage) : base(error, innerMessage) { }
    }
}
