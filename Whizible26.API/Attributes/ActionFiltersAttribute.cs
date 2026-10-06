using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;

namespace WhizibleAPI.API.Filters
{
    /// <summary>
    /// Opt-in rate limit for data-modification POST actions (Save/Insert/Update/Delete).
    /// Uses appsettings Security:RateLimitEnabled, RateLimit_Duration_Minutes, RateLimit_Count.
    /// Added by Vishal Mane on 03/06/2026 for Rate Limiting.
    /// </summary>
    public class ValidateRateLimitAttribute : ActionFilterAttribute
    {
        private static readonly Dictionary<string, (int Count, DateTime Timestamp)> RequestCounts =
            new Dictionary<string, (int Count, DateTime Timestamp)>();

        private static readonly object SyncRoot = new object();

        private readonly IConfiguration _config;

        public ValidateRateLimitAttribute(IConfiguration config)
        {
            _config = config;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!_config.GetValue("Security:RateLimit_Enabled", true))
            {
                return;
            }

            if (!HttpMethods.IsPost(context.HttpContext.Request.Method))
            {
                return;
            }

            var durationMinutes = _config.GetValue("Security:RateLimit_Duration_Minutes", 5);
            var maxCount = _config.GetValue("Security:RateLimit_Count", 3);

            var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var controller = actionDescriptor?.ControllerName ?? "unknown";
            var action = actionDescriptor?.ActionName ?? "unknown";
            var userKey = context.HttpContext.User?.Identity?.Name ?? string.Empty;

            var key = $"{ip}|{controller}|{action}|{userKey}";
            var rateLimited = false;

            lock (SyncRoot)
            {
                if (!RequestCounts.ContainsKey(key))
                {
                    RequestCounts[key] = (1, DateTime.UtcNow);
                }
                else
                {
                    var entry = RequestCounts[key];
                    if ((DateTime.UtcNow - entry.Timestamp).TotalMinutes < durationMinutes)
                    {
                        if (entry.Count >= maxCount)
                        {
                            rateLimited = true;
                        }
                        else
                        {
                            RequestCounts[key] = (entry.Count + 1, entry.Timestamp);
                        }
                    }
                    else
                    {
                        RequestCounts[key] = (1, DateTime.UtcNow);
                    }
                }
            }

