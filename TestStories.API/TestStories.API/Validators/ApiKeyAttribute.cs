using Humanizer.Localisation;
using System.ComponentModel.DataAnnotations;
using System;

namespace TestStories.API.Validators
{
    public class ApiKeyAttribute : ValidationAttribute
    {
        public ApiKeyAttribute(int apiKeyLength)
            => ApiKeyLenght = apiKeyLength;

        public int ApiKeyLenght { get; }

        public string GetErrorMessage() =>
            $"Api Key should be empty or have length of {ApiKeyLenght} chars.";

        protected override ValidationResult? IsValid(
            object? value, ValidationContext validationContext)
        {
            string apiKey = (string)value;
            if (String.IsNullOrEmpty(apiKey) || apiKey.Length == ApiKeyLenght)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(GetErrorMessage());
        }
    }
}
