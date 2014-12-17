using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Documents.Storage
{
    public interface IPathStrategy
    {
        string GetPath(byte[] documentHash);
        string GetTemporaryPath();
    }
}
