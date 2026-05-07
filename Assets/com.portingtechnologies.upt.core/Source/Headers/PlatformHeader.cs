namespace UPT.Services
{
    public interface IPlatformService
    {
        event OverlayStateChanged OnOverlayStateChanged;

        string GetLocaleCode();
        string GetUsername();
        string GetUserID();
        bool IsOverlayVisible();
    }

    public delegate void OverlayStateChanged(bool isVisible);
}
