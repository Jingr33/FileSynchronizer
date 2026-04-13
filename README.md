# File Synchronizer

File Synchronizer is a background CLI application that provides one-way file and folder synchronization based on metadata or cryptographic hashes.

## Running the Application
The application can be started from the command line using the .NET CLI (`dotnet run`) from the source directory, or by executing the compiled `FileSynchronizer.exe` file. The application must always be executed with arguments to define its behavior.

## Arguments
* `-l` (`--logFile`) **(Required)**: Specifies the absolute or relative path to the output file where the application will log all its activities.
* `-b` (`--backupInterval`) **(Optional)**: Interval for running the **lightweight metadata synchronization**. Specified as a combination of days, hours, and minutes abbreviations (e.g., `15m`, `2h`, `1d12h`). The configuration must always represent a positive integer number of minutes.
* `-d` (`--deepBackupInterval`) **(Optional)**: Interval for running the **deep hash-based synchronization**. The format is identical to the metadata interval.

Execution example (dotnet cli):
`dotnet run -- -l "logs\app.log" -b "15m"`

Execution example (compiled EXE):
`.\FileSynchronizer.exe -l "app.log" -b "15m" -d "1d12h"`

---

## Application Behavior and Specifics

### Directory Structure
The application strictly depends on specific folder naming conventions within the directory from which it is executed. It exclusively tracks a folder named `source` and replicates its contents into a folder named `replica`.
The `source` directory must exist before starting the application. The `replica` folder is created automatically if missing.

### Scheduling
Interval arguments (`-b`, `-d`) determine future automatic recurring synchronization loops. 
If both period arguments are omitted from the command application terminates immediately. If the user specifies at least one preiod argument, the web server with Hangfire is initialized and the task administration interface can be accessed in a web browser at `http://localhost:5000/hangfire`.

### Comparison Modes (Metadata vs. Hash)
* **Metadata Synchronization** (`-b` argument): Compares files based on their "last modified" timestamp and size. It is fast but might miss changes in specific edge cases where the system timestamp does not change.
* **Hash Synchronization** (`-d` argument): Compares files by reading their actual content and calculating a SHA-256 hash. It is slower and computationally heavier, but accurate in detecting any byte-level changes and renamed files.

### Initial Execution
Upon startup, the first synchronization runs immediately. If both intervals are specified, metadata synchronization takes precedence.