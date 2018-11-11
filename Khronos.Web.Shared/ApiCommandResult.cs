using System;
using System.Collections.Generic;
using System.Text;

namespace Khronos.Web.Shared
{
    public abstract class ApiCommandResult
    {
        public bool Success { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
    }
}
