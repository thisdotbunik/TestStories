using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;

using TestStories.Common;
using Newtonsoft.Json.Linq;


namespace TestStories.API.Services.Validators
{
    public class GoogleReCaptchaValidator: ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            validationContext.DisplayName = "g-recaptcha-response";
            validationContext.MemberName = "g-recaptcha-response";

            if (string.IsNullOrEmpty(EnvironmentVariables.GOOGLE_APPLICATION_SECRET_KEY))
            {
                return new Lazy<ValidationResult>(() => new ValidationResult("Environment variable GOOGLE_APPLICATION_SECRET_KEY has not been defined", Array.Empty<string>())).Value;
            }

            if (string.IsNullOrEmpty(EnvironmentVariables.GOOGLE_APPLICATION_RECAPTCHA_URL))
            {
                return new Lazy<ValidationResult>(() => new ValidationResult("Environment variable GOOGLE_APPLICATION_RECAPTCHA_URL has not been defined", Array.Empty<string>())).Value;
            }

            var validationError = new Lazy<ValidationResult>(() => new ValidationResult("Google reCAPTCHA validation failed", Array.Empty<string>()));
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return validationError.Value;
            }

            var httpClient = new HttpClient();
            var httpResponse = httpClient.GetAsync(EnvironmentVariables.GOOGLE_APPLICATION_RECAPTCHA_URL + $"?secret={EnvironmentVariables.GOOGLE_APPLICATION_SECRET_KEY}&response={value}").Result;
            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                return validationError.Value;
            }

            var json = httpResponse.Content.ReadAsStringAsync().Result;
            dynamic jsonData = JObject.Parse(json);
            if (jsonData.success != true.ToString().ToLower())
            {
                return validationError.Value;
            }

            return ValidationResult.Success;
        }

    }
}
