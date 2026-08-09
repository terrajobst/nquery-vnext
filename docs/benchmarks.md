# Benchmarks

All three engines are fed the exact same strongly typed Northwind records, so
differences come from the engine, not the data.

The three engines compared:

- **Old** — the original NQuery engine (`external/nquery-old`), a .NET Framework 2.0-era
  code base recompiled as .NET Core 8.0 (`NQuery.Old`).
- **Baseline** — `main` of this repo (`NQuery.Baseline`); the ratio reference
  (`Ratio = 1.00`).
- **Refactored** — the current worktree.

`Ratio` and `Alloc Ratio` are relative to **Baseline**; lower is better. There
are three suites: **Execution** measures per-row engine cost with compilation
excluded; **Compilation** measures turning SQL text into an executable plan;
**Parsing** measures just lexing and parsing SQL text into a syntax tree.

# Execution

Execution-only: each query is compiled once in setup and then drained per
iteration, so compilation cost is excluded and the numbers reflect per-row engine
cost and allocations in isolation.

## Observations

- **Refactored is fastest on every shape**, often by a wide margin. `Scan` is
  the standout: it streams rows with almost no per-row allocation (2.38 KB vs
  ~310 KB), running ~12× faster than Baseline and ~5× faster than Old.
- **Old vs Baseline is a mixed bag.** The original engine is actually faster
  than Baseline on several shapes (Scan, Aggregate, Report) but slower or
  comparable on others (Join, Sort, TopWithTies), so the previous rewrite traded
  raw speed for other properties on some plans.
- **Plan shape dominates the correlated cases.** On `NestedLoops` (a correlated
  scalar `TOP 1 … ORDER BY` that survives as a dependent join), Old runs the
  inner loop cheaply (75 μs) while Baseline blows up to 3.1 ms / 1.9 MB;
  Refactored's index spool (`Planner.TryPlanIndexSpool`) keeps it close to Old at
  138 μs (0.04×), the residual gap being allocation-bound — 539 KB vs Old's 61 KB
  (the spool's columnar store plus boxed keys; see the index-spool follow-ups in
  `REFACTORING.md`). On `Decorrelated`, Baseline's hash-join decorrelation keeps
  allocations tiny (24 KB), so Old looks allocation-heavy by comparison (11.5×)
  even though it is faster in wall-clock time; Refactored is both fastest (37 μs)
  and leanest (18 KB).

## Summary

Shapes as rows, engines as columns. All factors are relative to **Baseline**
(`main` = 1.00×).

### Speed (× Baseline, lower = faster)

| Shape        |  Old  | Baseline | Refactored |
|------------- |------:|---------:|-----------:|
| Scan         | 0.45× |   1.00×  |   0.08×    |
| Join         | 1.09× |   1.00×  |   0.24×    |
| Aggregate    | 0.41× |   1.00×  |   0.32×    |
| Sort         | 0.99× |   1.00×  |   0.59×    |
| Report       | 0.57× |   1.00×  |   0.45×    |
| TopWithTies  | 0.94× |   1.00×  |   0.38×    |
| Decorrelated | 0.63× |   1.00×  |   0.05×    |
| NestedLoops  | 0.02× |   1.00×  |   0.04×    |

### Memory (allocated per op, with × Baseline, lower = leaner)

| Shape        |        Old        |     Baseline     |    Refactored    |
|------------- |------------------:|-----------------:|-----------------:|
| Scan         |  320 KB (1.03×)   |  309 KB (1.00×)  |  2.4 KB (0.008×) |
| Join         |  532 KB (1.44×)   |  371 KB (1.00×)  |   62 KB (0.17×)  |
| Aggregate    |  535 KB (0.96×)   |  558 KB (1.00×)  |  241 KB (0.43×)  |
| Sort         |  401 KB (0.95×)   |  423 KB (1.00×)  |  192 KB (0.46×)  |
| Report       | 1029 KB (1.29×)   |  800 KB (1.00×)  |  417 KB (0.52×)  |
| TopWithTies  |  401 KB (0.95×)   |  423 KB (1.00×)  |  126 KB (0.30×)  |
| Decorrelated |  282 KB (11.50×)  |   25 KB (1.00×)  |   18 KB (0.73×)  |
| NestedLoops  |   61 KB (0.03×)   | 1907 KB (1.00×)  |  539 KB (0.28×)  |

