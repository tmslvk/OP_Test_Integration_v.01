namespace BPMSoft.Configuration.OPCarsBaseIntegration.Logger
{
    using BPMSoft.Common;
    using BPMSoft.Configuration.Logger;
    using BPMSoft.Configuration.OPCarsBaseIntegrationModels.Exceptions;
    using BPMSoft.Configuration.Validation;
    using BPMSoft.Core;
    using System;
    using System.Collections.Generic;

    internal class OPCarsBaseIntegrationLoggerImpl : OPBaseIntegrationLogger
    {
        #region Fields : Public

        public override string SchemaName => "OPCarsBaseIntegrationLog";

        #endregion

        #region Methods : Public

        public override void LogError(UserConnection connection, Guid logId, Exception exception, bool withStackTrace = false)
        {
            if (logId.IsEmpty())
                logId = CreateLog(connection);

            var errorText = $"{exception.Message}";
            var errorType = exception.GetType().Name;
            if (exception is RequestProcessingException requestException)
            {
                errorText = $"[{requestException.IdCarsBase}]: {requestException.Message}";
                errorType = requestException.ExceptionType.Name;
            }
            if (withStackTrace)
                errorText += $"\nStackTrace:\n{exception.StackTrace}";

            connection.UpdateEntityById(SchemaName, logId, new Dictionary<string, object>()
            {
                { "OPErrorText", errorText },
                { "OPErrorType", errorType }
            });
        }

        public virtual void LogOPError(UserConnection connection, Guid logId, OPError exception)
        {
            if (logId.IsEmpty())
                logId = CreateLog(connection);

            var errorText = $"{exception.Message}";
            var errorType = exception.Code;

            connection.UpdateEntityById(SchemaName, logId, new Dictionary<string, object>()
            {
                { "OPErrorText", errorText },
                { "OPNotes", errorType }
            });
        }

        #endregion
    }

    public static class OPCarsBaseIntegrationLogger
    {
        #region Fields : Private

        private static readonly OPBaseIntegrationLogger Impl = new OPCarsBaseIntegrationLoggerImpl();

        #endregion

        #region Methods : Public

        public static Guid LogRequest(UserConnection connection, string methodName, object request) =>
            Impl.LogRequest(connection, methodName, request);

        public static void LogResponse(UserConnection connection, Guid logId, object response) =>
            Impl.LogResponse(connection, logId, response);

        public static void LogError(UserConnection connection, Guid logId, Exception exception, bool withStackTrace = false) =>
            Impl.LogError(connection, logId, exception, withStackTrace);

        #endregion
    }
} 