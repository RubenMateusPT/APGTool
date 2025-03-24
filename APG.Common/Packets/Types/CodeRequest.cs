using System;
using System.Collections.Generic;
using System.Text;
using APG.Common.Commands;

namespace APG.Common.Packets.Types
{
    public class CodeRequest
    {
        public string GameName { get; set; }
        public char CommandDelimiter { get; set; }
        public Command[] Commands { get; set; }
    }
}
