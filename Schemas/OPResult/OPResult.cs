
using System.Runtime.Serialization;

namespace BPMSoft.Configuration.Validation
{
    [DataContract(Name = "Result")]
    [KnownType(typeof(OPError))]
    public class OPResult<TValue, TError>
    {
        [DataMember(Name = "value")]
        public TValue Value { get; set; }

        [DataMember(Name = "error")]
        public TError Error { get; set; }

        [DataMember(Name = "isSuccess")]
        public bool IsSuccess { get; set; }

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