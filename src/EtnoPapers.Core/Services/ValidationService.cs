using System.Collections.Generic;
using EtnoPapers.Core.Models;
using EtnoPapers.Core.Validation;

namespace EtnoPapers.Core.Services
{
    /// <summary>
    /// Validates article records against schema and business rules.
    /// </summary>
    public class ValidationService
    {
        private readonly ArticleRecordValidator _validator = new();

        public bool ValidateRecord(ArticleRecord record)
        {
            return _validator.Validate(record);
        }

        public bool CheckMandatoryFields(ArticleRecord record)
        {
            return _validator.ValidateMandatoryFields(record);
        }

        public List<string> GetValidationErrors(ArticleRecord record)
        {
            _validator.Validate(record);
            return _validator.ValidationErrors;
        }

        public bool IsValidForSaving(ArticleRecord record)
        {
            return _validator.IsValidForSaving(record);
        }
    }
}
