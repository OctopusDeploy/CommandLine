using System;
using System.IO;
using System.Threading;

namespace Octopus.CommandLine.Plumbing
{
    internal interface IOctopusFileSystem
    {
        bool FileExists(string path);
        string ReadAllText(string scriptFile);
        void CopyFile(string sourceFile, string targetFile, int overwriteFileRetryAttempts = 3);
        void OverwriteFile(string path, string contents);
        void EnsureDirectoryExists(string directoryPath);
    }

    class OctopusFileSystem : IOctopusFileSystem
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void CopyFile(string sourceFile, string targetFile, int overwriteFileRetryAttempts = 3)
        {
            for (var i = 0; i < overwriteFileRetryAttempts; i++)
            {
                try
                {
                    FileInfo fi = new FileInfo(targetFile);
                    if (fi.Exists)
                    {
                        if ((fi.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            fi.Attributes = fi.Attributes & ~FileAttributes.ReadOnly;
                        }
                    }
                    File.Copy(sourceFile, targetFile, true);
                }
                catch
                {
                    if (i == overwriteFileRetryAttempts - 1)
                        throw;
                    Thread.Sleep(1000 + (2000 * i));
                }
            }
            throw new Exception("Internal error, cannot get here");
        }

        public void OverwriteFile(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        public void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }
    }
}
