using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace MoreAdminCommands
{
    public class MACconfig
    {
        public string defaultMuteAllReason = "Listen to find out";
        public string muteAllReason = "Listen to find out";
        public string redPass = "";
        public string bluePass = "";
        public string greenPass = "";
        public string yellowPass = "";

        public int maxDamage = 500;

        public bool maxDamageIgnore = false;
        public bool maxDamageKick = false;
        public bool maxDamageBan = false;



        public static MACconfig Read(string path)
        {
            if (!File.Exists(path))
                return new MACconfig();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fs);
            }
        }

        public static MACconfig Read(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                var cf = JsonConvert.DeserializeObject<MACconfig>(sr.ReadToEnd());
                if (ConfigRead != null)
                    ConfigRead(cf);
                return cf;
            }
        }

        public void Write(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                Write(fs);
            }
        }

        public void Write(Stream stream)
        {
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(str);
            }
        }

        public static Action<MACconfig> ConfigRead;
    }
}
