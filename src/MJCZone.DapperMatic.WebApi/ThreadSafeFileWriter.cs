namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Provides thread-safe file read and write operations.
/// </summary>
/// <remarks>
/// This class uses a Mutex to ensure that only one thread can read or write to a file at a time.
/// Adapted from https://briancaos.wordpress.com/2022/06/16/c-thread-safe-file-writer-and-reader/.
/// </remarks>
internal class ThreadSafeFileWriter
{
    /// <summary>
    /// Reads the contents of a file in a thread-safe manner.
    /// </summary>
    /// <param name="filePathAndName">The full path and name of the file to read.</param>
    /// <returns>The contents of the file as a string. Returns an empty string if the file does not exist.</returns>
    public static string ReadFile(string filePathAndName)
    {
        // This block will be protected area
        using var mutex = new Mutex(
            false,
            filePathAndName.Replace("\\", string.Empty, StringComparison.Ordinal)
        );
        var hasHandle = false;
        try
        {
            // Wait for the muted to be available
            hasHandle = mutex.WaitOne(Timeout.Infinite, false);
            // Do the file read
            return !File.Exists(filePathAndName) ? string.Empty : File.ReadAllText(filePathAndName);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            // Very important! Release the mutex
            // Or the code will be locked forever
            if (hasHandle)
            {
                mutex.ReleaseMutex();
            }
        }
    }

    /// <summary>
    /// Reads the contents of a file in a thread-safe manner.
    /// </summary>
    /// <param name="filePathAndName">The full path and name of the file to read.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// /// <returns>The contents of the file as a string. Returns an empty string if the file does not exist.</returns>
    public static async Task<string> ReadFileAsync(
        string filePathAndName,
        CancellationToken cancellationToken = default
    )
    {
        // This block will be protected area
        using var mutex = new Mutex(
            false,
            filePathAndName.Replace("\\", string.Empty, StringComparison.Ordinal)
        );
        var hasHandle = false;
        try
        {
            // Wait for the muted to be available
            hasHandle = mutex.WaitOne(Timeout.Infinite, false);
            // Do the file read
            return !File.Exists(filePathAndName)
                ? string.Empty
                : await File.ReadAllTextAsync(filePathAndName, cancellationToken)
                    .ConfigureAwait(false);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            // Very important! Release the mutex
            // Or the code will be locked forever
            if (hasHandle)
            {
                mutex.ReleaseMutex();
            }
        }
    }

    /// <summary>
    /// Writes the specified contents to a file in a thread-safe manner.
    /// </summary>
    /// <param name="fileContents">The contents to write to the file.</param>
    /// <param name="filePathAndName">The full path and name of the file to write.</param>
    public static void WriteFile(string fileContents, string filePathAndName)
    {
        using var mutex = new Mutex(
            false,
            filePathAndName.Replace("\\", string.Empty, StringComparison.Ordinal)
        );
        var hasHandle = false;
        try
        {
            hasHandle = mutex.WaitOne(Timeout.Infinite, false);
            if (!EnsureFileExists(filePathAndName, true))
            {
                return;
            }

            File.WriteAllText(filePathAndName, fileContents);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            if (hasHandle)
            {
                mutex.ReleaseMutex();
            }
        }
    }

    /// <summary>
    /// Writes the specified contents to a file in a thread-safe manner.
    /// </summary>
    /// <param name="fileContents">The contents to write to the file.</param>
    /// <param name="filePathAndName">The full path and name of the file to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task WriteFileAsync(
        string fileContents,
        string filePathAndName,
        CancellationToken cancellationToken = default
    )
    {
        using var mutex = new Mutex(
            false,
            filePathAndName.Replace("\\", string.Empty, StringComparison.Ordinal)
        );
        var hasHandle = false;
        try
        {
            hasHandle = mutex.WaitOne(Timeout.Infinite, false);
            if (!EnsureFileExists(filePathAndName, true))
            {
                return;
            }

            await File.WriteAllTextAsync(filePathAndName, fileContents, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            if (hasHandle)
            {
                mutex.ReleaseMutex();
            }
        }
    }

    private static bool EnsureFileExists(string filePathAndName, bool createIfNotExists)
    {
        if (!File.Exists(filePathAndName))
        {
            if (!createIfNotExists)
            {
                return false;
            }

            var directory = Path.GetDirectoryName(filePathAndName);
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Path.GetDirectoryName(Path.GetFullPath(filePathAndName));
            }

            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new InvalidOperationException("Invalid file path.");
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            filePathAndName = Path.Combine(directory, Path.GetFileName(filePathAndName));

            using var fs = File.Create(filePathAndName);
            fs.Close();
        }

        return true;
    }
}