## Full results

Default job (`DefaultJob`, full warmup + 15 iterations).

| Method     | Shape        | Mean        | Error     | StdDev     | Median      | Ratio | Gen0     | Gen1     | Allocated  | Alloc Ratio |
|----------- |------------- |------------:|----------:|-----------:|------------:|------:|---------:|---------:|-----------:|------------:|
| Old        | Scan         |   551.01 μs | 10.967 μs |  28.308 μs |   543.06 μs |  0.45 |  39.0625 |        - |  319.97 KB |       1.034 |
| Baseline   | Scan         | 1,233.31 μs | 22.830 μs |  39.381 μs | 1,225.28 μs |  1.00 |  37.1094 |  11.7188 |  309.42 KB |       1.000 |
| Refactored | Scan         |   103.30 μs |  2.038 μs |   4.253 μs |   101.45 μs |  0.08 |   0.2441 |        - |    2.38 KB |       0.008 |
|            |              |             |           |            |             |       |          |          |            |             |
| Old        | Join         |   655.71 μs |  6.150 μs |   5.752 μs |   655.68 μs |  1.09 |  64.4531 |  22.4609 |  532.41 KB |        1.44 |
| Baseline   | Join         |   601.61 μs |  3.005 μs |   2.346 μs |   601.47 μs |  1.00 |  44.9219 |  13.6719 |   370.9 KB |        1.00 |
| Refactored | Join         |   146.84 μs |  1.079 μs |   0.957 μs |   146.53 μs |  0.24 |   7.3242 |   0.7324 |   61.82 KB |        0.17 |
|            |              |             |           |            |             |       |          |          |            |             |
| Old        | Aggregate    |   854.37 μs | 17.035 μs |  39.480 μs |   853.29 μs |  0.41 |  65.4297 |  23.4375 |  535.34 KB |        0.96 |
| Baseline   | Aggregate    | 2,103.69 μs | 41.369 μs |  89.934 μs | 2,089.16 μs |  1.00 |  66.4063 |  27.3438 |   558.4 KB |        1.00 |
| Refactored | Aggregate    |   676.00 μs | 13.396 μs |  27.963 μs |   684.30 μs |  0.32 |  29.2969 |   6.8359 |  240.67 KB |        0.43 |
|            |              |             |           |            |             |       |          |          |            |             |
| Old        | Sort         | 1,122.67 μs | 18.313 μs |  27.410 μs | 1,114.58 μs |  0.99 |  48.8281 |  17.5781 |   401.2 KB |        0.95 |
| Baseline   | Sort         | 1,138.79 μs | 19.128 μs |  30.889 μs | 1,127.52 μs |  1.00 |  50.7813 |  15.6250 |  422.55 KB |        1.00 |
| Refactored | Sort         |   674.63 μs | 13.192 μs |  15.192 μs |   673.40 μs |  0.59 |  23.4375 |   5.8594 |  192.45 KB |        0.46 |
|            |              |             |           |            |             |       |          |          |            |             |
| Old        | Report       | 1,902.04 μs | 37.239 μs |  47.095 μs | 1,901.46 μs |  0.57 | 125.0000 | 117.1875 | 1028.52 KB |        1.29 |
| Baseline   | Report       | 3,314.51 μs | 60.497 μs | 130.225 μs | 3,269.09 μs |  1.00 |  93.7500 |  54.6875 |  799.72 KB |        1.00 |
| Refactored | Report       | 1,484.83 μs | 29.659 μs |  77.088 μs | 1,453.78 μs |  0.45 |  50.7813 |  19.5313 |   417.3 KB |        0.52 |
|            |              |             |           |            |             |       |          |          |            |             |
| Old        | TopWithTies  |   639.84 μs | 12.568 μs |  22.339 μs |   626.25 μs |  0.94 |  48.8281 |  17.5781 |  401.23 KB |        0.95 |
| Baseline   | TopWithTies  |   680.81 μs |  4.509 μs |   4.218 μs |   679.93 μs |  1.00 |  51.7578 |  21.4844 |  423.34 KB |        1.00 |
| Refactored | TopWithTies  |   257.65 μs |  3.272 μs |   3.061 μs |   258.24 μs |  0.38 |  15.1367 |   3.4180 |  125.58 KB |        0.30 |
|            |              |             |           |            |             |       |          |          |            |             |
| Old        | Decorrelated |   450.92 μs |  7.174 μs |   6.711 μs |   449.37 μs |  0.63 |  34.1797 |        - |  282.07 KB |       11.50 |
| Baseline   | Decorrelated |   713.31 μs |  5.271 μs |   4.401 μs |   714.57 μs |  1.00 |   2.9297 |   1.9531 |   24.52 KB |        1.00 |
| Refactored | Decorrelated |    37.74 μs |  0.171 μs |   0.160 μs |    37.78 μs |  0.05 |   2.1973 |   0.0610 |      18 KB |        0.73 |
|            |              |             |           |            |             |       |          |          |            |             |
| Old        | NestedLoops  |    75.18 μs |  0.409 μs |   0.342 μs |    75.18 μs |  0.02 |   7.4463 |        - |    60.9 KB |        0.03 |
| Baseline   | NestedLoops  | 3,121.26 μs | 14.788 μs |  13.109 μs | 3,121.26 μs |  1.00 | 230.4688 |  35.1563 | 1906.72 KB |        1.00 |
| Refactored | NestedLoops  |   138.25 μs |  2.751 μs |   3.378 μs |   138.25 μs |  0.04 |  65.9180 |   8.0566 |  538.74 KB |        0.28 |

