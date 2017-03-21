// <copyright file="StringUtility.cs" company="Eegee">
//   Copyright Erik Jan Meijer
// </copyright>
// <author>Erik Jan Meijer</author>
namespace Eegee
{
    using System.Globalization;

    /// <summary>
    /// A static class with generic string methods
    /// </summary>
    public static class StringUtility
    {
        /// <summary>
        /// String.Contains with arguments for CultureInfo
        /// </summary>
        /// <param name="haystack">the string to test</param>
        /// <param name="value">the string to seek</param>
        /// <param name="cultureInfo">a CultureInfo</param>
        /// <param name="compareOptions">Optional CompareOptions</param>
        /// <returns>true of string contains value, false if not</returns>
        public static bool Contains(string haystack, string value, CultureInfo cultureInfo, CompareOptions compareOptions = CompareOptions.None)
        {
            return cultureInfo.CompareInfo.IndexOf(haystack, value, compareOptions) > -1;
        }
    }
}
