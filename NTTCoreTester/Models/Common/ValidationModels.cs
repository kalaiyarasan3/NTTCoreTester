using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    namespace NTTCoreTester.Models.Common
    {
        /// <summary>
        /// Validation level enum
        /// </summary>
        public enum ValidationLevel
        {
            Level1_Envelope,
            Level2_CommonFields,
            Level3_ActivitySpecific,
            StatusCorrelation
        }

        /// <summary>
        /// Represents a single validation error
        /// </summary>
        public class ValidationError
        {
            public ValidationError(ValidationLevel level, string field, string message,
                                  object expected = null, object actual = null)
            {
                Level = level;
                Field = field;
                Message = message;
                Expected = expected;
                Actual = actual;
                Timestamp = DateTime.Now;
            }

            public ValidationLevel Level { get; set; }
            public string Field { get; set; }
            public string Message { get; set; }
            public object Expected { get; set; }
            public object Actual { get; set; }
            public DateTime Timestamp { get; set; }

            public override string ToString()
            {
                var result = $"{Field}: {Message}";

                if (Expected != null && Actual != null)
                    result += $" (Expected: {Expected}, Actual: {Actual})";
                else if (Expected != null)
                    result += $" (Expected: {Expected})";
                else if (Actual != null)
                    result += $" (Actual: {Actual})";

                return result;
            }

            public string ToDetailedString()
            {
                return $"[{Level}] {Field}: {Message}" +
                       (Expected != null ? $"\n  Expected: {Expected}" : "") +
                       (Actual != null ? $"\n  Actual: {Actual}" : "");
            }
        }

        /// <summary>
        /// Tracks validation results across all 3 levels
        /// </summary>
        public class ValidationResult
        {
            public ValidationResult()
            {
                Level1Errors = new List<ValidationError>();
                Level2Errors = new List<ValidationError>();
                Level3Errors = new List<ValidationError>();
                CorrelationErrors = new List<ValidationError>();
            }

            public List<ValidationError> Level1Errors { get; set; }
            public List<ValidationError> Level2Errors { get; set; }
            public List<ValidationError> Level3Errors { get; set; }
            public List<ValidationError> CorrelationErrors { get; set; }

            public bool IsValid => !HasErrors;
            public bool HasErrors => Level1Errors.Any() || Level2Errors.Any() ||
                                     Level3Errors.Any() || CorrelationErrors.Any();

            public int TotalErrorCount => Level1Errors.Count + Level2Errors.Count +
                                          Level3Errors.Count + CorrelationErrors.Count;

            public void AddLevel1Error(string field, string message, object expected = null, object actual = null)
            {
                Level1Errors.Add(new ValidationError(ValidationLevel.Level1_Envelope, field, message, expected, actual));
            }

            public void AddLevel2Error(string field, string message, object expected = null, object actual = null)
            {
                Level2Errors.Add(new ValidationError(ValidationLevel.Level2_CommonFields, field, message, expected, actual));
            }

            public void AddLevel3Error(string field, string message, object expected = null, object actual = null)
            {
                Level3Errors.Add(new ValidationError(ValidationLevel.Level3_ActivitySpecific, field, message, expected, actual));
            }

            public void AddCorrelationError(string message, object expected = null, object actual = null)
            {
                CorrelationErrors.Add(new ValidationError(ValidationLevel.StatusCorrelation, "StatusCorrelation", message, expected, actual));
            }

            public string GetAllErrorsFormatted()
            {
                var errors = new List<string>();

                if (Level1Errors.Any())
                    errors.Add($"[L1] {string.Join("; ", Level1Errors.Select(e => e.ToString()))}");

                if (Level2Errors.Any())
                    errors.Add($"[L2] {string.Join("; ", Level2Errors.Select(e => e.ToString()))}");

                if (CorrelationErrors.Any())
                    errors.Add($"[Corr] {string.Join("; ", CorrelationErrors.Select(e => e.ToString()))}");

                if (Level3Errors.Any())
                    errors.Add($"[L3] {string.Join("; ", Level3Errors.Select(e => e.ToString()))}");

                return string.Join(" | ", errors);
            }

            public string GetSummary()
            {
                if (IsValid)
                    return "All validations passed";

                return $"Failed: L1={Level1Errors.Count}, L2={Level2Errors.Count}, " +
                       $"Corr={CorrelationErrors.Count}, L3={Level3Errors.Count}";
            }
        }
    }

}
