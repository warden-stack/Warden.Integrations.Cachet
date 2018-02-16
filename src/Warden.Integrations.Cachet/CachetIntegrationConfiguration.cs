﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Warden.Integrations.Cachet
{
    /// <summary>
    /// Configuration of the CachetIntegration.
    /// </summary>
    public class CachetIntegrationConfiguration
    {
        public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatString = "yyyy-MM-dd H:mm:ss",
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Populate,
            NullValueHandling = NullValueHandling.Include,
            Error = (serializer, error) => { error.ErrorContext.Handled = true; },
            Converters = new List<JsonConverter>
            {
                new Newtonsoft.Json.Converters.StringEnumConverter
                {
                    AllowIntegerValues = true,
                    CamelCaseText = true
                }
            }
        };

        /// <summary>
        /// Default request header name of the API key.
        /// </summary>
        public const string AccessTokenHeader = "X-Cachet-Token";

        /// <summary>
        /// URL of the Cachet API.
        /// </summary>
        public Uri ApiUrl { get; protected set; }

        /// <summary>
        /// Access token of the Cachet account.
        /// </summary>
        public string AccessToken { get; protected set; }

        /// <summary>
        /// Username of the Cachet account.
        /// </summary>
        public string Username { get; protected set; }

        /// <summary>
        /// Password of the Cachet account.
        /// </summary>
        public string Password { get; protected set; }

        /// <summary>
        /// Request headers.
        /// </summary>
        public IDictionary<string, string> Headers { get; protected set; }

        /// <summary>
        /// Optional timeout of the HTTP request.
        /// </summary>
        public TimeSpan? Timeout { get; protected set; }

        /// <summary>
        /// Flag determining whether an exception should be thrown if HTTP request returns invalid reponse (false by default).
        /// </summary>
        public bool FailFast { get; protected set; }

        /// <summary>
        /// Id of the group to which will be assigned the watchers (components) - 0 by default.
        /// </summary>
        public int GroupId { get; protected set; }

        /// <summary>
        /// Custom JSON serializer settings of the Newtonsoft.Json library.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; protected set; } = DefaultJsonSerializerSettings;

        /// <summary>
        /// Custom provider for the DateTime (UTC by default).
        /// </summary>
        public Func<DateTime> DateTimeProvider { get; protected set; }

        /// <summary>
        /// Custom provider for the ICachetService.
        /// </summary>
        public Func<ICachetService> CachetServiceProvider { get; protected set; }

        /// <summary>
        /// Mappings between the Watcher group names and group ids used in Cachet to organize the similar components.
        /// </summary>
        public IDictionary<string, int> WatcherGroups { get; protected set; } = new Dictionary<string, int>();

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the CachetIntegrationConfiguration.
        /// </summary>
        /// <param name="apiUrl">URL of the Cachet API.</param>
        /// <param name="accessToken">Access token of the Cachet account.</param>
        /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
        public static Builder Create(string apiUrl, string accessToken)
            => new Builder(apiUrl, accessToken);

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the CachetIntegrationConfiguration.
        /// </summary>
        /// <param name="apiUrl">URL of the Cachet API.</param>
        /// <param name="username">Username of the Cachet account.</param>
        /// <param name="password">Password of the Cachet account.</param>
        /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
        public static Builder Create(string apiUrl, string username, string password) 
            => new Builder(apiUrl, username, password);

        protected CachetIntegrationConfiguration(string apiUrl, string accessToken)
        {
            if (string.IsNullOrEmpty(apiUrl))
                throw new ArgumentException("API URL can not be empty.", nameof(apiUrl));
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token can not be empty.", nameof(accessToken));

            ApiUrl = GetApiUrl(apiUrl);
            AccessToken = accessToken;
            CachetServiceProvider = () => new CachetService(ApiUrl, JsonSerializerSettings,
                accessToken, AccessTokenHeader);
            Headers = new Dictionary<string, string>();
            DateTimeProvider = () => DateTime.UtcNow;
        }

        protected CachetIntegrationConfiguration(string apiUrl, string username, string password)
        {
            if (string.IsNullOrEmpty(apiUrl))
                throw new ArgumentException("API URL can not be empty.", nameof(apiUrl));
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username can not be empty.", nameof(username));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password can not be empty.", nameof(password));

            ApiUrl = GetApiUrl(apiUrl);
            Username = username;
            Password = password;
            CachetServiceProvider = () => new CachetService(ApiUrl, JsonSerializerSettings,
                username: username, password: password);
            Headers = new Dictionary<string, string>();
            DateTimeProvider = () => DateTime.UtcNow;
        }

        private static Uri GetApiUrl(string apiUrl) => apiUrl.EndsWith("/")
            ? new Uri(apiUrl.Substring(0, apiUrl.Length - 1))
            : new Uri(apiUrl);

        /// <summary>
        /// Fluent builder for the SlackIntegrationConfiguration.
        /// </summary>
        public class Builder
        {
            protected readonly CachetIntegrationConfiguration Configuration;

            /// <summary>
            /// Constructor of fluent builder for the CachetIntegrationConfiguration.
            /// </summary>
            /// <param name="apiUrl">URL of the Cachet API.</param>
            /// <param name="accessToken">Access token of the Cachet account.</param>
            /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
            public Builder(string apiUrl, string accessToken)
            {
                Configuration = new CachetIntegrationConfiguration(apiUrl, accessToken);
            }

            /// <summary>
            /// Constructor of fluent builder for the CachetIntegrationConfiguration.
            /// </summary>
            /// <param name="apiUrl">URL of the Cachet API.</param>
            /// <param name="username">Username of the Cachet account.</param>
            /// <param name="password">Password of the Cachet account.</param>
            /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
            public Builder(string apiUrl, string username, string password)
            {
                Configuration = new CachetIntegrationConfiguration(apiUrl, username, password);
            }

            /// <summary>
            /// Timeout of the HTTP request.
            /// </summary>
            /// <param name="timeout">Timeout.</param>
            /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
            public Builder WithTimeout(TimeSpan timeout)
            {
                if (timeout == null)
                    throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

                if (timeout == TimeSpan.Zero)
                    throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));

                Configuration.Timeout = timeout;

                return this;
            }

            /// <summary>
            /// Request headers of the HTTP request.
            /// </summary>
            /// <param name="headers">Collection of the HTTP request headers.</param>
            /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
            public Builder WithHeaders(IDictionary<string, string> headers)
            {
                if (headers == null || !headers.Any())
                    throw new ArgumentNullException(nameof(headers), "Request headers can not be empty.");

                Configuration.Headers = headers;

                return this;
            }

            /// <summary>
            /// Flag determining whether an exception should be thrown if HTTP request returns invalid reponse (false by default).
            /// </summary>
            /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
            public Builder FailFast()
            {
                Configuration.FailFast = true;

                return this;
            }

            /// <summary>
            /// Id of the group to which will be assigned the watchers (components) - 0 by default.
            /// Overriden by the Watcher group mappings parameter if provided.
            /// </summary>
            /// <param name="groupId">Id of the component group (0 by default).</param>
            /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
            public Builder WithGroupId(int groupId)
            {
                Configuration.GroupId = groupId;

                return this;
            }

            /// <summary>
            /// Mappings between the Watcher group names and group ids used in Cachet to organize the similar components.
            /// Overrides the global Watcher Group id parameter.
            /// </summary>
            /// <param name="watcherGroups">Dictionary of watcher group names and component group ids.</param>
            /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
            public Builder WithWatcherGroups(IDictionary<string, int> watcherGroups)
            {
                if (watcherGroups == null)
                {
                    throw new ArgumentNullException(nameof(watcherGroups),
                        "Watcher groups can not be null.");
                }

                Configuration.WatcherGroups = watcherGroups;

                return this;
            }

            /// <summary>
            /// Sets the custom JSON serializer settings of the Newtonsoft.Json library.
            /// </summary>
            /// <param name="jsonSerializerSettings">Custom JSON serializer settings of the Newtonsoft.Json library.</param>
            /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
            public Builder WithJsonSerializerSettings(JsonSerializerSettings jsonSerializerSettings)
            {
                if (jsonSerializerSettings == null)
                {
                    throw new ArgumentNullException(nameof(jsonSerializerSettings),
                        "JSON serializer settings can not be null.");
                }


                Configuration.JsonSerializerSettings = jsonSerializerSettings;

                return this;
            }

            /// <summary>
            /// Provider for the custom DateTime (UTC by default).
            /// </summary>
            /// <param name="dateTimeProvider">Custom DateTime provider.</param>
            /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
            public Builder WithDateTimeProvider(Func<DateTime> dateTimeProvider)
            {
                Configuration.DateTimeProvider = dateTimeProvider;

                return this;
            }

            /// <summary>
            /// Sets the custom provider for the ICachetService.
            /// </summary>
            /// <param name="cachetServiceProvider">Custom provider for the ICachetService.</param>
            /// <returns>Lambda expression returning an instance of the ICachetService.</returns>
            /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
            public Builder WithCachetServiceProvider(Func<ICachetService> cachetServiceProvider)
            {
                if (cachetServiceProvider == null)
                {
                    throw new ArgumentNullException(nameof(cachetServiceProvider),
                        "Cachet service provider can not be null.");
                }

                Configuration.CachetServiceProvider = cachetServiceProvider;

                return this;
            }

            /// <summary>
            /// Builds the CachetIntegrationConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of CachetIntegrationConfiguration.</returns>
            public CachetIntegrationConfiguration Build() => Configuration;
        }
    }
}