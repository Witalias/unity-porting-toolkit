namespace UPT.Services
{
    public interface IPlatformService
    {
        /// <summary>
        /// A callback for tracking the status of the overlay. It is called every time the overlay becomes visible or invisible for whatever reason.
        /// </summary>
        event OverlayStateChanged OnOverlayStateChanged;

        /// <summary>
        /// Get the preferred locale code. This is the system language or the language set in the settings by the user.
        /// </summary>
        /// <returns>The locale code corresponding to <see cref="UPT.Core.Constants.Locale"/>.</returns>
        string GetLocaleCode();

        /// <summary>
        /// Get the user's display name.
        /// </summary>
        /// <returns>The user's display name.</returns>
        string GetUsername();

        /// <summary>
        /// Get a unique user ID on the platform.
        /// </summary>
        /// <returns>A unique user ID.</returns>
        string GetUserID();

        /// <summary>
        /// Get the current status of the overlay.
        /// </summary>
        /// <returns><c>true</c> if the overlay is visible, <c>false</c> otherwise.</returns>
        bool IsOverlayVisible();
    }

    public delegate void OverlayStateChanged(bool isVisible);
}
