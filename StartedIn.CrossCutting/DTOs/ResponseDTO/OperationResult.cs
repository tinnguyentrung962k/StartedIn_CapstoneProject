using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public static OperationResult<T> SuccessResult(T data) => new OperationResult<T> { Success = true, Data = data };
        public static OperationResult<T> FailureResult(IEnumerable<string> errors) => new OperationResult<T> { Success = false, Errors = errors.ToList() };
    }
}
