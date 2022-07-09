using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QekaCrypt
{
    public class ResultReadyEventArgs : EventArgs
    {
        public readonly string Result;

        public ResultReadyEventArgs(string result)
        {
            Result = result;
        }
    }
}
