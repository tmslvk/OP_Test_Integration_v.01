namespace BPMSoft.Configuration.Logger
{
    using BPMSoft.Common;
    using BPMSoft.Configuration.TelephonyCallRecordService;
    using BPMSoft.Core;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security.Policy;

    public abstract class OPBaseIntegrationLogger
    {
        #region Fields : Public 

        public abstract string SchemaName { get; }

        #endregion

        #region Methods : Protected

        protected string ProcessData(object data)
        {
            if (data is string str)
            {
                try
                {
                    var parsed = JsonConvert.DeserializeObject(str);
                    return JsonConvert.SerializeObject(parsed, Formatting.None, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                }
                catch
                {
                    return str;
                }
            }
            else
            {
                return JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
            }
        }
        protected string GenerateLogName(string methodName)
        {
            return $"Log_{methodName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
        }

        protected Guid CreateLog(UserConnection connection, string methodName)
        {
            return connection.CreateEntity(SchemaName, new Dictionary<string, object>()
            {
                { "OPName", GenerateLogName(methodName) }
            })?.PrimaryColumnValue ?? Guid.Empty;
        }

        #endregion

        public virtual void LogError(UserConnection connection, Guid logId, Exception exception, bool withStackTrace = false)
        {
            if (logId.IsEmpty())
                logId = CreateLog(connection, "AutoCreated");

            var errorText = $"{exception.Message}";
            if (withStackTrace)
                errorText += $"\nStackTrace:\n{exception.StackTrace}";

            connection.UpdateEntityById(SchemaName, logId, new Dictionary<string, object>()
            {
                { "OPErrorText", errorText },
                { "OPErrorType",  exception.GetType().Name }
            });
        }

        public virtual Guid StartRequest(UserConnection connection, string methodName, string url, object request = null)
        {
            return connection.CreateEntity(SchemaName, new Dictionary<string, object>()
                {
                    { "OPName", GenerateLogName(methodName) },
                    { "OPMethodName", methodName },
                    { "OPUrl", url },
                    { "OPRequestBody", ProcessData(request) },
                })?.PrimaryColumnValue ?? Guid.Empty;
        }

        public virtual void CompleteResponse(UserConnection connection, Guid logId, string methodName, object response)
        {
            if (logId.IsEmpty())
                logId = CreateLog(connection, "AutoCreated");

            connection.UpdateEntityById(SchemaName, logId, new Dictionary<string, object>()
            {
                { "OPResponseBody", ProcessData(response) },
                { "OPName", $"Log_{methodName}_SUCCESS_{DateTime.UtcNow:yyyyMMdd_HHmmss}" }
            });
        }
    }
}