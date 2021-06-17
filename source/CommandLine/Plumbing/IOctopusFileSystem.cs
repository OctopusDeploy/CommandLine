using System;

namespace Octopus.CommandLine.Plumbing
{
    public interface IOctopusFileSystem
    {
        bool FileExists(string path);
        string ReadAllText(string scriptFile);
        void CopyFile(string sourceFile, string targetFile, int overwriteFileRetryAttempts = 3);
        void OverwriteFile(string path, string contents);
        void EnsureDirectoryExists(string directoryPath);
    }
}
