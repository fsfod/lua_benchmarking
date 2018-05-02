using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Krun {

    public enum SteadyState {
        [EnumMember(Value = "no steady state")]
        NoSteadyState,
        [EnumMember(Value = "warmup")]
        Warmup,
        [EnumMember(Value = "slowdown")]
        SlowDown,
    }


    public class KrunResults {
        public string config { get; set; }
        public int window_size { get; set; }
        public Audit audit { get; set; }
        public bool error_flag { get; set; }

        /*
        public Eta_Estimates eta_estimates { get; set; }
        public Unique_Outliers unique_outliers { get; set; }
        public Changepoint_Vars changepoint_vars { get; set; }
        public Classifier classifier { get; set; }
        public Core_Cycle_Counts core_cycle_counts { get; set; }
        
        
        public Common_Outliers common_outliers { get; set; }
        public ResultSet<int> all_outliers { get; set; }
        */

        public Dictionary<string, SteadyState[]> classifications { get; set; }
        public Dictionary<string, long[][][]> aperf_counts { get; set; }
        public Dictionary<string, long[][][]> mperf_counts { get; set; }
        public Dictionary<string, int[][]> changepoints { get; set; }
        public Dictionary<string, int[][]> all_outliers { get; set; }

        public Dictionary<string, int[][]> common_outliers { get; set; }

        public Dictionary<string, float[][]> wallclock_times { get; set; }
        public Dictionary<string, float[][]> changepoint_means { get; set; }

        public List<Benchmark> GetBenchmarks(string key) {
            var classifications = this.classifications[key];
            var changepoints = this.changepoints[key];
            var wallclock_times = this.wallclock_times[key];
            var changepoint_means = this.changepoint_means[key];
            var outliers = this.all_outliers[key];
            var aperf_counts = this.aperf_counts[key];
            var mperf_counts = this.mperf_counts[key];

            var benchmarks = new List<Benchmark>();
            for (int i = 0; i < this.classifications.Values.First().Length; i++) {
                var b = new Benchmark() {
                    Name = key,
                    PExecI = i,
                    classification = classifications[i],
                    Outliers = new HashSet<int>(outliers[i]),
                    wallclock_times = wallclock_times[i],
                    aperf_times = aperf_counts[i],
                    mperf_times = mperf_counts[i],
                };
                var a = b.AMPerfRatios;

                int numcpoints = changepoints[i].Length;
                int numvalues = wallclock_times[i].Length;
                if (numcpoints == 1) {
                    b.changepoints = new ChangePoint[] {
                       new ChangePoint(0, changepoints[i][0], changepoint_means[i][0]) {
                           Flags = ChangePointFlags.Start | ChangePointFlags.End,
                           Bench = b,
                       }
                    };
                } else if (numcpoints > 1) {

                    b.changepoints = changepoints[i].Select((x, j) => {
                        int start = x;
                        ChangePoint ret;
                        if (j == 0) {
                            Debug.Assert(x != 0);
                            ret = new ChangePoint(0, x, changepoint_means[i][j]);
                            ret.Flags = ChangePointFlags.Start;
                        } else {
                            ret = new ChangePoint(changepoints[i][j - 1], x, changepoint_means[i][j]);
                        }
                        ret.Bench = b;
                        return ret;
                    }).Concat(new ChangePoint[] {
                        new ChangePoint(changepoints[i][numcpoints-1], numvalues, 0)
                    }).ToArray();
                }
                benchmarks.Add(b);
            }
            return benchmarks;
        }
    }

    public class Audit {
        public string cpuinfo { get; set; }
        public string dmesg { get; set; }
        public string debian_version { get; set; }
        public string[] cli_args { get; set; }
        public string uname { get; set; }
        public string krun_version { get; set; }
        public string packages { get; set; }
    }

    public class Eta_Estimates {
        public float[] data { get; set; }
    }

    public class Unique_Outliers {
        public int?[][] data { get; set; }
    }


    public class Changepoint_Vars {
        public float[][] data { get; set; }
    }

    public class Changepoint_Means {
        public float[][] data { get; set; }
    }

    public class Classifier {
        public int steady { get; set; }
        public float delta { get; set; }
    }

    public class Core_Cycle_Counts {
        public int[][][] data { get; set; }
    }


    public class Classifications {
        public string[] data { get; set; }
    }

    public class Common_Outliers {
        public int?[][] data { get; set; }
    }

}
