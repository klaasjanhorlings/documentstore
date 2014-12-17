using System;
using System.IO;

namespace Documents.Storage
{
    public interface IBlobStore
    {
        void Delete(byte[] hash);
        Stream Get(byte[] hash);
        byte[] Store(Stream stream);
    }
}
