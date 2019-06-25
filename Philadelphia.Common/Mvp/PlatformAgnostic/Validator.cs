using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Philadelphia.Common {
    public static class Validator {
        public static void IsNotNull<T>(T x, ISet<string> errors) where T : new() {
            var realNull = default(T) == null;
            
            errors.IfTrueAdd(
                realNull && null == x ||
                !realNull && default(T).Equals(x), 
                I18n.Translate("Field cannot be empty"));
        }
        
        public static void IsNotNull<T>(T? x, ISet<string> errors) where T : struct {
            errors.IfTrueAdd(!x.HasValue, I18n.Translate("Field cannot be empty"));
        }

        public static void IsNotEmptyOrWhitespaceOnly(string x, ISet<string> errors) {
            errors.IfTrueAdd(string.IsNullOrWhiteSpace(x), I18n.Translate("Field cannot be empty"));
        }

        public static void IsNotNullRef<T>(T x, ISet<string> errors) where T : class {
            errors.IfTrueAdd(x == null, I18n.Translate("Field cannot be empty"));
        }
        
        public static Validate<T?> MustBeNonNegative<T>() where T : struct,IComparable<T> {
            return (x, errors) => errors.IfTrueAdd(
                x.HasValue && default(T).CompareTo(x.Value) > 0, 
                I18n.Translate("Must be non negative"));
        }

        public static void MustBeNonNegative<T>(T x, ISet<string> errors) where T : IComparable<T> {
            IsBiggerOrEqualTo(default(T), I18n.Translate("Must be zero or more"))(x, errors);
        }
        
        public static Validate<T?> MustBePositive<T>() where T:struct,IComparable<T> {
            return (x, errors) => {
                if (!x.HasValue || default(T).CompareTo(x.Value) >= 0 ) {errors.Add(I18n.Translate("Must be positive"));}};
        }
        
        public static void MustBePositive<T>(T x, ISet<string> errors) where T : IComparable<T> {
            IsBiggerThan(default(T), I18n.Translate("Must be positive"))(x, errors);
        }
        
        public static Validate<T> IsBiggerOrEqualTo<T>(T min, string msg) where T : IComparable<T> {
            return (x, errors) => errors.IfTrueAdd(min.CompareTo(x) > 0, msg);
        }
        
        public static Validate<T> IsBiggerThan<T>(T min, string msg) where T : IComparable<T> {
            return (x, errors) => errors.IfTrueAdd(min.CompareTo(x) >= 0, msg);
        }

        public static Validate<string> LimitSize(int length) {
            return (v, errors) => errors.IfTrueAdd(v != null && v.Length > length, 
                string.Format(I18n.Translate("Field is too long by {0} chars"), v?.Length - length));
        }
        
        public static void CannotBePastDate(DateTime? x, ISet<string> errors){
            errors.IfTrueAdd(x.HasValue && x.Value.DateOnly() < DateTime.Now.DateOnly(), I18n.Translate("Cannot be past date"));
        }
        
        public static void MustBeTomorrowOrLaterNullable(DateTime? x, ISet<string> errors) {
            errors.IfTrueAdd(x.HasValue && x.Value.DateOnly() < DateTimeExtensions.BuildSoonestMidnight(), I18n.Translate("Must be tomorrow or later"));
        }

        public static void IsValidEmailAddressOrEmpty(string emailAddress, ISet<string> errors) {
            if (emailAddress == null) {
                return; //contractors may miss email
            }
            emailAddress = emailAddress.Trim();
            var emailCount = emailAddress.Count(x => x == '@');

            switch (emailCount) {
                case 0:
                    errors.IfTrueAdd(!string.IsNullOrEmpty(emailAddress), I18n.Translate("This is not a valid email address"));
                    return;

                case 1:
                    if (emailAddress.Length < 3 || 
                        emailAddress.IndexOf('@') == 0 || 
                        emailAddress.IndexOf('@') == emailAddress.Length-1) {

                        errors.Add(I18n.Translate("This is not a valid email address"));
                    }
                    return;

                default:
                    errors.IfTrueAdd(!string.IsNullOrEmpty(emailAddress.Trim()), I18n.Translate("This is not a valid email address"));
                    break;
            }
        }

        public static Validate<string> MustMatchRegex(Regex re, string msg) {
            return (newValue, errors) => errors.IfTrueAdd(newValue == null || re.Matches(newValue).Count <= 0, msg);
        }
    }
}
