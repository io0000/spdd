using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace spdd.messages
{
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

    public class FileHandle
    {
        protected FileInfo file;
        protected FileType type;

        /** Creates a new absolute FileHandle for the file name. Use this for tools on the desktop that don't need any of the backends.
         * Do not use this constructor in case you write something cross-platform. Use the {@link Files} interface instead.
         * @param fileName the filename. */
        public FileHandle(string fileName)
        {
            this.file = new FileInfo(fileName);
            this.type = FileType.Absolute;
        }

        /** Creates a new absolute FileHandle for the {@link File}. Use this for tools on the desktop that don't need any of the
         * backends. Do not use this constructor in case you write something cross-platform. Use the {@link Files} interface instead.
         * @param file the file. */
        public FileHandle(FileInfo file)
        {
            this.file = file;
            this.type = FileType.Absolute;
        }

        protected FileHandle(string fileName, FileType type)
        {
            this.type = type;
            file = new FileInfo(fileName);
        }

        protected FileHandle(FileInfo file, FileType type)
        {
            this.file = file;
            this.type = type;
        }

        //public FileStream Read() 
        //{
        //    try 
        //    {
        //        return file.OpenRead();
        //    } 
        //    catch (Exception) 
        //    {
        //        throw;
        //    }
        //}

        public FileType Type()
        {
            return type;
        }

        /** Returns a java.io.File that represents this file handle. Note the returned file will only be usable for
         * {@link FileType#Absolute} and {@link FileType#External} file handles. */
        public FileInfo File()
        {
            //if (type == FileType.External) return new File(Gdx.files.getExternalStoragePath(), file.getPath());
            return file;
        }

        /** @return the name of the file, without any parent paths. */
        public string Name()
        {
            return file.Name;
        }
    }


    public class I18NBundle
    {
        private static CultureInfo ROOT_LOCALE = new CultureInfo("");

        private I18NBundle parent;

        private CultureInfo locale;

        private Dictionary<string, string> properties;

        public static I18NBundle CreateBundle(FileHandle baseFileHandle)
        {
            return CreateBundleImpl(baseFileHandle, CultureInfo.CurrentCulture);
        }

        public static I18NBundle CreateBundle(FileHandle baseFileHandle, CultureInfo locale)
        {
            return CreateBundleImpl(baseFileHandle, locale);
        }

        private static I18NBundle CreateBundleImpl(FileHandle baseFileHandle, CultureInfo locale)
        {
            I18NBundle bundle = null;
            I18NBundle baseBundle = null;
            CultureInfo targetLocale = locale;
            do
            {
                // Create the candidate locales
                List<CultureInfo> candidateLocales = GetCandidateLocales(targetLocale);

                // Load the bundle and its parents recursively
                bundle = LoadBundleChain(baseFileHandle, candidateLocales, 0, baseBundle);

                // Check the loaded bundle (if any)
                if (bundle != null)
                {
                    CultureInfo bundleLocale = bundle.GetLocale(); // WTH? GWT can't access bundle.locale directly
                    bool isBaseBundle = bundleLocale.Equals(ROOT_LOCALE);

                    if (!isBaseBundle || bundleLocale.Equals(locale))
                    {
                        // Found the bundle for the requested locale
                        break;
                    }
                    if (candidateLocales.Count == 1 && bundleLocale.Equals(candidateLocales[0]))
                    {
                        // Found the bundle for the only candidate locale
                        break;
                    }
                    if (isBaseBundle && baseBundle == null)
                    {
                        // Store the base bundle and keep on processing the remaining fallback locales
                        baseBundle = bundle;
                    }
                }

                // Set next fallback locale
                targetLocale = GetFallbackLocale(targetLocale);

            }
            while (targetLocale != null);

            if (bundle == null)
            {
                if (baseBundle == null)
                {
                    //// No bundle found
                    //throw new MissingResourceException("Can't find bundle for base file handle " + baseFileHandle.path() + ", locale "
                    //	+ locale, baseFileHandle + "_" + locale, "");
                    return null;
                }
                // Set the base bundle to be returned
                bundle = baseBundle;
            }

            return bundle;
        }


        private static List<CultureInfo> GetCandidateLocales(CultureInfo locale)
        {
            List<CultureInfo> locales = new List<CultureInfo>();
            locales.Add(locale);
            locales.Add(ROOT_LOCALE);
            return locales;
        }

        /** Returns a <code>CultureInfo</code> to be used as a fallback locale for further bundle searches by the <code>CreateBundle</code>
         * factory method. This method is called from the factory method every time when no resulting bundle has been found for
         * <code>baseFileHandler</code> and <code>locale</code>, where locale is either the parameter for <code>CreateBundle</code> or
         * the previous fallback locale returned by this method.
         * 
         * <p>
         * This method returns the {@linkplain CultureInfo#getDefault() default <code>CultureInfo</code>} if the given <code>locale</code> isn't
         * the default one. Otherwise, <code>null</code> is returned.
         * 
         * @param locale the <code>CultureInfo</code> for which <code>CreateBundle</code> has been unable to find any resource bundles
         *           (except for the base bundle)
         * @return a <code>CultureInfo</code> for the fallback search, or <code>null</code> if no further fallback search is needed.
         * @exception NullPointerException if <code>locale</code> is <code>null</code> */
        private static CultureInfo GetFallbackLocale(CultureInfo locale)
        {
            CultureInfo defaultLocale = CultureInfo.CurrentCulture;
            return locale.Equals(defaultLocale) ? null : defaultLocale;
        }

        private static I18NBundle LoadBundleChain(FileHandle baseFileHandle, List<CultureInfo> candidateLocales,
            int candidateIndex, I18NBundle baseBundle)
        {
            CultureInfo targetLocale = candidateLocales[candidateIndex];
            I18NBundle parent = null;
            if (candidateIndex != candidateLocales.Count - 1)
            {
                // Load recursively the parent having the next candidate locale
                parent = LoadBundleChain(baseFileHandle, candidateLocales, candidateIndex + 1, baseBundle);
            }
            else if (baseBundle != null && targetLocale.Equals(ROOT_LOCALE))
            {
                return baseBundle;
            }

            // Load the bundle
            I18NBundle bundle = LoadBundle(baseFileHandle, targetLocale);
            if (bundle != null)
            {
                bundle.parent = parent;
                return bundle;
            }

            return parent;
        }

        // Tries to load the bundle for the given locale.
        private static I18NBundle LoadBundle(FileHandle baseFileHandle, CultureInfo targetLocale)
        {
            I18NBundle bundle = null;

            try
            {
                FileHandle fileHandle = ToFileHandle(baseFileHandle, targetLocale);
                if (CheckFileExistence(fileHandle))
                {
                    // Instantiate the bundle
                    bundle = new I18NBundle();

                    // Load bundle properties from the stream with the specified encoding                   
                    bundle.Load(fileHandle.File().FullName);
                }
            }
            catch (IOException)
            {
                throw;
            }

            if (bundle != null)
            {
                bundle.SetLocale(targetLocale);
            }

            return bundle;
        }

        // On Android this is much faster than fh.exists(), see https://github.com/libgdx/libgdx/issues/2342
        // Also this should fix a weird problem on iOS, see https://github.com/libgdx/libgdx/issues/2345
        private static bool CheckFileExistence(FileHandle fh)
        {
            return fh.File().Exists;
        }

        /** Load the properties from the specified reader.
         * 
         * @param reader the reader
         * @throws IOException if an error occurred when reading from the input stream. */
        // NOTE:
        // This method can't be private otherwise GWT can't access it from loadBundle()
        protected void Load(string path)
        {
            properties = new Dictionary<string, string>();
            PropertiesUtils.Load(properties, path);
        }

        /** Converts the given <code>baseFileHandle</code> and <code>locale</code> to the corresponding file handle.
         * 
         * <p>
         * This implementation returns the <code>baseFileHandle</code>'s sibling with following value:
         * 
         * <pre>
         * baseFileHandle.name() + &quot;_&quot; + language + &quot;_&quot; + country + &quot;_&quot; + variant + &quot;.properties&quot;
         * </pre>
         * 
         * where <code>language</code>, <code>country</code> and <code>variant</code> are the language, country and variant values of
         * <code>locale</code>, respectively. Final component values that are empty Strings are omitted along with the preceding '_'.
         * If all of the values are empty strings, then <code>baseFileHandle.name()</code> is returned with ".properties" appended.
         * 
         * @param baseFileHandle the file handle to the base of the bundle
         * @param locale the locale for which a resource bundle should be loaded
         * @return the file handle for the bundle
         * @exception NullPointerException if <code>baseFileHandle</code> or <code>locale</code> is <code>null</code> */
        private static FileHandle ToFileHandle(FileHandle baseFileHandle, CultureInfo locale)
        {
            StringBuilder sb = new StringBuilder(baseFileHandle.Name());
            if (!locale.Equals(ROOT_LOCALE))
            {
                string language = locale.TwoLetterISOLanguageName;
                string country = "";// locale.getCountry();
                string variant = "";//locale.getVariant();
                bool emptyLanguage = "".Equals(language);
                bool emptyCountry = "".Equals(country);
                bool emptyVariant = "".Equals(variant);

                if (!(emptyLanguage && emptyCountry && emptyVariant))
                {
                    sb.Append('_');
                    if (!emptyVariant)
                    {
                        sb.Append(language).Append('_').Append(country).Append('_').Append(variant);
                    }
                    else if (!emptyCountry)
                    {
                        sb.Append(language).Append('_').Append(country);
                    }
                    else
                    {
                        sb.Append(language);
                    }
                }
            }

            var path = Path.Combine(baseFileHandle.File().DirectoryName, sb.Append(".properties").ToString());
            return new FileHandle(path);
        }

        /** Returns the locale of this bundle. This method can be used after a call to <code>CreateBundle()</code> to determine whether
         * the resource bundle returned really corresponds to the requested locale or is a fallback.
         * 
         * @return the locale of this bundle */
        public CultureInfo GetLocale()
        {
            return locale;
        }

        /** Sets the bundle locale. This method is private because a bundle can't change the locale during its life.
         * 
         * @param locale */
        private void SetLocale(CultureInfo locale)
        {
            this.locale = locale;
            //this.formatter = new TextFormatter(locale, !simpleFormatter);
        }

        /** Gets a string for the given key from this bundle or one of its parents.
         * 
         * @param key the key for the desired string
         * @exception NullPointerException if <code>key</code> is <code>null</code>
         * @exception MissingResourceException if no string for the given key can be found and {@link #getExceptionOnMissingKey()}
         *               returns {@code true}
         * @return the string for the given key or the key surrounded by {@code ???} if it cannot be found and
         *         {@link #getExceptionOnMissingKey()} returns {@code false} */
        public string Get(string key)
        {
            string result = null;
            properties.TryGetValue(key, out result);
            if (result == null)
            {
                if (parent != null)
                    result = parent.Get(key);

                if (result == null)
                {
                    //return "???" + key + "???";
                    return result;
                }
            }
            return result;
        }
    }
}
