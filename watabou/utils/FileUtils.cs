using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace watabou.utils
{
    // gdx
    public enum FileType
    {
        /** Path relative to the root of the classpath. Classpath files are always readonly. Note that classpath files are not
         * compatible with some functionality on Android, such as {@link Audio#newSound(FileHandle)} and
         * {@link Audio#newMusic(FileHandle)}. */
        Classpath,

        /** Path relative to the asset directory on Android and to the application's root directory on the desktop. On the desktop,
         * if the file is not found, then the classpath is checked. This enables files to be found when using JWS or applets.
         * Internal files are always readonly. */
        Internal,

        /** Path relative to the root of the SD card on Android and to the home directory of the current user on the desktop. */
        External,

        /** Path that is a fully qualified, absolute filesystem path. To ensure portability across platforms use absolute files only
         * when absolutely (heh) necessary. */
        Absolute,

        /** Path relative to the private files directory on Android and to the application's root directory on the desktop. */
        Local
    }

    // 참조:
    // osu-framework-master\osu.Framework\Platform\NativeStorage.cs

    public class FileUtils
    {
        // Helper methods for setting/using a default base path and file address mode

        private static FileType defaultFileType = FileType.External;
        private static string defaultPath = "";

        public static void SetDefaultFileProperties(FileType type, string path)
        {
            defaultFileType = type;
            defaultPath = path;
        }

        //public static FileHandle getFileHandle(string name)
        //{
        //	return getFileHandle(defaultFileType, defaultPath, name);
        //}
        //
        //public static FileHandle getFileHandle(FileType type, string name)
        //{
        //	return getFileHandle(type, "", name);
        //}
        //
        //public static FileHandle getFileHandle(FileType type, string basePath, string name)
        //{
        //	switch (type)
        //	{
        //		case Classpath:
        //			return Gdx.files.classpath(basePath + name);
        //		case Internal:
        //			return Gdx.files.internal (basePath + name );
        //		case External:
        //			return Gdx.files.external(basePath + name );
        //		case Absolute:
        //			return Gdx.files.absolute(basePath + name );
        //		case Local:
        //			return Gdx.files.local(basePath + name );
        //		default:
        //			return null;
        //	}
        //}

        // path는 파일이름
        public static string GetFullPath(string path, bool createIfNotExisting = false)
        {
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            var basePath = Path.GetFullPath(defaultPath).TrimEnd(Path.DirectorySeparatorChar);
            var resolvedPath = Path.GetFullPath(Path.Combine(basePath, path));

            if (!resolvedPath.StartsWith(basePath))
                throw new ArgumentException($"\"{resolvedPath}\" traverses outside of \"{basePath}\" and is probably malformed");

            if (createIfNotExisting)
                Directory.CreateDirectory(Path.GetDirectoryName(resolvedPath));

            return resolvedPath;
        }

        // Files

        public static bool FileExists(string name)
        {
            //FileHandle file = getFileHandle(name);
            //return file.exists() && !file.isDirectory();

            return File.Exists(GetFullPath(name));
        }

        public static bool DeleteFile(string name)
        {
            //return getFileHandle(name).delete();

            name = GetFullPath(name);

            if (File.Exists(name))
            {
                try
                {
                    File.Delete(name);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        // Directories

        public static bool DirExists(string name)
        {
            //FileHandle dir = getFileHandle(name);
            //return dir.exists() && dir.isDirectory();

            return Directory.Exists(GetFullPath(name));
        }

        public static bool DeleteDir(string name)
        {
            //FileHandle dir = getFileHandle(name);
            //
            //if (dir == null || !dir.isDirectory())
            //{
            //	return false;
            //}
            //else
            //{
            //	return dir.deleteDirectory();
            //}

            name = GetFullPath(name);

            // handles the case where the directory doesn't exist, which will throw a DirectoryNotFoundException.
            if (Directory.Exists(name))
            {
                try
                {
                    Directory.Delete(name, true);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        // bundle reading

        //only works for base path
        public static Bundle BundleFromFile(string fileName)
        {
            //try {
            //	FileHandle file = getFileHandle(fileName);
            //	return bundleFromStream(file.read());
            //} catch (Exception e){
            //	//game classes expect an IO exception, so wrap the GDX exception in that
            //	throw new IOException(e);
            //}

            try
            {
                string str = Read(fileName);
                JObject obj = JObject.Parse(str);
                return new Bundle(obj);
            }
            catch (Exception e)
            {
                throw new IOException(e.ToString());
            }
        }

        public static string Read(string fileName)
        {
            fileName = Path.Combine(defaultPath, fileName);

            string str;
            try
            {
                using (var stream = File.OpenRead(fileName))
                {
                    using (var sr = new StreamReader(stream))
                    {
                        str = sr.ReadToEnd();
                    }
                    stream.Close();
                }
            }
            catch (Exception)
            {
                //CA2200: 스택 정보를 유지하도록 다시 throw하십시오.
                throw;
            }

            return str;
        }

        // bundle writing

        //only works for base path
        public static void BundleToFile(string fileName, Bundle bundle)
        {
            //try {
            //    bundleToStream(getFileHandle( fileName ).write(false), bundle);
            //} catch (GdxRuntimeException e){
            //    //game classes expect an IO exception, so wrap the GDX exception in that
            //    throw new IOException(e);
            //}             

            //Write(fileName, bundle.data.ToString(Formatting.None));
            Write(fileName, bundle.data.ToString());
        }

        public static void Write(string fileName, string str)
        {
            fileName = Path.Combine(defaultPath, fileName);

            try
            {
                var destinationDirectory = new DirectoryInfo(Path.GetDirectoryName(fileName));
                if (!destinationDirectory.Exists)
                    destinationDirectory.Create();

                using (var stream = File.Open(fileName, FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(str);
                        writer.Close();
                    }
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                //game classes expect an IO exception, so wrap the GDX exception in that
                throw new IOException(e.ToString());
            }
        }
    }
}