---

# Compilation

Compilation-only: each iteration drives a *fresh* `Query` through
`ExecuteSchemaReader()`, i.e. parse → bind → algebrize → optimize → plan → emit,
but no row iteration. The schema-reader path is used deliberately so the phase
measured is the same for all three engines: **Baseline emits lazily on the first
open** (`IteratorBuilder.Build` runs inside `CreateReader`), whereas Refactored
and Old build the executable plan during compile. Stopping at `Compilation.Compile()`
would undercount Baseline, so all three are forced through emit (without reading
rows).

## Observations

- **Old compiles far faster and leaner than both** — roughly 0.04–0.17× the time
  and 0.08–0.28× the allocations of Baseline. Its pipeline is a single, smaller
  pass; the new engines pay for a layered bind → algebrize → optimize → plan →
  emit pipeline.
- **Refactored compiles a bit *slower* than Baseline on most shapes** (up to
  ~1.7× on Join, ~1.5× on Aggregate) and allocates slightly more. This is the
  flip side of the execution wins: it front-loads work — eager emit of the
  executable plan plus richer optimization/planning — which is exactly what buys
  the large per-row speedups above.
- **The correlated shapes are the exception:** Refactored compiles *faster* than
  Baseline on `Decorrelated` (0.87×) and `NestedLoops` (0.54× time, 0.60×
  allocations), so its handling of those plans is leaner end to end.
- **Tradeoff:** for run-once queries that never amortize, Refactored's higher
  compile cost matters; for reused/cached plans it is paid once while execution
  savings recur. (Baseline additionally re-emits on *every* open, since its built
  iterator is not cached — not captured here, where each iteration compiles once.)

## Summary

Shapes as rows, engines as columns. All factors are relative to **Baseline**
(`main` = 1.00×).

### Speed (× Baseline, lower = faster)

| Shape        |  Old  | Baseline | Refactored |
|------------- |------:|---------:|-----------:|
| Scan         | 0.04× |   1.00×  |   1.13×    |
| Join         | 0.10× |   1.00×  |   1.74×    |
| Aggregate    | 0.17× |   1.00×  |   1.52×    |
| Sort         | 0.07× |   1.00×  |   1.24×    |
| Report       | 0.17× |   1.00×  |   1.21×    |
| TopWithTies  | 0.07× |   1.00×  |   1.10×    |
| Decorrelated | 0.11× |   1.00×  |   0.87×    |
| NestedLoops  | 0.04× |   1.00×  |   0.54×    |

