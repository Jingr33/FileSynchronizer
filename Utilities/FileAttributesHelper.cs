namespace FileSynchronizer.Utilities;

public static class FileAttributesHelper
{
    public static bool RemoveReadOnlyAttribute(string filePath)
    {
        var attributes = File.GetAttributes(filePath);
        if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
        {
            File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
            return true;
        }

        return false;
    }

    public static void AddReadOnlyAttribute(string filePath)
    {
        var attributes = File.GetAttributes(filePath);
        File.SetAttributes(filePath, attributes | FileAttributes.ReadOnly);
    }
}