using Microsoft.AspNet.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Globalization;

namespace Owin2AspNet.Helper
{
    internal static class ParsingHelper
    {
        public static long? GetContentLength(IHeaderDictionary headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            const NumberStyles styles = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
            long value;
            var rawValue = headers[HeaderNames.ContentLength];
            if (rawValue.Count == 1 &&
                !string.IsNullOrWhiteSpace(rawValue[0]) &&
                long.TryParse(rawValue[0], styles, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }

            return null;
        }

        public static void SetContentLength(IHeaderDictionary headers, long? value)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            if (value.HasValue)
            {
                headers[HeaderNames.ContentLength] = value.Value.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                headers.Remove(HeaderNames.ContentLength);
            }
        }
    }
}