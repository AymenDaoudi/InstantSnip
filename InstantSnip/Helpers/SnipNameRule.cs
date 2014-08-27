using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace InstantSnip.Helpers
{
    class SnipNameRule : ValidationRule
    {
        #region Properties
        #endregion

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var match = Regex.Match((String) value, "[/\\:*?\"<>|]");
            return new ValidationResult(!match.Success, "A file name can't contain any of the following characters:" + "\n \\ / : \" * ? < > |");
        }

        #region Fields

        #endregion
    }
}
