using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KeeMind.Services;

public static class ValidateExtensions
{
    [return: NotNull]
    public static T ThrowIfNull<T>([NotNull] this T? o) => o ?? throw new InvalidOperationException();

    [return: NotNull]
    public static T ThrowIfNull<T>([NotNull] this T? o) where T : struct => o ?? throw new InvalidOperationException();

    [return: NotNull]
    public static T ThrowIfNull<T>([NotNull] this T? o, string exceptionMessage) => o ?? throw new InvalidOperationException(exceptionMessage);

    [return: NotNull]
    public static T ThrowIfNull<T, TException>([NotNull] this T? o) where TException : Exception, new() => o ?? throw new TException();

    [return: NotNull]
    public static T ThrowIfNull<T, TException>([NotNull] this T? o, string message) where TException : Exception, new() 
        => o ?? throw (Activator.CreateInstance(typeof(TException), new object?[] { message }) as TException) ?? throw new InvalidOperationException();

    [return: NotNull]
    public static string ThrowIfNullOrEmpty([NotNull] this string? o) => string.IsNullOrEmpty(o) ? throw new InvalidOperationException() : o;

    [return: NotNull]
    public static string ThrowIfNullOrEmpty([NotNull] this string? o, string exceptionMessage) => string.IsNullOrEmpty(o) ? throw new InvalidOperationException(exceptionMessage) : o;

    [return: NotNull]
    public static string ThrowIfNullOrEmpty<TException>([NotNull] this string? o) where TException : Exception, new() => string.IsNullOrEmpty(o) ? throw new TException() : o;

    [return: NotNull]
    public static string ThrowIsNullOrWhiteSpace([NotNull] this string? o) => string.IsNullOrWhiteSpace(o) ? throw new InvalidOperationException() : o;

    [return: NotNull]
    public static string ThrowIsNullOrWhiteSpace([NotNull] this string? o, string exceptionMessage) => string.IsNullOrWhiteSpace(o) ? throw new InvalidOperationException(exceptionMessage) : o;

    [return: NotNull]
    public static string ThrowIsNullOrWhiteSpace<TException>([NotNull] this string? o) where TException : Exception, new() => string.IsNullOrWhiteSpace(o) ? throw new TException() : o;

