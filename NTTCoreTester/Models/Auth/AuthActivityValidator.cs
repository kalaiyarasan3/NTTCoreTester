using NTTCoreTester.Models.Auth;
using NTTCoreTester.Models.Common;
using System;
using System.Collections.Generic;

namespace NTTCoreTester.Validators.Auth
{
   
    public interface IAuthActivityValidator
    {
        ValidationResult ValidateSendOtp(SendOtpData data);
        ValidationResult ValidateLoginSuccess(LoginSuccessData data);
        ValidationResult ValidateLoginError(LoginErrorData data);
    }

    public class AuthActivityValidator : IAuthActivityValidator
    {
        // Validates SendOTP - data types and structure (3 fields)
        public ValidationResult ValidateSendOtp(SendOtpData data)
        {
            var result = new ValidationResult();

            if (data == null)
            {
                result.AddLevel3Error("SendOtpData", "SendOtpData is null", "SendOtpData", null);
                return result;
            }

            // DealerUCC - must be string
            ValidateStringType(data.DealerUCC, "DealerUCC", false, result);


            return result;
        }

        /// Validates Login SUCCESS - data types and structure (17 fields + complex objects)
        public ValidationResult ValidateLoginSuccess(LoginSuccessData data)
        {
            var result = new ValidationResult();

            if (data == null)
            {
                result.AddLevel3Error("LoginSuccessData", "LoginSuccessData is null", "LoginSuccessData", null);
                return result;
            }

            // String fields
            ValidateStringType(data.susertoken, "susertoken", false, result);
            ValidateStringType(data.uname, "uname", false, result);
            ValidateStringType(data.uid, "uid", false, result);
            ValidateStringType(data.email, "email", true, result); // nullable

            // Number fields - enforced by C# type system
            // TOTPEnabled, IsTOTPSkip, DealerStatus, ClientType - all int

            // Array fields - check structure (specify type explicitly)
            ValidateListOfStrings(data.prarr, "prarr", false, result);
            ValidateListOfStrings(data.access_type, "access_type", false, result);
            ValidateListOfStrings(data.orarr, "orarr", false, result);
            ValidateListOfInts(data.KraStatus, "KraStatus", true, result); // nullable
            ValidateListOfStrings(data.Clients, "Clients", true, result); // nullable

            // Complex object: values - must be Dictionary<string, string>
            if (data.values == null)
            {
                result.AddLevel3Error("values", "values is null", "Dictionary<string, string>", null);
            }
            else if (!(data.values is Dictionary<string, string>))
            {
                result.AddLevel3Error("values", "values is not a Dictionary", "Dictionary<string, string>",
                    data.values.GetType().Name);
            }
            else
            {
                // Validate all values are strings
                foreach (var kvp in data.values)
                {
                    if (kvp.Value != null && !(kvp.Value is string))
                    {
                        result.AddLevel3Error($"values[{kvp.Key}]", "Value is not a string", "string",
                            kvp.Value.GetType().Name);
                    }
                }
            }

            // Complex object: mws - must be Dictionary<string, List<MwsItem>>
            if (data.mws == null)
            {
                result.AddLevel3Error("mws", "mws is null", "Dictionary<string, List<MwsItem>>", null);
            }
            else if (!(data.mws is Dictionary<string, List<MwsItem>>))
            {
                result.AddLevel3Error("mws", "mws is not a Dictionary", "Dictionary<string, List<MwsItem>>",
                    data.mws.GetType().Name);
            }
            else
            {
                // Validate structure of mws
                foreach (var kvp in data.mws)
                {
                    if (kvp.Value == null)
                    {
                        result.AddLevel3Error($"mws[{kvp.Key}]", "Array is null", "List<MwsItem>", null);
                        continue;
                    }

                    if (!(kvp.Value is List<MwsItem>))
                    {
                        result.AddLevel3Error($"mws[{kvp.Key}]", "Value is not a List", "List<MwsItem>",
                            kvp.Value.GetType().Name);
                        continue;
                    }

                    // Validate each MwsItem structure
                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        ValidateMwsItemStructure(kvp.Value[i], $"mws[{kvp.Key}][{i}]", result);
                    }
                }
            }

            // AuthorizedActivity - nullable, any type
            // No validation needed

            return result;
        }

        // Validates Login ERROR - just check structure exists
        public ValidationResult ValidateLoginError(LoginErrorData data)
        {
            var result = new ValidationResult();

            if (data == null)
            {
                result.AddLevel3Error("LoginErrorData", "LoginErrorData is null", "LoginErrorData", null);
            }

            return result;
        }

        private void ValidateStringType(object value, string fieldName, bool nullable, ValidationResult result)
        {
            if (value == null)
            {
                if (!nullable)
                {
                    result.AddLevel3Error(fieldName, $"{fieldName} is null", "string", null);
                }
            }
            else if (!(value is string))
            {
                result.AddLevel3Error(fieldName, $"{fieldName} is not a string", "string", value.GetType().Name);
            }
        }

        private void ValidateListOfStrings(List<string> value, string fieldName, bool nullable, ValidationResult result)
        {
            if (value == null)
            {
                if (!nullable)
                {
                    result.AddLevel3Error(fieldName, $"{fieldName} is null", "List<string>", null);
                }
            }
        }

        private void ValidateListOfInts(List<int> value, string fieldName, bool nullable, ValidationResult result)
        {
            if (value == null)
            {
                if (!nullable)
                {
                    result.AddLevel3Error(fieldName, $"{fieldName} is null", "List<int>", null);
                }
            }
        }

        private void ValidateMwsItemStructure(MwsItem item, string path, ValidationResult result)
        {
            if (item == null)
            {
                result.AddLevel3Error(path, "MwsItem is null", "MwsItem", null);
                return;
            }

            ValidateStringType(item.MarketWatchName, $"{path}.MarketWatchName", false, result);
            ValidateStringType(item.exch, $"{path}.exch", false, result);
            ValidateStringType(item.tsym, $"{path}.tsym", false, result);
            ValidateStringType(item.Segment, $"{path}.Segment", false, result);
            ValidateStringType(item.instname, $"{path}.instname", false, result);
            ValidateStringType(item.pp, $"{path}.pp", false, result);
            ValidateStringType(item.ls, $"{path}.ls", false, result);
            ValidateStringType(item.ti, $"{path}.ti", false, result);

            // token - must be int (enforced by C#)
            // SymbolInfos - nullable, any type
        }
    }
}
