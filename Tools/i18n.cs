namespace Tools;

public class i18n
{
    /// <summary>
    /// Parses the locale query-string value into a <see cref="LocaleLang"/>.
    /// Returns <c>null</c> when the value is unrecognised so the caller can
    /// return 400 immediately, before any service call is made.
    /// </summary>
    public static LocaleLang? ParseLocale(string locale) =>
        locale.ToLower() switch
        {
            "fr" => LocaleLang.Fr,
            "en" => LocaleLang.En,
            _    => null,
        };
}