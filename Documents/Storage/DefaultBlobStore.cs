using System;
using System.IO;
using System.Security.Cryptography;

namespace Documents.Storage
{
    public sealed class DefaultBlobStore : IBlobStore
    {
        private IPathStrategy PathStrategy;

        public DefaultBlobStore(IPathStrategy pathStrategy)
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
            var tempFilePath = PathStrategy.GetTemporaryPath();

            // Write stream to temporary file while generating a hash
            var algo = new SHA1CryptoServiceProvider();
            using (var cryptoStream = new CryptoStream(stream, algo, CryptoStreamMode.Read))
            {
                using (var fileStream = File.OpenWrite(tempFilePath))
                {
                    cryptoStream.CopyTo(fileStream);
                    fileStream.Close();
                }
            }

            // Read out hash and release hash algorithm
            var hash = algo.Hash;
            algo.Clear();

            // And move it to it's final destination
            var destination = PathStrategy.GetPath(hash);
            File.Move(tempFilePath, destination);

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
