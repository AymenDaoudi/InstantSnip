using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace InstantSnip.Helpers
{
    class SnipNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!string.IsNullOrWhiteSpace((String)value))
            {
                var match = Regex.Match((String) value, "[/\\:*?\"<>|]");
                return new ValidationResult(!match.Success, string.Concat("A file name can't contain any of the following characters:", "\n \\ / : \" * ? < > |"));
            }
            else
            {
                return new ValidationResult(false, "Empty file name");
            }
        }
    }
}
