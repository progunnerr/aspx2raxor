using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dlrsoft.AspxToRazorConverter.Dom
{
    public class CodeBlock : Block
    {
        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }
    }
}
