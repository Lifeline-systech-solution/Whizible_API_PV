using Authentication;
using CommonFunctions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Whizible26.Application.Repository;
using Whizible26.Domain.Entity;

namespace Whizible26.Application.General
{
    /// <summary>
    /// Login validation for W26 API token endpoint (uses AuthRepository + GetAsyncSP pattern).
    /// </summary>
    public class Auth
    {
        private readonly IConfiguration _configuration;
        private readonly AuthRepository? _authRepository;

        public Auth(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionString = configuration.GetSection("ConnectionStrings:WhizibleDbConnection").Value;
            if (!string.IsNullOrEmpty(connectionString))
            {
                _authRepository = new AuthRepository(CommonFunctions.General.BuildConnectionString(connectionString));
            }
        }

        public async Task<LoginValidationResult> ValidateUserAsync(string username, string password)
        {
            var result = new LoginValidationResult
            {
                NormalizedUserName = NormalizeUserName(username),
                DisplayUserName = username
            };

            try
            {
                if (_authRepository == null)
                {
                    return result;
                }

                if (_configuration["AzureAD"] == "1")
                {
                    result.IsValid = true;
                    return result;
                }

                var objPw = new PWEncryption(username, password);
                var encryptedPassword = objPw.Encrypt().ToString();

                var loginRows = await GetValidateActiveLoginAsync(username, encryptedPassword).ConfigureAwait(false);
                if (loginRows.Count == 0)
                {
                    return result;
                }

                var row = loginRows[0];
                result.IsValid = true;
                result.LoginId = row.LoginID > 0 ? row.LoginID.ToString() : string.Empty;
                result.SecurityStamp = row.SecurityStamp?.ToString() ?? string.Empty;
                result.DisplayUserName =
                    row.DispalyName
                    ?? row.LoginName
                    ?? row.UserName
                    ?? username;

                return result;
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Error during user validation: {sqlEx.Message}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during user validation: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Same pattern as GetResourceDropdownData in EmployeeSkillsBulkUpload.
        /// </summary>
        private async Task<List<ValidateActiveLoginEntity>> GetValidateActiveLoginAsync(
            string loginName,
            string encryptedPassword)
        {
            try
            {
                if (_authRepository == null)
                {
                    return new List<ValidateActiveLoginEntity>();
                }

                return await _authRepository.ValidateActiveLoginAsync(loginName, encryptedPassword)
                    .ConfigureAwait(false);
            }
            catch (Exception)
            {
                return new List<ValidateActiveLoginEntity>();
            }
        }

        private static string NormalizeUserName(string? userName)
            => (userName ?? string.Empty).Trim().ToLowerInvariant();
    }

    public class LoginValidationResult
    {
        public bool IsValid { get; set; }
        public string? DisplayUserName { get; set; }
        public string? NormalizedUserName { get; set; }
        public string LoginId { get; set; } = string.Empty;
        public string SecurityStamp { get; set; } = string.Empty;
    }
}
