using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guester
{
    public class CacheService : ICacheService
    {
        public void ClearCache()
        {
            try
            {
                string cacheDirectory = FileSystem.CacheDirectory;
                if (!string.IsNullOrEmpty(cacheDirectory))
                {
                    ClearDirectory(new DirectoryInfo(cacheDirectory));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void ClearDirectory(DirectoryInfo directory)
        {
            if (directory != null && directory.Exists)
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    ClearDirectory(subDirectory);
                    subDirectory.Delete();
                }
            }
        }
    }
}
