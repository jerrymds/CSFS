using System;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace CTBC.CSFS.BackupHistoryFile
{
    public class ZipHelper
    {


        public static void addFolderToZip(ZipFile f, string root, string folder)
        {
            string relative = folder.Substring(root.Length);
            if (relative.Length > 0)
            {
                f.AddDirectory(relative);
            }

            foreach (string file in Directory.GetFiles(folder))
            {
                relative = file.Substring(root.Length);
                f.Add(file, relative);
            }

            foreach (string subFolder in Directory.GetDirectories(folder))
            {
                addFolderToZip(f, root, subFolder);
            }
        }

        public static void CreateZip(string sourceFilePath, string destinationZipFilePath, string zipFileName)
        {
            ICSharpCode.SharpZipLib.Zip.ZipFile zipFile = ICSharpCode.SharpZipLib.Zip.ZipFile.Create(zipFileName);
            zipFile.BeginUpdate();
            addFolderToZip(zipFile, sourceFilePath, sourceFilePath);
            zipFile.CommitUpdate();
            zipFile.Close();
        }





    }
}
