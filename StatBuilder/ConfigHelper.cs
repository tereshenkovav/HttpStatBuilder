using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace StatBuilder
{
    public class BasicConfig
    {
        [NonSerialized]
        private string FileName;
        private bool noFile;
    
        public static T CreateConfig<T>(string FileName) where T : BasicConfig,new()
        {
            if (!File.Exists(FileName))
            {
                T res = new T();
                res.FileName = FileName;
                res.noFile = true;
                return res;
            }

            Stream stm = File.Open(FileName, FileMode.Open);
                
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                T res = (T)xml.Deserialize(stm);
                res.FileName = FileName;
                res.noFile = false;
                return res;
            }
            finally
            {
                stm.Close();
            }
        }

        public void SaveConfig()
        {
            Stream stm = File.Create(FileName);
            XmlSerializer xml = new XmlSerializer(this.GetType());
            xml.Serialize(stm, this);
            stm.Close();                
        }

        public bool isNoFile()
        {
            return noFile;
        }
    }
}
