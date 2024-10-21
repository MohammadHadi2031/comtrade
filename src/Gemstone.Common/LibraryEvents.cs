using Gemstone.EventHandlerExtensions;

namespace Gemstone;
public static class LibraryEvents
{
    private static EventHandler<UnhandledExceptionEventArgs>? s_suppressedExceptionHandler;
    private static readonly object s_suppressedExceptionLock = new();

    /// <summary>
    /// Exposes exceptions that were suppressed but otherwise unhandled.  
    /// </summary>
    /// <remarks>
    /// <para>
    /// End users should attach to this event so that suppressed exceptions can be exposed to a log.
    /// </para>
    /// <para>
    /// The <see cref="LibraryEvents"/> class automatically attaches to the <see cref="TaskScheduler.UnobservedTaskException"/> event so that
    /// any unobserved task exceptions encountered will be marked as observed and exposed via the <see cref="SuppressedException"/> event.<br/>
    /// To disable this feature and only use custom <see cref="TaskScheduler.UnobservedTaskException"/> event handling, call the
    /// <see cref="DisableUnobservedTaskExceptionHandling"/> method during program initialization.
    /// </para>
    /// <para>
    /// Gemstone libraries only raise this event, no library functions attach to this end user-only event.
    /// </para>
    /// </remarks>
    public static event EventHandler<UnhandledExceptionEventArgs> SuppressedException
    {
        add
        {
            lock (s_suppressedExceptionLock)
                s_suppressedExceptionHandler += value;
        }
        remove
        {
            lock (s_suppressedExceptionLock)
                s_suppressedExceptionHandler -= value;
        }
    }

    // This method is internal to prevent exceptions from being recursively handled. Consequently, Gemstone libraries
    // should not attach to the SuppressedException event to avoid accidentally passing any caught exceptions back to
    // the OnSuppressedException method via an event handler for the SuppressedException event.
    internal static void OnSuppressedException(object? sender, Exception ex)
    {
        if (s_suppressedExceptionHandler is null)
            return;

        // Have to use custom exception handler here, default SafeInvoke handler already calls LibraryEvents.OnSuppressedException
        static void exceptionHandler(Exception ex, Delegate handler)
        {
            throw new Exception(
                $"Failed in {nameof(Gemstone)}.{nameof(LibraryEvents)}.{nameof(SuppressedException)} event handler \"{handler.GetHandlerName()}\": {ex.Message}",
                ex);
        }

        s_suppressedExceptionHandler.SafeInvoke(s_suppressedExceptionLock, exceptionHandler, sender, new UnhandledExceptionEventArgs(ex, false));
    }
}
