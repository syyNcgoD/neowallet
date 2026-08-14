namespace NeoWallet.Domain.Common;
public static class Guard
{
    public static T AgainstNull<T>(T? input, string parameterName)
    {
        if (input is null)
        {
            throw new ArgumentNullException(parameterName, $"Parameter '{parameterName}' cannot be null.");
        }

        return input;
    }

    public static string AgainstNullOrWhiteSpace(string? input, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException($"Parameter '{parameterName}' cannot be null, empty, or whitespace.", parameterName);
        }

        return input;
    }

    public static T AgainstDefault<T>(T input, string parameterName) where T : struct, IEquatable<T>
    {
        if (input.Equals(default))
        {
            throw new ArgumentException($"Parameter '{parameterName}' cannot have default value.", parameterName);
        }

        return input;
    }

    public static decimal AgainstNegative(decimal input, string parameterName)
    {
        if (input < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, input, $"Parameter '{parameterName}' cannot be negative.");
        }

        return input;
    }

    public static decimal AgainstZeroOrNegative(decimal input, string parameterName)
    {
        if (input <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, input, $"Parameter '{parameterName}' must be strictly positive.");
        }

        return input;
    }

    public static DateTime AgainstNonUtc(DateTime input, string parameterName)
    {
        if (input.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException($"Parameter '{parameterName}' must have DateTimeKind.Utc.", parameterName);
        }

        return input;
    }
}
