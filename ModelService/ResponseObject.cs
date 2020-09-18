using System;
using System.Collections.Generic;
using System.Text;

namespace ModelService
{
    public class ResponseObject
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public dynamic Data { get; set; }
    }
}
