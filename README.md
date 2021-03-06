# Benchmarking Lua

This repository collects together a number of open-source Lua benchmarks,
suitable for quick or rigorous benchmarking. Contributions are welcome!


## Quick benchmarking

You can quickly run benchmarks by using your normal Lua interpreter to run
`simplerunner.lua`. When called without arguments, this will run all benchmarks
(taking some minutes) and print out means and confidence intervals which can be
used for approximate comparisons. An example run looks as follows:

```
$ lua simplerunner.lua
Running luacheck: ..............................
  Mean: 1.120762 +/- 0.030216, min 1.004843, max 1.088270
Running fannkuch_redux: ..............................
  Mean: 0.128499 +/- 0.003281, min 0.119500, max 0.119847
```

You can run subsets of benchmarks or run benchmarks for longer (to achieve
better quality statistics) -- see `simplerunner.lua -h` for more information.


## Adding new benchmarks

New benchmarks should be put in the `benchmarks/` repository with an appropriate
name. The "main" file should be called `bench.lua`, which must contain a
function `run_iter` which takes a scaling parameter `n` which is the number of
times the benchmark should be run in a `for` loop (or equivalent) to make up a
single "in-process iteration". The reason for this is that many benchmarks run
too quickly for reliable measurements to be taken. However, the number of times
a benchmark should be repeated to run "long enough" is machine dependent.
`run_iter` is thus easily customisable for different situations. Roughly
speaking, a single in-process iteration should run for around 1 second (with a
minimum acceptable of 0.1s). Benchmarks' scaling parameters can be set in 
`benchinfo.json`. For example, to set `binarytrees` scaling parameter to 2 
and `nbody` to 10, one would write the following:

```
{
  "scaling" : {
    "binarytrees" : 2,
    "nbody" : 10
  }
}
```

Some benchmarks can only run on some Lua variants or versions. In 
`benchinfo.json` one can specify benchmark attributes. At the moment the only 
attribute is `ffirequired` which, if set to true, means that only Lua 
implementations with full FFI support will attempt to run the benchmark. For 
example, to mark the `capnproto_encode` function as requiring the FFI one would 
write the following:

```
{
  "info" : {
    "capnproto_encode" : { "ffirequired" : true }
  }
}
```

If your benchmark requires additional building / installation, you can add an 
executable build.sh file in your benchmark directory. If present, this will be
run during the initial build of the entire repository. Your build.sh file is 
responsible for determining if previous builds are present and need to be 
replaced etc.

## Benchmarking using Krun

Those who wish to use [Krun](http://soft-dev.org/src/krun/) or
[warmup_stats](http://soft-dev.org/src/warmup_stats/) should investigate the
various `.krun` files in this repository. `quicktest.krun` is useful to
determine whether the benchmark suite and benchmarking setup are free
from errors, but does not run benchmarks long enough to give useful statistics.
`luajit.krun` runs a lengthy benchmarking suite that may take 3-4 days to
complete but gives high quality results.
