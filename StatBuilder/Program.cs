using System;
using LiteDB;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace StatBuilder
{
    class Program
    {
        private static List<String> months = new string[] 
 { "Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec" }.ToList() ;

        public static bool getDateFromRegEx(string str, string regex, out DateTime value)
        {
            Regex Re = new Regex(regex,RegexOptions.Singleline);
            Match M = Re.Match(str);

            if (M.Success)
            {
                value = new DateTime(Int32.Parse(M.Groups[3].Value),
                  months.IndexOf(M.Groups[2].Value)+1,
                  Int32.Parse(M.Groups[1].Value)) ;
                return true;
            }
            else
            {
                value = DateTime.Now;
                return false;
            }
            
        }

        static void Main(string[] args)
        {               
            if (args.Length==0) {
               Console.WriteLine("Parameter - xml config file name") ;
               return ;
            }

            var config = MainConfig.CreateConfig<MainConfig>(args[0]);

            var db = new LiteDatabase(config.StoreDB) ;

            var recs = db.GetCollection<DownloadRec>("downloadrecs");
            
            foreach(var file in Directory.EnumerateFiles(config.LogDir,config.LogFilePart+"*")) {
               using (var stm = new StreamReader(file)) {
                 String line;
                 while ((line = stm.ReadLine()) != null)
                    if (line.Trim().Length>0) 
                      if (line.Contains(config.StatFilePart)) {                         
                         String filename="" ;
                         foreach(var s in line.Split(new char[] { ' ' })) 
                           if (s.Contains(config.StatFilePart)) {
                             string[] tmp=s.Split(new char[] { '/' }) ;
                             filename=tmp[tmp.Length-1] ;
                           }
                         DateTime date ;
                         getDateFromRegEx(line,@"\[(\d\d)\/(.*?)\/(\d\d\d\d)\:",out date) ;
                         var rec = new DownloadRec(date,line,filename) ;
                         var result = recs.Find(x => x.recstr.Equals(rec.recstr));
                         if (result.Count()==0) recs.Insert(rec) ;
                      }                         
                      
               }
            }

            using (var stm = new StreamWriter(config.OutFile)) {
              stm.WriteLine("<html><body><h1>Stat of download {0} - {1}</h1>",
                config.LogFilePart,config.StatFilePart) ;
              stm.WriteLine("<table border=1 cellPadding=5 cellSpacing=0>") ;
              var res = recs.FindAll().GroupBy(x=>new { date = x.date, filename = x.filename}).OrderBy(x=>x.Key.date).ThenBy(x=>x.Key.filename) ;            
              int cntall=0 ;
              foreach(var el in res) {
                stm.WriteLine("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>",
                   el.Key.date.ToString("dd.MM.yyyy"),el.Key.filename,el.Count()) ;
                cntall+=el.Count() ;
              }
              stm.WriteLine("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>",
                 "","Total",cntall) ;
              stm.WriteLine("</table></body></html>") ;
            }
          
            db.Dispose() ;
        }
    }
}
