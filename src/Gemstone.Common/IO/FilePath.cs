//******************************************************************************************************
//  FilePath.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/05/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/29/2005 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (GSF.Shared.FilePath).
//  08/22/2007 - Darrell Zuercher
//       Edited code comments.
//  09/19/2008 - J. Ritchie Carroll
//       Converted to C#.
//  10/24/2008 - Pinal C. Patel
//       Edited code comments.
//  12/17/2008 - F. Russell Robertson
//       Fixed issue in GetFilePatternRegularExpression().
//  06/30/2009 - Pinal C. Patel
//       Removed FilePathHasFileName() since the result was error prone.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/17/2009 - Pinal C. Patel
//       Modified GetAbsolutePath() to remove dependency on HttpContext.Current.
//  04/19/2010 - Pinal C. Patel
//       Added GetApplicationDataFolder() method.
//  04/21/2010 - Pinal C. Patel
//       Updated GetApplicationDataFolder() to include the company name if available.
//  01/28/2011 - J. Ritchie Carroll
//       Added IsValidFileName function.
//  02/14/2011 - J. Ritchie Carroll
//       Fixed issue in GetDirectoryName where last directory was being truncated as a file name.
//  06/06/2011 - Stephen C. Wills
//       Fixed issue in GetFileName where path suffix was being removed before extracting the file name.
//  07/29/2011 - Pinal C. Patel
//       Updated GetApplicationDataFolder() to use the TEMP directory for web applications.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

namespace Gemstone.IO;

/// <summary>
/// Contains File and Path manipulation methods.
/// </summary>
public static class FilePath
{
    /// <summary>
    /// Gets the file name without extension from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path from which the file name is to be obtained.</param>
    /// <returns>File name without the extension if the file path has it; otherwise empty string.</returns>
    public static string GetFileNameWithoutExtension(string filePath)
    {
        return Path.GetFileNameWithoutExtension(RemovePathSuffix(filePath));
    }


    /// <summary>
    /// Makes sure path is not suffixed with <see cref="Path.DirectorySeparatorChar"/> or <see cref="Path.AltDirectorySeparatorChar"/>.
    /// </summary>
    /// <param name="filePath">The file path to be unsuffixed.</param>
    /// <returns>Unsuffixed path.</returns>
    public static string RemovePathSuffix(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            filePath = "";
        }
        else
        {
            char suffixChar = filePath[^1];

            while ((suffixChar == Path.DirectorySeparatorChar || suffixChar == Path.AltDirectorySeparatorChar) && filePath.Length > 0)
            {
                filePath = filePath[..^1];

                if (filePath.Length > 0)
                    suffixChar = filePath[^1];
            }
        }

