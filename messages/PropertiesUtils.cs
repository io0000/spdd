using System;
using System.IO;
using System.Collections.Generic;

namespace spdd.messages
{
    /** {@code PropertiesUtils} is a helper class that allows you to load and store key/value pairs of an
     * {@code ObjectMap<string,string>} with the same line-oriented syntax supported by {@code java.util.Properties}. */
    public class PropertiesUtils
    {
        private const int NONE = 0, SLASH = 1, UNICODE = 2, CONTINUE = 3, KEY_DONE = 4, IGNORE = 5;

        private const string LINE_SEPARATOR = "\n";

        public PropertiesUtils()
        {
        }

        /** Adds to the specified {@code ObjectMap} the key/value pairs loaded from the {@code Reader} in a simple line-oriented format
         * compatible with <code>java.util.Properties</code>.
         * <p>
         * The input stream remains open after this method returns.
         *
         * @param properties the map to be filled.
         * @param reader the input character stream reader.
         * @throws IOException if an error occurred when reading from the input stream.
         * @throws IllegalArgumentException if a malformed Unicode escape appears in the input. */
        //public static void load (ObjectMap<string, string> properties, Reader reader)
        public static void Load(Dictionary<string, string> properties, string path)
        {
            int mode = NONE, unicode = 0, count = 0;
            char nextChar;
            char[] buf = new char[40];
            int offset = 0, keyLength = -1, intVal;
            bool firstChar = true;

            using (StreamReader br = new StreamReader(path))
            {
                while (true)
                {
                    intVal = br.Read();
                    if (intVal == -1)
                    {
                        break;
                    }
                    nextChar = (char)intVal;

                    if (offset == buf.Length)
                    {
                        char[] newBuf = new char[buf.Length * 2];
                        Array.Copy(buf, 0, newBuf, 0, offset);
                        buf = newBuf;
                    }
                    if (mode == UNICODE)
                    {
                        //int digit = Character.digit(nextChar, 16);
                        int digit = Convert.ToInt32(nextChar.ToString(), 16);
                        if (digit >= 0)
                        {
                            unicode = (unicode << 4) + digit;
                            if (++count < 4)
                            {
                                continue;
                            }
                        }
                        else if (count <= 4)
                        {
                            throw new ArgumentException("Invalid Unicode sequence: illegal character");
                        }
                        mode = NONE;
                        buf[offset++] = (char)unicode;
                        if (nextChar != '\n')
                        {
                            continue;
                        }
                    }
                    if (mode == SLASH)
                    {
                        mode = NONE;
                        switch (nextChar)
                        {
                            case '\r':
                                mode = CONTINUE; // Look for a following \n
                                continue;
                            case '\n':
                                mode = IGNORE; // Ignore whitespace on the next line
                                continue;
                            case 'b':
                                nextChar = '\b';
                                break;
                            case 'f':
                                nextChar = '\f';
                                break;
                            case 'n':
                                nextChar = '\n';
                                break;
                            case 'r':
                                nextChar = '\r';
                                break;
                            case 't':
                                nextChar = '\t';
                                break;
                            case 'u':
                                mode = UNICODE;
                                unicode = count = 0;
                                continue;
                        }
                    }
                    else
                    {
                        switch (nextChar)
                        {
                            case '#':
                            case '!':
                                if (firstChar)
                                {
                                    while (true)
                                    {
                                        intVal = br.Read();
                                        if (intVal == -1)
                                        {
                                            break;
                                        }
                                        nextChar = (char)intVal;
                                        if (nextChar == '\r' || nextChar == '\n')
                                        {
                                            break;
                                        }
                                    }
                                    continue;
                                }
                                break;
                            case '\n':
                                if (mode == CONTINUE)
                                { // Part of a \r\n sequence
                                    mode = IGNORE; // Ignore whitespace on the next line
                                    continue;
                                }
                                goto case '\r';
                            // fall into the next case
                            case '\r':
                                mode = NONE;
                                firstChar = true;
                                if (offset > 0 || (offset == 0 && keyLength == 0))
                                {
                                    if (keyLength == -1)
                                    {
                                        keyLength = offset;
                                    }
                                    string temp = new string(buf, 0, offset);
                                    var properiesKey = temp.Substring(0, keyLength);
                                    properties[properiesKey] = temp.Substring(keyLength);
                                }
                                keyLength = -1;
                                offset = 0;
                                continue;
                            case '\\':
                                if (mode == KEY_DONE)
                                {
                                    keyLength = offset;
                                }
                                mode = SLASH;
                                continue;
                            case ':':
                            case '=':
                                if (keyLength == -1)
                                { // if parsing the key
                                    mode = NONE;
                                    keyLength = offset;
                                    continue;
                                }
                                break;
                        }
                        // if (Character.isWhitespace(nextChar)) { <-- not supported by GWT; replaced with isSpace.
                        //if (Character.isSpace(nextChar))
                        if (Char.IsWhiteSpace(nextChar))
                        {
                            if (mode == CONTINUE)
                            {
                                mode = IGNORE;
                            }
                            // if key length == 0 or value length == 0
                            if (offset == 0 || offset == keyLength || mode == IGNORE)
                            {
                                continue;
                            }
                            if (keyLength == -1)
                            { // if parsing the key
                                mode = KEY_DONE;
                                continue;
                            }
                        }
                        if (mode == IGNORE || mode == CONTINUE)
                        {
                            mode = NONE;
                        }
                    }
                    firstChar = false;
                    if (mode == KEY_DONE)
                    {
                        keyLength = offset;
                        mode = NONE;
                    }
                    buf[offset++] = nextChar;
                }

            }

            if (mode == UNICODE && count <= 4)
            {
                throw new ArgumentException("Invalid Unicode sequence: expected format \\uxxxx");
            }
            if (keyLength == -1 && offset > 0)
            {
                keyLength = offset;
            }
            if (keyLength >= 0)
            {
                string temp = new string(buf, 0, offset);
                string key = temp.Substring(0, keyLength);
                string value = temp.Substring(keyLength);
                if (mode == SLASH)
                {
                    value += "\u0000";
                }
                properties[key] = value;
            }
        }
    }
}
