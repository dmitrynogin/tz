using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using TimeZoneConverter;

namespace TimeZones
{
    class Program
    {
        const string Data = "../../../tz.json";

        static void Main(string[] args)
        {
            Load();

            foreach (var iana in TZConvert.KnownIanaTimeZoneNames)
                if(!Zones.Exists(z => z.Iana == iana && z.Windows == TZConvert.IanaToWindows(iana)))
                    Zones.Add(new Zone { Iana = iana, Windows = TZConvert.IanaToWindows(iana) });

            foreach (var windows in TZConvert.KnownWindowsTimeZoneIds)
                if (!Zones.Exists(z => z.Iana == TZConvert.WindowsToIana(windows) && z.Windows == windows))
                    Zones.Add(new Zone { Iana = TZConvert.WindowsToIana(windows), Windows = windows });

            Zones = Zones
                .DistinctBy(z => (z.Iana, z.Windows))
                .ToList();

            Save();

            Console.WriteLine("All z[tz]: " + Zones.All(z => TimeZoneInfo.GetSystemTimeZones().Any(tz => tz.Id == z.Windows || tz.Id == z.Iana)));
            Console.WriteLine("All tz[z]: " + TimeZoneInfo.GetSystemTimeZones().All(tz => Zones.Any(z => tz.Id == z.Windows || tz.Id == z.Iana)));
        }

        static List<Zone> Zones { get; set; } = new List<Zone>();

        static void Load()
        {
            if (!File.Exists(Data))
                return;

            using (var sr = File.OpenText(Data))
            using(var jr = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer();
                Zones = serializer.Deserialize<List<Zone>>(jr);
            }

            Console.WriteLine($"{Zones.Count} loaded...");
        }

        static void Save()
        {
            using (var sw = File.CreateText(Data))
            using (var jw = new JsonTextWriter(sw) { Formatting = Formatting.Indented })
            {
                var serializer = new JsonSerializer() {  };
                serializer.Serialize(jw, Zones);
            }

            Console.WriteLine($"{Zones.Count} saved...");
        }
    }

    public class Zone
    {
        public string Windows { get; set; }
        public string Iana { get; set; }
    }
}