        return filePath;
    }

    /// <summary>
    /// Gets the directory information from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path from which the directory information is to be obtained.</param>
    /// <returns>Directory information.</returns>
    /// <remarks>
    /// This differs from <see cref="Path.GetDirectoryName(string)"/> in that it will see if last name in path is
    /// a directory and, if it exists, will properly treat that part of path as a directory. The .NET path
    /// function always assumes last entry is a file name if path is not suffixed with a slash. For example:
    ///     Path.GetDirectoryName(@"C:\Music") will return "C:\", however, 
    /// FilePath.GetDirectoryName(@"C:\Music") will return "C:\Music\", so long as Music directory exists.
    /// </remarks>
    public static string GetDirectoryName(string filePath)
    {
        // Test for case where valid path does not end in directory separator, Path.GetDirectoryName assumes
        // this is a file name - whether is exists or not :-(
        string directoryName = AddPathSuffix(filePath);

        return Directory.Exists(directoryName) ? directoryName : AddPathSuffix(Path.GetDirectoryName(filePath) ?? filePath);
    }

    /// <summary>
    /// Makes sure path is suffixed with standard <see cref="Path.DirectorySeparatorChar"/>.
    /// </summary>
    /// <param name="filePath">The file path to be suffixed.</param>
    /// <returns>Suffixed path.</returns>
    public static string AddPathSuffix(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            filePath = "." + Path.DirectorySeparatorChar;
        }
        else
        {
            char suffixChar = filePath[^1];

            if (suffixChar != Path.DirectorySeparatorChar && suffixChar != Path.AltDirectorySeparatorChar)
                filePath += Path.DirectorySeparatorChar;
        }

        return filePath;
    }

    /// <summary>
    /// Gets a list of files under the specified path. Search wild card pattern (c:\Data\*.dat) can be used for 
    /// including only the files matching the pattern or path wild-card pattern (c:\Data\*\*.dat) to indicate the 
    /// inclusion of files under all subdirectories in the list.
    /// </summary>
    /// <param name="path">The path for which a list of files is to be returned.</param>
    /// <param name="exceptionHandler">Handles exceptions thrown during file enumeration.</param>
    /// <returns>A list of files under the given path.</returns>
    public static string[] GetFileList(string path, Action<Exception>? exceptionHandler = null)
    {
        string directory = GetDirectoryName(path);
        string filePattern = GetFileName(path);
        SearchOption options = SearchOption.TopDirectoryOnly;

        // No wild-card pattern was specified, so get a listing of all files.
        if (string.IsNullOrEmpty(filePattern))
            filePattern = "*.*";

        if (GetLastDirectoryName(directory) == "*")
        {
            // Path wild-card pattern is used to specify the option to include subdirectories.
            options = SearchOption.AllDirectories;
            directory = directory.Remove(directory.LastIndexOf("*", StringComparison.OrdinalIgnoreCase));
        }

        if (exceptionHandler is null)
            return Directory.GetFiles(directory, filePattern, options);

        return GetFiles(directory, filePattern, options, exceptionHandler);
    }

    /// <summary>
    /// Gets the file name and extension from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path from which the file name and extension is to be obtained.</param>
    /// <returns>File name and extension if the file path has it; otherwise empty string.</returns>
    public static string GetFileName(string filePath)
    {
        return Path.GetFileName(filePath);
    }

    /// <summary>
    /// Gets the last directory name from a file path.
    /// </summary>
    /// <param name="filePath">The file path from where the last directory name is to be retrieved.</param>
    /// <returns>The last directory name from a file path.</returns>
    /// <remarks>
    /// <see cref="GetLastDirectoryName(string)"/> would return sub2 from c:\windows\sub2\filename.ext.
    /// </remarks>
    public static string GetLastDirectoryName(string filePath)
    {
        // Test case should verify the following:
        //   FilePath.GetLastDirectoryName(@"C:\Test\sub") == "Test" <-- sub assumed to be filename
        //   FilePath.GetLastDirectoryName(@"C:\Test\sub\") == "sub" <-- sub assumed to be directory

        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

        int index;
        char[] dirVolChars = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar };

        // Remove file name and trailing directory separator character from the file path.
        filePath = RemovePathSuffix(GetDirectoryName(filePath));

        // Keep going through the file path until all directory separator characters are removed.
        while ((index = filePath.IndexOfAny(dirVolChars)) > -1)
            filePath = filePath[(index + 1)..];

        return filePath;
    }

    /// <summary>
    /// Returns the names of files (including their paths) that match the specified search pattern in the specified directory, using a value to determine whether to search subdirectories.
    /// </summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <param name="searchPattern">The search string to match against the names of files in <paramref name="path"/>. This parameter can contain a combination of valid literal path and wild-card (* and ?) characters, but doesn't support regular expressions.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
    /// <param name="exceptionHandler">Handles exceptions thrown during file enumeration.</param>
    /// <returns>An array of the full names (including paths) for the files in the specified directory that match the specified search pattern and option, or an empty array if no files are found.</returns>
    public static string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories, Action<Exception>? exceptionHandler = null)
    {
        return EnumerateFiles(path, searchPattern, searchOption, exceptionHandler).ToArray();
    }

    /// <summary>
    /// Returns an enumerable collection of file names that match a search pattern in a specified path, and optionally searches subdirectories.
    /// </summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <param name="searchPattern">The search string to match against the names of files in <paramref name="path"/>. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but doesn't support regular expressions.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.</param>
    /// <param name="exceptionHandler">Handles exceptions thrown during file enumeration.</param>
    /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path"/> and that match the specified search pattern and option.</returns>
    public static IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories, Action<Exception>? exceptionHandler = null)
    {
        IEnumerable<string> enumerable;
        IEnumerator<string> enumerator;

        void handleException(Exception ex)
        {
            InvalidOperationException enumerationEx = new($"Failed while enumerating files in \"{path}\": {ex.Message}", ex);

            if (exceptionHandler is null)
                LibraryEvents.OnSuppressedException(typeof(FilePath), enumerationEx);
            else
                exceptionHandler(enumerationEx);
        }

        try
        {
            IEnumerable<string> topDirectory = Directory.EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
            IEnumerable<string> recursive = Enumerable.Empty<string>();

            if (searchOption == SearchOption.AllDirectories)
            {
                recursive = Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly)
                    .SelectMany(directory => EnumerateFiles(directory, searchPattern, searchOption, exceptionHandler));
            }

            enumerable = topDirectory.Concat(recursive);
            enumerator = enumerable.GetEnumerator();
        }
        catch (Exception ex)
        {
            handleException(ex);
            yield break;
        }

        // yield return cannot be used in a try block with a catch clause,
        // so in order to handle errors in enumerator.MoveNext() and enumerator.Current,
        // the enumerator must be accessed directly rather than using foreach
        using (enumerable as IDisposable)
        using (enumerator)
        {
            while (true)
            {
                string? current;

                try
                {
                    if (!enumerator.MoveNext())
                        break;

                    current = enumerator.Current;
                }
                catch (Exception ex)
                {
                    handleException(ex);

                    // To avoid an infinite exception loop,
                    // break out at the first sign of trouble
                    break;
                }

                if (current is not null)
                    yield return current;
            }
        }
    }

    /// <summary>
    /// Gets the extension from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path from which the extension is to be obtained.</param>
    /// <returns>File extension.</returns>
    public static string GetExtension(string filePath)
    {
        return Path.GetExtension(RemovePathSuffix(filePath));
    }
}
