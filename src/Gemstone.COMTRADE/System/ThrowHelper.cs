using System.Runtime.CompilerServices;

namespace System;
internal static class ThrowHelper
{
    internal static void ThrowArgumentNullExceptionIfNull(
#if NET
            [NotNull]
#endif
        object? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }
}
