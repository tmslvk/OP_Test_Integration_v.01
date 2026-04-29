namespace BPMSoft.Configuration.Logger
{
    using BPMSoft.Common;
    using BPMSoft.Configuration.TelephonyCallRecordService;
    using BPMSoft.Core;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

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

        protected Guid CreateLog(UserConnection connection)
        {
            return connection.CreateEntity(SchemaName, new Dictionary<string, object>() { })?.PrimaryColumnValue ?? Guid.Empty;
        }

        #endregion

        public virtual Guid LogRequest(UserConnection connection, string methodName, string url, object request)
        {
            return connection.CreateEntity(SchemaName, new Dictionary<string, object>()
            {
                { "OPUrl", url },
                { "OPMethodName", methodName },
                { "OPRequestBody", ProcessData(request) },
            })?.PrimaryColumnValue ?? Guid.Empty;
        }

        public virtual Guid LogRequest(UserConnection connection, string methodName, object request)
        {
            return connection.CreateEntity(SchemaName, new Dictionary<string, object>()
            {
                { "OPMethodName", methodName },
                { "OPRequestBody", ProcessData(request) },
            })?.PrimaryColumnValue ?? Guid.Empty;
        }

        public virtual void LogResponse(UserConnection connection, Guid logId, object response)
        {
            if (logId.IsEmpty())
                logId = CreateLog(connection);

            connection.UpdateEntityById(SchemaName, logId, new Dictionary<string, object>()
            {
                { "OPResponseBody", ProcessData(response) }
            });
        }

        public virtual void LogError(UserConnection connection, Guid logId, Exception exception, bool withStackTrace = false)
        {
            if (logId.IsEmpty())
                logId = CreateLog(connection);

            var errorText = $"{exception.Message}";
            if (withStackTrace)
                errorText += $"\nStackTrace:\n{exception.StackTrace}";

            connection.UpdateEntityById(SchemaName, logId, new Dictionary<string, object>()
            {
                { "OPErrorText", errorText },
                { "OPErrorType",  exception.GetType().Name }
            });
        }
    }
}