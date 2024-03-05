using Android.Content;
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
                var context = MainActivity.Instance.BaseContext;

                Java.IO.File cacheDir = context.CacheDir;
                if (cacheDir != null && cacheDir.IsDirectory)
                {
                    DeleteFiles(cacheDir);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void DeleteFiles(Java.IO.File directory)
        {
            if (directory != null && directory.IsDirectory)
            {
                Java.IO.File[] files = directory.ListFiles();
                if (files != null)
                {
                    foreach (Java.IO.File file in files)
                    {
                        if (file.IsDirectory)
                        {
                            DeleteFiles(file);
                        }
                        else
                        {
                            file.Delete();
                        }
                    }
                }
            }
        }
    }
}
