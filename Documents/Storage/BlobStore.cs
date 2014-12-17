using System;
using System.IO;
using System.Security.Cryptography;

namespace Documents.Storage
{
    public class BlobStore : IBlobStore
    {
        private IPathStrategy PathStrategy;
        public HashAlgorithm HashAlgorithm = new SHA1CryptoServiceProvider();

        public BlobStore(IPathStrategy pathStrategy)
        {
            if (pathStrategy == null)
                throw new ArgumentNullException("pathStrategy");

            PathStrategy = pathStrategy;
        }

        public byte[] Store(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanRead)
                throw new ArgumentException("Stream must be readable");

            return _Store(stream);
        }

        private byte[] _Store(Stream stream)
        {
            var tempFile = PathStrategy.GetTemporaryPath();

            // Write stream to temporary file while generating a hash
            HashAlgorithm.Clear();
            using (var cryptoStream = new CryptoStream(stream, HashAlgorithm, CryptoStreamMode.Read))
            {
                using (var fileStream = File.OpenWrite(tempFile))
                {
                    cryptoStream.CopyTo(fileStream);
                    cryptoStream.FlushFinalBlock();
                }
            }
            var hash = HashAlgorithm.Hash;

            // And move it to it's final destination
            File.Move(tempFile, PathStrategy.GetPath(hash));

            return hash;
        }

        public Stream Get(byte[] hash)
        {
            var path = PathStrategy.GetPath(hash);
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            return File.OpenRead(path);
        }

        public void Delete(byte[] hash)
        {
            var path = PathStrategy.GetPath(hash);
            if (!File.Exists(path))
                throw new FileNotFoundException(path);
            
            File.Delete(path);
        }
    }
}