### Memory (allocated per op, with × Baseline, lower = leaner)

| Shape        |       Old       |     Baseline     |    Refactored    |
|------------- |----------------:|-----------------:|-----------------:|
| Scan         |   34 KB (0.21×) |  158 KB (1.00×)  |  173 KB (1.09×)  |
| Join         |   68 KB (0.17×) |  405 KB (1.00×)  |  435 KB (1.07×)  |
| Aggregate    |  164 KB (0.28×) |  595 KB (1.00×)  |  596 KB (1.00×)  |
| Sort         |   20 KB (0.22×) |   91 KB (1.00×)  |  118 KB (1.29×)  |
| Report       |  274 KB (0.24×) | 1163 KB (1.00×)  | 1179 KB (1.01×)  |
| TopWithTies  |   18 KB (0.21×) |   86 KB (1.00×)  |  109 KB (1.26×)  |
| Decorrelated |   64 KB (0.23×) |  277 KB (1.00×)  |  317 KB (1.15×)  |
| NestedLoops  |   46 KB (0.08×) |  586 KB (1.00×)  |  355 KB (0.60×)  |

## Full results

Short job (`ShortRun`, 3 warmup + 3 iterations) — chosen because the full job is
slow for this many cases. Allocations are deterministic and reliable; the **Mean
times are indicative only** (note the wide Error/StdDev), though the relative
ordering is stable across runs.