            if (rateLimited)
            {
                context.Result = new ObjectResult("Too many attempts for this request")
                {
                    StatusCode = (int)HttpStatusCode.TooManyRequests
                };
            }
        }
    }

    //End of Added by Vishal Mane on 03/06/2026 for Rate Limiting.

    public class ValidateHeadersAttribute : ActionFilterAttribute
    {
        private readonly IConfiguration _config;

        public ValidateHeadersAttribute(IConfiguration config)
        {
            _config = config;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            bool tokenAuthenticate = _config.GetValue<bool>("TokenAuthenticate:TokenAuthenticate");
            if (!tokenAuthenticate)
                return;

            // Added By Dipali V On 23rd Jan 2026 For W26 - Skip validation for [FromForm] requests (file uploads)
            // [FromForm] requests use multipart/form-data and model binding happens differently
            var actionDescriptor = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
            if (actionDescriptor != null)
            {
                var parameters = actionDescriptor.MethodInfo.GetParameters();
                bool hasFromForm = parameters.Any(p => p.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.FromFormAttribute), false).Any());
                if (hasFromForm)
                {
                    // Skip validation for file upload endpoints - they don't use JSON body
                    return;
                }
            }

            var headers = context.HttpContext.Request.Headers;
            if (!headers.ContainsKey("Params"))
                return;

            string encryptedParam = headers["Params"].FirstOrDefault();
            if (string.IsNullOrEmpty(encryptedParam))
                return;

            try
            {
                // Added By Dipali V On 23rd Jan 2026 For W26 - Handle both encrypted and unencrypted Params
                // Check if the param is already JSON (unencrypted) or needs decryption
                string decrypted = encryptedParam.Trim();

                // If it starts with { or [, it's likely already JSON (unencrypted)
                // Otherwise, try to decrypt it
                if (!decrypted.StartsWith("{") && !decrypted.StartsWith("["))
                {
                    // Try to decrypt - this will fail if it's not in encrypted format
                    try
                    {
                        decrypted = CommonFunctions.General.DecryptString(encryptedParam);
                    }
                    catch
                    {
                        // If decryption fails, assume it's already in the correct format
                        decrypted = encryptedParam;
                    }
                }

                JObject obj = null;
                JValue val = null;
                try
                {
                    obj = JsonConvert.DeserializeObject<JObject>(decrypted);
                }
                catch
                {
                    val = JsonConvert.DeserializeObject<JValue>(decrypted);
                }

                foreach (var arg in context.ActionArguments)
                {
                    if (obj != null)
                        ValidateObject(obj, arg.Value);
                    else if (val != null && Convert.ToString(val.Value) != Convert.ToString(arg.Value))
                        throw new Exception("Parameter mismatch");
                }
            }
            catch (Exception ex)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Message = ex.Message == "Parameter mismatch"
                        ? "Parameter mismatch"
                        : "Something went wrong"
                });
            }
        }

        #region Helpers - type classification

        /// <summary>
        /// Returns true for types that can be safely string-compared:
        /// primitives, string, DateTime, bool, decimal, Guid, enums,
        /// and their Nullable&lt;T&gt; variants.
        /// Everything else (List, array, nested class) is NOT scalar.
        /// </summary>
        private static bool IsScalarType(Type t)
        {
            // Unwrap Nullable<T> e.g. int? -> int, bool? -> bool, DateTime? -> DateTime
            t = Nullable.GetUnderlyingType(t) ?? t;

            return t.IsPrimitive          // int, long, double, float, bool, char, byte ...
                || t == typeof(string)
                || t == typeof(decimal)
                || t == typeof(DateTime)
                || t == typeof(DateTimeOffset)
                || t == typeof(Guid)
                || t.IsEnum;
        }

        /// <summary>
        /// Returns true for List&lt;T&gt;, T[], or any IEnumerable except string.
        /// </summary>
        private static bool IsCollectionType(Type t)
        {
            return t != typeof(string) && (t.IsArray
                    || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
                    || typeof(System.Collections.IEnumerable).IsAssignableFrom(t));
        }

        #endregion

        #region Core validation

        /// <summary>
        /// Validates every property in jsonObj against the corresponding
        /// property on model.
        ///
        /// Scalar        - direct string comparison (DateTime-aware).
        /// List&lt;T&gt; / T[] - each JSON array element validated against the matching
        ///                  item in the model collection (same index). Recurses for
        ///                  nested objects inside the list.
        /// Complex object - recurses into ValidateObject with the child JObject
        ///                  and child model value.
        ///
        /// Previously the method was flat-only and threw "Parameter mismatch"
        /// whenever a property was a List&lt;T&gt; because Convert.ToString(List&lt;T&gt;)
        /// returns the CLR type name string, not the actual values.
        /// </summary>
        private void ValidateObject(JObject jsonObj, object model)
        {
            if (model == null)
                throw new Exception("Something went wrong");

            var props = model.GetType().GetProperties();

            foreach (var token in jsonObj.Children())
            {
                foreach (var li in token.ToList())
                {
                    var rawPath = li.Path ?? "";
                    var dotIdx = rawPath.LastIndexOf('.');
                    var propName = dotIdx >= 0 ? rawPath.Substring(dotIdx + 1) : rawPath;
                    // Case-insensitive property lookup (Vaibhav K, 08-01-25)
                    var prop = props.FirstOrDefault(
                        p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));

                    if (prop == null)
                        continue;

                    var propType = prop.PropertyType;
                    var modelValue = prop.GetValue(model);

                    // Unwrap JProperty to get the actual value token
                    var jsonToken = li is JProperty jp ? jp.Value : (JToken)li;

                    // ── 1. SCALAR (int, string, bool, DateTime, decimal, Guid ...) ──
                    if (IsScalarType(propType))
                    {
                        ValidateScalar(prop, modelValue, jsonToken);
                        continue;
                    }

                    // ── 2. COLLECTION (List<T> / T[]) ──────────────────────────────
                    if (IsCollectionType(propType))
                    {
                        if (jsonToken.Type != JTokenType.Array)
                            continue; // JSON side isn't an array - nothing to compare

                        var jArray = (JArray)jsonToken;
                        var modelList = modelValue as System.Collections.IList;

                        if (modelList == null || modelList.Count == 0)
                            continue; // model collection is empty - nothing to compare

                        for (int i = 0; i < jArray.Count && i < modelList.Count; i++)
                        {
                            var jItem = jArray[i];
                            var mItem = modelList[i];

                            if (mItem == null)
                                continue;

                            if (jItem is JObject nestedObj)
                            {
                                // Recurse: validate each list element as a nested object
                                ValidateObject(nestedObj, mItem);
                            }
                            else if (IsScalarType(mItem.GetType()))
                            {
                                // Collection of scalars e.g. List<int>, List<string>
                                var mStr = Convert.ToString(mItem) ?? "";
                                var jStr = Convert.ToString(jItem) ?? "";
                                if (mStr != jStr)
                                    throw new Exception("Parameter mismatch");
                            }
                        }

                        continue;
                    }

                    // ── 3. COMPLEX OBJECT (nested class) ───────────────────────────
                    if (jsonToken is JObject childJson && modelValue != null)
                    {
                        ValidateObject(childJson, modelValue);
                    }
                    // If modelValue is null or jsonToken is null/undefined, skip silently.
                }
            }
        }

        /// <summary>
        /// Compares a single scalar property value (DateTime-safe).
        /// Throws "Parameter mismatch" if values differ.
        /// </summary>
        private void ValidateScalar(PropertyInfo prop, object modelValue, JToken jsonToken)
        {
            var baseType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            var modelStr = Convert.ToString(modelValue) ?? "";
            if (baseType == typeof(int) && modelStr == "")
            {
                modelStr = "0";
            }
            var jsonStr = Convert.ToString(GetValueOfProperty(prop, jsonToken)) ?? "";

            // DateTime-safe comparison (Added By Vaibhav On 21 Jan 2026)
            if (TryParseDate(modelStr, out DateTime modelDate) &&
                TryParseDate(jsonStr, out DateTime jsonDate))
            {
                if (modelDate != jsonDate)
                    throw new Exception("Parameter mismatch");
            }
            else
            {
                if (modelStr != jsonStr)
                    throw new Exception("Parameter mismatch");
            }
        }

        #endregion

        #region GetValueOfProperty

        /// <summary>
        /// Extracts a comparable string value from the JSON token for the given property.
        /// FIX: unwraps Nullable&lt;T&gt; before the type comparison so int?/bool?/DateTime?
        /// are handled identically to their non-nullable counterparts.
        /// All original logic branches are preserved.
        /// </summary>
        private object GetValueOfProperty(PropertyInfo info, object value)
        {
            // Unwrap Nullable<T> - key fix for int?, bool?, DateTime?
            // Previously: info.PropertyType == typeof(int) was FALSE for int? (Nullable<int>)
            // and those types fell through to the raw Convert.ToString path.
            var baseType = Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType;

            if (baseType == typeof(DateTime))
            {    // Handled Null date Added By Nikhil Mane
                if (Convert.ToString(value) == "")
                {
                    return "";
                }
                // End of Handled Null date Added By Nikhil Mane
                return Convert.ToDateTime(value).ToString();
            }

            if (baseType == typeof(bool))
            {
                // Handled Null bool Added By Nikhil Mane
                if (Convert.ToString(value) == "")
                {
                    return "";
                }
                // End of Handled bool date Added By Nikhil Mane
                if (value is JValue jVal && jVal.Type == JTokenType.Boolean)
                    return jVal.Value<bool>().ToString();
                return Convert.ToBoolean(value).ToString();
            }

            if (baseType == typeof(int))
                return string.IsNullOrWhiteSpace(Convert.ToString(value)) ? "0" : value.ToString();

            if (Convert.ToString(value) == "null")
                return "";

            return Convert.ToString(value);
        }

        #endregion

        //Added By Vaibhav On 21 Jan 2026 for the correcting the date format
        private bool TryParseDate(string value, out DateTime date)
        {
            date = default;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            return DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                out date);
        }
        //End of Added By Vaibhav On 21 Jan 2026 for the correcting the date format

    }
}
