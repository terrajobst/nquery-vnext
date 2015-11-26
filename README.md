# NQuery vNext

[![dev version](http://img.shields.io/appveyor/ci/terrajobst/nquery-vnext.svg?style=flat)](https://ci.appveyor.com/project/terrajobst/nquery-vnext)

This is a Roslyn inspired rewrite of [NQuery][nquery]. The goals are:

1. Expose the internals of the compiler as an object model to support building
   advanced editor features.

2. Maintain language parity with the previous version.

3. Utilize modern idioms to simplify implementation of the compiler.

[nquery]: https://www.github.com/terrajobst/nquery

## Getting NQuery

If you want to play with the bits, you need to add the following feed to your
NuGet configuration:

```
https://ci.appveyor.com/nuget/nquery-vnext
```

You can do this by using the [package manager dialog](http://docs.nuget.org/consume/package-manager-dialog#package-sources).