    public static void NotNull(object value, [NotNull] string parameterName, string? field = null)
    {
        if (value == null)
        {
            throw new ArgumentNullException(parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    public static void MustBeNull(object value, [NotNull] string parameterName, string? field = null)
    {
        if (value != null)
        {
            throw new ArgumentException($"Parameter must be null", parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    public static void NotNullOrEmptyOrWhiteSpace(string value, [NotNull] string parameterName, string? field = null)
    {
        if (value == null)
        {
            throw new ArgumentNullException(parameterName + (field == null ? string.Empty : "." + field));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Parameter can't be an empty or whitespace string", parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    public static void NotEmptyOrWhiteSpace(string value, [NotNull] string parameterName, string? field = null)
    {
        if (value == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Parameter can't be an empty or whitespace string", parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    public static bool IsValidInstanceName(string instanceName)
    {
        return !string.IsNullOrWhiteSpace(instanceName)
            && instanceName.Trim().Length >= 3;
    }

    public static void NotNullOrEmptyArray<T>(T[] values, [NotNull] string parameterName, string? field = null)
    {
        if (values == null)
        {
            throw new ArgumentNullException(parameterName + (field == null ? string.Empty : "." + field));
        }

        if (values.Length == 0)
        {
            throw new ArgumentException($"Parameter can't be an empty array", parameterName + (field == null ? string.Empty : "." + field));
        }

        if (values.Any(_ => _ == null))
        {
            throw new ArgumentException($"Parameter cannot contain Null values", parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    public static void NotNullOrContainingNullArray<T>(IEnumerable<T> values, [NotNull] string parameterName, string? field = null)
    {
        if (values == null)
        {
            throw new ArgumentNullException(parameterName + (field == null ? string.Empty : "." + field));
        }

        if (values.Any(_ => _ == null))
        {
            throw new ArgumentException($"Parameter cannot contain Null values", parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    public static void NotContainingNullArray<T>(IEnumerable<T> values, [NotNull] string parameterName, string? field = null)
    {
        if (values.Any(_ => _ == null))
        {
            throw new ArgumentException($"Parameter cannot contain Null values", parameterName + (field == null ? string.Empty : "." + field));
        }
    }


    public static void NotContainingNullOrWhiteSpaceStringArray(IEnumerable<string> values, [NotNull] string parameterName, string? field = null)
    {
        if (values.Any(_ => string.IsNullOrWhiteSpace(_)))
        {
            throw new ArgumentException($"Parameter cannot contain null or empty strings", parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    public static void NotEmpty(Guid id, [NotNull] string parameterName, string? field = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Parameter can't be empty", parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    public static void Any(bool[] arrayOfValidations, [NotNull] string parameterName, string? field = null)
    {
        if (!arrayOfValidations.Any(_ => _))
        {
            throw new ArgumentException("Parameter is not valid", parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    public static void All(bool[] arrayOfValidations, [NotNull] string parameterName,
         string? field = null)
    {
        if (!arrayOfValidations.All(_ => _))
        {
            throw new ArgumentException("Parameter is not valid",
                parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    public static void Positive(int value, [NotNull] string parameterName, string? field = null)
    {
        if (value <= 0)
        {
            throw new ArgumentException($"Parameter must be greater than 0", parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    public static void PositiveOrZero(int value, [NotNull] string parameterName, string? field = null)
    {
        if (value < 0)
        {
            throw new ArgumentException($"Parameter must be greater than or equal to 0", parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    public static void Between(double value, double min, double max, [NotNull] string parameterName, string? field = null)
    {
        if (value < min || value > max)
        {
            throw new ArgumentException($"Parameter must be greater than or equal to {min} and less than or equal to {max}", parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    private static readonly Regex ValidUsernameRegex = new Regex(@"^(?=[a-zA-Z])[-\w.]{0,23}([a-zA-Z\d]|(?<![-.])_)$", RegexOptions.Compiled);

    public static void Username([NotNull] string username, [NotNull] string parameterName, string? field = null)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException($"Username can't be an empty or whitespace string", parameterName + (field == null ? string.Empty : "." + field));
        }
        //faccio in modo di far passare le email come username
        if (!ValidUsernameRegex.IsMatch(username) && !ValidEmailRegex.IsMatch(username))
        {
            throw new ArgumentException("Username contains invalid characters", parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    public static void Username([NotNull] string username, int minUsernameLength, [NotNull] string parameterName, string? field = null)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException($"Username can't be an empty or whitespace string", parameterName + (field == null ? string.Empty : "." + field));
        }

        if (username.Length < minUsernameLength)
        {
            throw new ArgumentException($"Username must be at least {minUsernameLength} characters", parameterName + (field == null ? string.Empty : "." + field));
        }
        //faccio in modo di far passare le email come username
        if (!ValidUsernameRegex.IsMatch(username) && !ValidEmailRegex.IsMatch(username))
        {
            throw new ArgumentException("Username contains invalid characters", parameterName + (field == null ? string.Empty : "." + field));
        }
    }

    private static readonly Regex ValidEmailRegex = new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static void Email([NotNull] string email, [NotNull] string parameterName, string? field = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException($"Email can't be an empty or whitespace string", parameterName + (field == null ? string.Empty : "." + field));
        }

        if (email.IndexOf(' ') > -1)
        {
            throw new ArgumentException("Email must not contains spaces", parameterName + (field == null ? string.Empty : "." + field));
        }

        if (!ValidEmailRegex.IsMatch(email))
        {
            throw new ArgumentException("Email contains invalid characters", parameterName + (field == null ? string.Empty : "." + field));
        }
    }

}
