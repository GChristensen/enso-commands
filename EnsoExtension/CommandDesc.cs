using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EnsoExtension
{
    public class CommandDesc
    {
        public Action<String, IEnsoService> action { get; set; }
        public String desc { get; set; }
        public String postfix { get; set; }
        public EnsoPostfixType postfixType { get; set; }
        public Func<String[]> getPostfixes { get; set; }
    };
}