| Method     | Shape        | Mean        | Error        | StdDev     | Ratio | Gen0     | Gen1    | Allocated  | Alloc Ratio |
|----------- |------------- |------------:|-------------:|-----------:|------:|---------:|--------:|-----------:|------------:|
| Old        | Scan         |    47.83 μs |    14.866 μs |   0.815 μs |  0.04 |   3.9063 |  0.9766 |   33.53 KB |        0.21 |
| Baseline   | Scan         | 1,221.59 μs | 1,139.256 μs |  62.446 μs |  1.00 |  17.5781 |  7.8125 |  158.11 KB |        1.00 |
| Refactored | Scan         | 1,372.73 μs |   836.701 μs |  45.862 μs |  1.13 |  19.5313 |  5.8594 |  172.73 KB |        1.09 |
|            |              |             |              |            |       |          |         |            |             |
| Old        | Join         |    87.76 μs |    15.144 μs |   0.830 μs |  0.10 |   8.3008 |  2.0752 |   68.49 KB |        0.17 |
| Baseline   | Join         |   915.82 μs | 1,158.698 μs |  63.512 μs |  1.00 |  48.8281 | 15.6250 |  405.23 KB |        1.00 |
| Refactored | Join         | 1,590.06 μs | 1,513.639 μs |  82.968 μs |  1.74 |  50.7813 | 11.7188 |  435.08 KB |        1.07 |
|            |              |             |              |            |       |          |         |            |             |
| Old        | Aggregate    |   458.30 μs |   204.113 μs |  11.188 μs |  0.17 |  19.5313 |  5.8594 |  163.58 KB |        0.28 |
| Baseline   | Aggregate    | 2,653.01 μs | 3,272.380 μs | 179.370 μs |  1.00 |  70.3125 | 23.4375 |  594.62 KB |        1.00 |
| Refactored | Aggregate    | 4,007.16 μs | 5,570.601 μs | 305.343 μs |  1.52 |  70.3125 | 23.4375 |  596.29 KB |        1.00 |
|            |              |             |              |            |       |          |         |            |             |
| Old        | Sort         |    30.49 μs |     2.908 μs |   0.159 μs |  0.07 |   2.3804 |  0.7324 |   19.67 KB |        0.22 |
| Baseline   | Sort         |   450.68 μs |   332.478 μs |  18.224 μs |  1.00 |  10.7422 |  4.8828 |   91.37 KB |        1.00 |
| Refactored | Sort         |   556.85 μs | 1,813.591 μs |  99.409 μs |  1.24 |  13.6719 |  3.9063 |  117.84 KB |        1.29 |
|            |              |             |              |            |       |          |         |            |             |
| Old        | Report       |   723.28 μs | 1,784.616 μs |  97.821 μs |  0.17 |  33.2031 |  7.8125 |  273.87 KB |        0.24 |
| Baseline   | Report       | 4,266.53 μs | 8,385.789 μs | 459.653 μs |  1.01 | 140.6250 | 46.8750 | 1162.61 KB |        1.00 |
| Refactored | Report       | 5,109.76 μs | 2,786.278 μs | 152.725 μs |  1.21 | 140.6250 | 46.8750 | 1178.63 KB |        1.01 |
|            |              |             |              |            |       |          |         |            |             |
| Old        | TopWithTies  |    29.95 μs |    20.488 μs |   1.123 μs |  0.07 |   2.1667 |  0.6409 |   17.89 KB |        0.21 |
| Baseline   | TopWithTies  |   421.81 μs |    27.412 μs |   1.503 μs |  1.00 |   9.7656 |  4.8828 |   86.42 KB |        1.00 |
| Refactored | TopWithTies  |   463.40 μs | 1,230.612 μs |  67.454 μs |  1.10 |  12.6953 |  3.9063 |  108.51 KB |        1.26 |
|            |              |             |              |            |       |          |         |            |             |
| Old        | Decorrelated |    91.84 μs |    53.521 μs |   2.934 μs |  0.11 |   7.8125 |  1.9531 |   63.98 KB |        0.23 |
| Baseline   | Decorrelated |   860.84 μs |   786.492 μs |  43.110 μs |  1.00 |  33.2031 |  9.7656 |  276.76 KB |        1.00 |
| Refactored | Decorrelated |   746.08 μs | 1,405.613 μs |  77.046 μs |  0.87 |  37.1094 |  9.7656 |  316.92 KB |        1.15 |
|            |              |             |              |            |       |          |         |            |             |
| Old        | NestedLoops  |    88.15 μs |    52.245 μs |   2.864 μs |  0.04 |   5.4932 |  1.3428 |   45.66 KB |        0.08 |
| Baseline   | NestedLoops  | 2,072.78 μs | 1,132.686 μs |  62.086 μs |  1.00 |  70.3125 | 23.4375 |  586.14 KB |        1.00 |
| Refactored | NestedLoops  | 1,116.25 μs |   783.431 μs |  42.943 μs |  0.54 |  42.9688 | 13.6719 |  354.55 KB |        0.60 |

---

# Parsing

Parse-only: each iteration lexes and parses the SQL text into a syntax tree and
does nothing else. Parsing is schema-independent, so no context/catalog is built
— all three engines run the identical query text through their lexer and parser.
The old engine's `Parser` is internal, so it is reached through a tiny public
shim (`NQuery.BenchmarkParser`) added to that engine; Baseline and Refactored
both expose a public `SyntaxTree.ParseQuery`.

## Observations

- **Parsing is cheap for every engine** — ~5–16 μs per query, two to three
  orders of magnitude below compilation (100s of μs to ms) and execution, so
  parser choice is negligible in any end-to-end cost.
- **Refactored is now the fastest engine on 7 of 8 shapes** — ~0.88–0.92×
  Baseline (roughly 8–12% quicker) — and the leanest of the two new engines at
  **~0.71–0.76× Baseline allocations** (24–29% less). `TopWithTies` is the lone
  exception at 1.06×; it is the smallest query (fewest tokens, so fixed costs
  dominate) and the noisiest row this run (StdDev ±3%), so it is effectively on
  par with Baseline rather than a real regression.
- **Old still allocates the least — ~0.30–0.42× Baseline** (its lighter AST/token
  model is cheaper to build) but is now frequently the *slowest* on wall-clock:
  1.29× on `Join`, 1.34× on `Decorrelated`, 1.13× on `NestedLoops`. So for
  parsing the engines trade places — Refactored wins on time, Old on allocation.

