using System;
using System.Runtime.Serialization;

namespace BPMSoft.Configuration.Validation
{
    [DataContract]
    public class OPError
    {
        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string Message { get; set; }

        public OPError() { }

        private OPError(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public static OPError Create(string code, string message)
            => new OPError(code, message);

    }

    public static class OPErrors
    {
        public static class API
        {
            public static OPError InvalidApiToken()
                => General.ValueIsInvalid("ApiToken");


            public static OPError InvalidApiUrl()
                => General.ValueIsInvalid("ApiUrl");
        }

        public static class General
        {
            public static OPError ValueIsInvalid(string name = null)
            {
                var label = name ?? "value";
                return OPError.Create("400", $"{label} is invalid");
            }

            public static OPError NotFound(Guid? id = null)
            {
                var forId = id == null ? "" : $"for id: {id.Value}";
                return OPError.Create("400", $"Record not found {forId}");
            }

            public static OPError Fatal(string message)
            {
                return OPError.Create("500", message);
            }
        }
    }
}