using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Core;

namespace CI_OneWorldParser
{
    class Utils
    {
        public static string checkMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }

        public static void ExtractFile(string filename, string extractfile)
        {
            Console.WriteLine("Extracting {0}", filename);
            byte[] buffer = new byte[4096];  //more than 4k is a waste
            using (Stream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (GZipInputStream gzipStream = new GZipInputStream(fs))
                {
                    using (FileStream fsout = File.Create(extractfile))
                    {
                        StreamUtils.Copy(gzipStream, fsout, buffer);
                    }
                }
            }
        }
    }
}