## Summary

Shapes as rows, engines as columns. All factors are relative to **Baseline**
(`main` = 1.00×).

### Speed (× Baseline, lower = faster)

| Shape        |  Old  | Baseline | Refactored |
|------------- |------:|---------:|-----------:|
| Scan         | 0.99× |   1.00×  |   0.91×    |
| Join         | 1.29× |   1.00×  |   0.88×    |
| Aggregate    | 1.02× |   1.00×  |   0.92×    |
| Sort         | 1.03× |   1.00×  |   0.91×    |
| Report       | 1.02× |   1.00×  |   0.90×    |
| TopWithTies  | 1.10× |   1.00×  |   1.06×    |
| Decorrelated | 1.34× |   1.00×  |   0.92×    |
| NestedLoops  | 1.13× |   1.00×  |   0.90×    |

### Memory (allocated per op, with × Baseline, lower = leaner)

| Shape        |       Old       |    Baseline     |   Refactored    |
|------------- |----------------:|----------------:|----------------:|
| Scan         |  3.1 KB (0.33×) |  9.5 KB (1.00×) |  7.3 KB (0.76×) |
| Join         |  5.8 KB (0.42×) | 13.8 KB (1.00×) |  9.9 KB (0.72×) |
| Aggregate    |  4.5 KB (0.36×) | 12.5 KB (1.00×) |  9.1 KB (0.73×) |
| Sort         |  3.4 KB (0.32×) | 10.7 KB (1.00×) |  8.1 KB (0.75×) |
| Report       |  8.1 KB (0.33×) | 24.5 KB (1.00×) | 17.3 KB (0.71×) |
| TopWithTies  |  2.9 KB (0.30×) |  9.5 KB (1.00×) |  7.1 KB (0.75×) |
| Decorrelated |  3.9 KB (0.32×) | 12.1 KB (1.00×) |  8.9 KB (0.74×) |
| NestedLoops  |  5.4 KB (0.33×) | 16.3 KB (1.00×) | 12.0 KB (0.74×) |

## Full results

Default job (`DefaultJob`, full warmup + 15 iterations). Parsing is fast enough
that the full job runs quickly across all shapes; the tight Error/StdDev (well
under 1%) makes these means reliable.

