
using System.Runtime.Serialization;

namespace BPMSoft.Configuration.Validation
{
    [DataContract]
    public class OPResult<TValue, TError>
    {
        [DataMember]
        public TValue Value { get; set; }

        [DataMember]
        public TError Error { get; set; }

        [DataMember]
        public bool IsSuccess { get; set; }

        [DataMember]
        public bool IsFailure => !IsSuccess;

        public OPResult() { }

        private OPResult(TValue value)
        {
            IsSuccess = true;
            Value = value;
            Error = default;
        }

        private OPResult(TError error)
        {
            IsSuccess = false;
            Value = default;
            Error = error;
        }

        public static implicit operator OPResult<TValue, TError>(TValue value)
        => new OPResult<TValue, TError>(value);

        public static implicit operator OPResult<TValue, TError>(TError error)
            => new OPResult<TValue, TError>(error);
    }
}