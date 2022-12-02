using System;
using System.Text.RegularExpressions;

namespace TestStories.API.Common
{
    public static class S3Utility
    {
        public static string GetFileName(string path)
        {
            try
            {
                return Regex.Match(path, @"[^\\:]*$").Value;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
