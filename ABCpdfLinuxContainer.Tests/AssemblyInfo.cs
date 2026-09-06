using Xunit.Sdk;
using Xunit.v3;

// Tests exercise real environment variables (Environment.SetEnvironmentVariable is process-wide,
// mutable, shared state) and a real Docker container, so classes must not run concurrently.
[assembly: Parallelization(Mode = ParallelMode.None)]
