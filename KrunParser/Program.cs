using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using System.Diagnostics;
using System;
using Newtonsoft.Json.Linq;
using Krun;
using LinqStatistics;

namespace StatsTesting {
    public class Program {
        static string basedir = @"c:\benchmarks";
        static string jsonname = "results.json";


        static void Main(string[] args) {
            JsonConvert.DefaultSettings = () => {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter { CamelCaseText = false });
                return settings;
            };


            var krunresult1 = ParseResultFile(Path.Combine(basedir, "resultdir1", jsonname));
            var krunresult2 = ParseResultFile(Path.Combine(basedir, "resultdir2", jsonname));
            var krunresult3 = ParseResultFile(Path.Combine(basedir, "resultdir2", jsonname));

            var key = krunresult2.classifications.Keys.First();
            var benchmarks1 = krunresult1.GetBenchmarks(key);
            var benchmarks2 = krunresult2.GetBenchmarks(key);
            var benchmarks3 = krunresult3.GetBenchmarks(key);

            var times1 = benchmarks1.Select(b => b.Average).OrderBy(n => n).ToList();
            var times1_q3 = benchmarks1.Select(b => b.Quertile3).OrderBy(n => n).ToList();
            var times2 = benchmarks2.Select(b => b.Average).OrderBy(n => n).ToList();
            var times2_q3 = benchmarks2.Select(b => b.Quertile3).OrderBy(n => n).ToList();

            int[] getlengthdist(IEnumerable<(Benchmark b, SpikeRange[] spikes)> benches) {
                return benches.Take(15).SelectMany(b => b.spikes).
                       Where(s => s.start > 20).
                       Select(s => s.times.Length).OrderBy(s => s).ToArray();
            }

            var benchSpikes1 = benchmarks1.Select(b =>  (b, spikes : GetSpikeRanges(b))).ToArray();
            var benchSpikes2 = benchmarks2.Select(b => (b, spikes: GetSpikeRanges(b))).ToArray(); ;
            var benchSpikes3 = benchmarks3.Select(b => (b, spikes: GetSpikeRanges(b))).ToArray();

            var shortspikes = benchSpikes1.SelectMany(b => b.spikes).Where(s => s.times.Length == 1).ToArray();
            var spikelengths1 = getlengthdist(benchSpikes1);
            var spikelengths2 = getlengthdist(benchSpikes2);
            var spikelengths3 = getlengthdist(benchSpikes3);


            var badbenches = benchmarks1.SelectMany(b => b.changepoints).
                             Where(c => c.Mean > 0.135 && c.Start != 0).
                             ToArray();

            var c2 = badbenches.First().Times.Count;

            //var root = JsonConvert.DeserializeObject<Rootobject>(text);

            return;
        }

        public class SpikeRange {
            public SpikeRange(Benchmark b, int start, float average, float[] times) {
                this.b = b;
                this.start = start;
                this.average = average;
                this.times = times;
            }

            public Benchmark b { get; set; }
            public int start { get; set; }
            public float average { get; set; }
            public float[] times { get; set; }

            public override string ToString() {
                return $"Length: {times.Length}, Start: {start}, Average:{average}";
            }
        }


        private static SpikeRange[] GetSpikeRanges(Benchmark b, float? minchange = null) {
            var upper = b.Quertile3;
            minchange = minchange ?? 1.03f;
            var outliers = b.wallclock_times.Select((v, n) => (n, v)).
                           Where(t => (t.v / upper) > minchange).
                           ToArray();
            int start = 1;
            int prev = -2;
            var list = new List<SpikeRange>();

            for (int i = 1; i < outliers.Length; i++) {
                bool end = i == (outliers.Length - 1);
                int distance = outliers[i].n - prev;

                if ((distance > 2 && i > 1) || end) {
                    Debug.Assert(end || outliers[i].n < 10 || distance > 2);
                    var times = outliers.Skip(start).Take(i - start).Select(t => t.v).ToArray();
                    if (prev >= 0) {
                        list.Add(new SpikeRange(b, outliers[start].n, times.Average(), times));
                    }
                    start = i;
                }
                prev = outliers[i].n;
            }
            return list.ToArray();
        }

        public static KrunResults ParseResultFile(string path) {
              //var json = JsonValue.Parse(text);
            //var root = serializer.Deserialize<Rootobject>(json);

            var serializer = new Newtonsoft.Json.JsonSerializer();

            var text = File.ReadAllText(path);           
            return JsonConvert.DeserializeObject<KrunResults>(text);
        }


    }
    







}
