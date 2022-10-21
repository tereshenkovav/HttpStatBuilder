using System;

namespace StatBuilder
{
    public class DownloadRec
    {
        public DateTime date { get ; set ; }
        public string recstr  { get ; set ; }
        public string filename { get ; set ; }
        public DownloadRec(DateTime date, String recstr, String filename) {
          this.date = date ;
          this.recstr = recstr ;
          this.filename = filename ;
        }
    }
}
