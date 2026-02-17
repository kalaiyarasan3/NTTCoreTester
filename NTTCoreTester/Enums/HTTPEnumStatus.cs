using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NTTCoreTester.Enums
{
    public enum HTTPEnumStatus
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
        public static HTTPEnumStatus ToBusinessStatus(this int statusCode)
        {
            return statusCode switch
            {
                0 => HTTPEnumStatus.Success,
                5 => HTTPEnumStatus.InvalidData,
                6 => HTTPEnumStatus.SessionExpired,
                7 => HTTPEnumStatus.AuthendicationFailed,
                8 => HTTPEnumStatus.AuthorizationFailed,
                _ => HTTPEnumStatus.Failed
            };
        }

    }
}