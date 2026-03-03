using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NTTCoreTester.Enums
{
    public enum ApiBusinessStatus
    {
        [Display(Name = "Success")]
        Success,

        [Display(Name = "Error")]
        Error,

        [Display(Name = "Exception")]
        Exception,

        [Display(Name = "InvalidData")]
        InvalidData,

        [Display(Name = "Failed")]
        Failed,

        [Display(Name = "Invalid")]
        Invalid,

        [Display(Name = "SessionExpired")]
        SessionExpired,

        [Display(Name = "AuthendicationFailed")]
        AuthendicationFailed,

        [Display(Name = "AuthorizationFailed")]
        AuthorizationFailed,

        [Display(Name = "MoreActionRequired")]
        MoreActionRequired
    }
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var member = enumValue.GetType()
                                  .GetMember(enumValue.ToString())
                                  .FirstOrDefault();

            if (member != null)
            {
                var displayAttr = member.GetCustomAttribute<DisplayAttribute>();
                if (displayAttr != null)
                    return displayAttr.Name;
            }

            return enumValue.ToString();
        }
        public static ApiBusinessStatus ToBusinessStatus(this int statusCode)
        {
            return statusCode switch
            {
                0 => ApiBusinessStatus.Success,
                1 => ApiBusinessStatus.Error,
                2 => ApiBusinessStatus.Exception,
                3 => ApiBusinessStatus.Invalid,
                5 => ApiBusinessStatus.InvalidData,
                6 => ApiBusinessStatus.SessionExpired,
                7 => ApiBusinessStatus.AuthendicationFailed,
                8 => ApiBusinessStatus.AuthorizationFailed,
                9 => ApiBusinessStatus.MoreActionRequired,
                _ => ApiBusinessStatus.Failed
            };
        }


    }
}