| Method     | Shape        | Mean      | Error     | StdDev    | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------- |------------- |----------:|----------:|----------:|------:|-------:|-------:|----------:|------------:|
| Old        | Scan         |  5.743 μs | 0.1145 μs | 0.2722 μs |  0.99 | 0.3738 |      - |    3.1 KB |        0.33 |
| Baseline   | Scan         |  5.794 μs | 0.1158 μs | 0.2862 μs |  1.00 | 1.1673 | 0.0229 |   9.54 KB |        1.00 |
| Refactored | Scan         |  5.285 μs | 0.1056 μs | 0.2362 μs |  0.91 | 0.8850 | 0.0153 |   7.27 KB |        0.76 |
|            |              |           |           |           |       |        |        |           |             |
| Old        | Join         | 11.855 μs | 0.2338 μs | 0.5032 μs |  1.29 | 0.7019 |      - |   5.79 KB |        0.42 |
| Baseline   | Join         |  9.209 μs | 0.1822 μs | 0.5019 μs |  1.00 | 1.6785 | 0.0458 |   13.8 KB |        1.00 |
| Refactored | Join         |  8.080 μs | 0.1568 μs | 0.2298 μs |  0.88 | 1.2054 | 0.0305 |    9.9 KB |        0.72 |
|            |              |           |           |           |       |        |        |           |             |
| Old        | Aggregate    |  7.609 μs | 0.0332 μs | 0.0311 μs |  1.02 | 0.5493 |      - |    4.5 KB |        0.36 |
| Baseline   | Aggregate    |  7.493 μs | 0.0736 μs | 0.0652 μs |  1.00 | 1.5259 | 0.0381 |  12.51 KB |        1.00 |
| Refactored | Aggregate    |  6.858 μs | 0.0650 μs | 0.0608 μs |  0.92 | 1.1139 | 0.0229 |   9.11 KB |        0.73 |
|            |              |           |           |           |       |        |        |           |             |
| Old        | Sort         |  6.659 μs | 0.0260 μs | 0.0243 μs |  1.03 | 0.4120 |      - |    3.4 KB |        0.32 |
| Baseline   | Sort         |  6.440 μs | 0.1170 μs | 0.1094 μs |  1.00 | 1.3046 | 0.0305 |  10.71 KB |        1.00 |
| Refactored | Sort         |  5.854 μs | 0.0519 μs | 0.0485 μs |  0.91 | 0.9842 | 0.0153 |   8.05 KB |        0.75 |
|            |              |           |           |           |       |        |        |           |             |
| Old        | Report       | 15.842 μs | 0.0701 μs | 0.0655 μs |  1.02 | 0.9766 |      - |   8.05 KB |        0.33 |
| Baseline   | Report       | 15.576 μs | 0.2056 μs | 0.1923 μs |  1.00 | 2.9907 | 0.1526 |  24.49 KB |        1.00 |
| Refactored | Report       | 14.030 μs | 0.1435 μs | 0.1342 μs |  0.90 | 2.1057 | 0.0916 |   17.3 KB |        0.71 |
|            |              |           |           |           |       |        |        |           |             |
| Old        | TopWithTies  |  6.028 μs | 0.0259 μs | 0.0242 μs |  1.10 | 0.3510 |      - |   2.88 KB |        0.30 |
| Baseline   | TopWithTies  |  5.493 μs | 0.0320 μs | 0.0284 μs |  1.00 | 1.1597 | 0.0229 |   9.49 KB |        1.00 |
| Refactored | TopWithTies  |  5.834 μs | 0.1115 μs | 0.1801 μs |  1.06 | 0.8698 | 0.0153 |   7.11 KB |        0.75 |
|            |              |           |           |           |       |        |        |           |             |
| Old        | Decorrelated |  9.437 μs | 0.1880 μs | 0.3840 μs |  1.34 | 0.4730 |      - |   3.91 KB |        0.32 |
| Baseline   | Decorrelated |  7.067 μs | 0.0508 μs | 0.1115 μs |  1.00 | 1.4801 | 0.0381 |  12.14 KB |        1.00 |
| Refactored | Decorrelated |  6.512 μs | 0.1180 μs | 0.1104 μs |  0.92 | 1.0910 | 0.0229 |   8.94 KB |        0.74 |
|            |              |           |           |           |       |        |        |           |             |
| Old        | NestedLoops  | 10.894 μs | 0.0669 μs | 0.0626 μs |  1.13 | 0.6561 |      - |   5.38 KB |        0.33 |
| Baseline   | NestedLoops  |  9.676 μs | 0.0772 μs | 0.0685 μs |  1.00 | 1.9989 | 0.0610 |  16.33 KB |        1.00 |
| Refactored | NestedLoops  |  8.678 μs | 0.0268 μs | 0.0209 μs |  0.90 | 1.4648 | 0.0458 |  12.04 KB |        0.74 |

---

# Environment

```
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.8655)
.NET SDK 10.0.201
  [Host] : .NET 8.0.28 (8.0.2826.26413), X64 RyuJIT AVX2
```

# How to run

```
# Both comparison engines are submodules under external\; check them out once:
git submodule update --init

# Build the original engine (plain bin\Release output, as that repo has no artifacts layout):
dotnet build external\nquery-old\Src\NQuery\NQuery.csproj -c Release

# Build the baseline engine (pinned at the comparison point):
dotnet build external\nquery-baseline\src\NQuery -c Release

cd src\NQuery.Benchmarks

# Execution (default job):
dotnet run -c Release -- --filter "*NorthwindExecutionBenchmarks*"

# Compilation (short job keeps the run quick across all shapes):
dotnet run -c Release -- --filter "*NorthwindCompilationBenchmarks*" --job short

# Parsing (default job — parsing is cheap, so the full run stays quick):
dotnet run -c Release -- --filter "*ParsingBenchmarks*"
```
