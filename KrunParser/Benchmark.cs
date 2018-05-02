using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krun {

    public class Benchmark {
        public string Name { get; set; }
        public int PExecI { get; set; }
        public SteadyState classification { get; set; }
        public ChangePoint[] changepoints { get; set; }
        public float[] wallclock_times { get; set; }
        public HashSet<int> Outliers { get; set; }
        public int IterationCount => wallclock_times.Length;

        public override string ToString() {
            return $"{Name}[{PExecI}] state: {classification}, average: {Average:0.####}({LowerAverage:0.####}-{UpperAverage:0.####}),  outliers: {Outliers.Count}, changepoints: {changepoints.Length}";
        }

        public double Average => wallclock_times.Skip(changepoints.FirstOrDefault()?.Start ?? 0).Where((v, i) => !Outliers.Contains(i)).Average();
        public double RawAverage => wallclock_times.Average();
        public float[] OutlierValues => wallclock_times.Where((v, i) => Outliers.Contains(i)).ToArray();
        public double? OutlierAverage => OutlierValues.Length > 0 ? OutlierValues.Average() : new double?();
        public double OutlierRatio => OutlierValues.Length / wallclock_times.Length;


        public int SmallOutlierCount => OutlierValues.Where(v => v < Average).Count();
        public int BigOutlierCount => OutlierValues.Where(v => v > Average).Count();

        public long[][] aperf_times { get; set; }
        public long[][] mperf_times { get; set; }

        public double[][] AMPerfRatios {
            get {
                var result = new double[aperf_times.Length][];

                for (int i = 0; i < aperf_times.Length; i++) {
                    var ratios = new double[IterationCount];
                    result[i] = ratios;

                    var mperf = mperf_times[i];
                    var aperf = aperf_times[i];
                    for (int j = 0; j < IterationCount; j++) {
                        ratios[j] = (double)(aperf[j] / (decimal)mperf[j]);
                        Debug.Assert(ratios[j] > 0.999 || j <= 1);
                    }
                }
                return result;
            }
        }

        private float[] _sortedtimes;
        public float[] SortedTimes {
            get {
                if (_sortedtimes == null) {
                    _sortedtimes = wallclock_times.OrderBy(v => v).ToArray();
                }
                return _sortedtimes;
            }
        }

        public double LowerAverage => SortedTimes.Take(wallclock_times.Length / 2).Average();
        public double UpperAverage => SortedTimes.Skip(wallclock_times.Length / 2).Average();
        public double Quertile3 => SortedTimes.Skip(wallclock_times.Length / 2).Take(wallclock_times.Length / 4).Average();

        public IEnumerable<float> GetValuesInRange(int start, int length) {
            for (int i = start; i < start + length; i++) {
                if (!Outliers.Contains(i)) {
                    yield return wallclock_times[i];
                }
            }
            yield break;
        }
    }

    public class ChangePoint {
        public Benchmark Bench { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public float Mean { get; set; }
        public ChangePointFlags Flags { get; set; }
        public List<float> Times => Bench.GetValuesInRange(Start, Length).ToList();
        public List<float> RawTimes => Bench.wallclock_times.Skip(Start).Take(Length).ToList();

        public ChangePoint(int start, int end, float mean = 0, ChangePointFlags flags = ChangePointFlags.None) {
            Start = start;
            Length = end - start;
            Mean = mean;
            Flags = flags;
        }

        public override string ToString() {
            return $"{Bench} start: {Start} length: {Length} mean: {Mean}";
        }
    }

    [Flags]
    public enum ChangePointFlags {
        None = 0,
        Start = 0x1,
        End = 0x2,
    }
}
