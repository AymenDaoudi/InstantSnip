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
    class TimeBeforeDeletingRule : ValidationRule
    {
        #region Properties
        #endregion

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {

                if (!((double.Parse((string) value) > _min) && (double.Parse((string) value) < _max)))
                    throw new ArgumentOutOfRangeException();
                return new ValidationResult(true, "Values must be between 0 and 180000");
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "Value can't contain non numerical characters");
            }
            catch (ArgumentOutOfRangeException)
            {
                return new ValidationResult(false, String.Format("Values must be between {0} and {1}",_min, _max));
            }
        }

        #region Fields

        private const double _min = 1000;
        private const double _max = 180000;

        #endregion
    }
}
