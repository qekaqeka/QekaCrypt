using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QekaCrypt
{
    public class CryptEventArgs : EventArgs
    {
        public readonly byte[] Data;
        public readonly long TotalSize;
        public readonly ActionsOnData ActionsOnData;

        public CryptEventArgs(byte[] data, long totalSize, ActionsOnData actionsOnData)
        {
            Data = data;
            TotalSize = totalSize;
            ActionsOnData = actionsOnData;
        }
    }
}
