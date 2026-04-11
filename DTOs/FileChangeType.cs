namespace FileSynchronizer.DTOs;

public enum FileChangeType
{
    None = 0,
    Same = 1,
    New = 2,
    Modified = 3,
    Deleted = 4,
    Renamed = 5,
}
