namespace BPMSoft.Configuration.OPCarsBaseIntegrationModels
{
    using System;
    using System.Runtime.Serialization;
    using BPMSoft.Configuration.OPConstants;

    [DataContract]
    public class OPBaseIntegrationResponse
    {
        [DataMember]
        public string Message { get; set; } = string.Empty;
        [DataMember]
        public string Status { get; set; } = OPResponseStatus.OK;

        public Type ErrorType { get; set; }
    }

    public class OPBaseResultFromAPI
    {
        public string ResponseString { get; set; }
        public bool IsSuccess { get; set; }
    }
}

namespace BPMSoft.Configuration.OPCarsBaseIntegrationModels.Exceptions
{
    using System;
    public class ExceptionAPI : Exception
    {
        public ExceptionAPI(string message) : base(message) { }
    }

    public class MissingReferenceException : Exception
    {
        public MissingReferenceException(string message) : base(message) { }
    }

    public class RequestProcessingException : Exception
    {
        public string IdCarsBase { get; }
        public Type ExceptionType { get; }

        public RequestProcessingException(string idCarsBase, string message,
            Type exceptionType, Exception innerException) : base(message, innerException)
        {
            IdCarsBase = idCarsBase;
            ExceptionType = exceptionType;
        }
    }
}