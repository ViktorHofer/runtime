# March 2021

Welcome to the March 2021 changelog of dotnet/runtime's infrastructure. There are a number of updates that we hope you will like, some of the key highlights include:
- :heavy_check_mark: [Unified GitHub project board](https://github.com/dotnet/runtime/projects/26) to track all infrastructure related work
- :heavy_check_mark: [Default branch has been renamed to main](https://github.com/dotnet/runtime/issues/48357)
- :heavy_check_mark: [Improve coreclr windows native build time](https://github.com/dotnet/runtime/issues/33872)
- :heavy_check_mark: [Enable building repo on Windows ARM64](https://github.com/dotnet/runtime/pull/49864)
- :heavy_check_mark: [Creating a separate pipeline to stage onboarding new test targets](https://github.com/dotnet/runtime/issues/44031)

### :bulb: Enhancements
- [Consolidate VS detection logic](https://github.com/dotnet/runtime/pull/49593)
- [Make sure event generation is incremental](https://github.com/dotnet/runtime/pull/48903)
- [Move dotnet-sdk snaps into the installer repo and dotnet-runtime snaps into servicing branches](https://github.com/dotnet/runtime/issues/49374)
- [Port Microsoft.Extensions.* from pkgproj to dotnet pack](https://github.com/dotnet/runtime/pull/48385)
- [Update debug dump template after runfo update to include dumps](https://github.com/dotnet/runtime/pull/49183)
- [Update the minimum version of the SDK to 6.0 Preview 2](https://github.com/dotnet/runtime/pull/50084)
- [Reorganize CoreCLR native build to reduce CMake reconfigures when the build system is untouched](https://github.com/dotnet/runtime/pull/49906)

### :x: Bug fixes
- [Code coverage is broken](https://github.com/dotnet/runtime/issues/49172)
- [Error when building using the -ninja flag](https://github.com/dotnet/runtime/issues/47314)
- [Unable to build repo on latest VS dogfood](https://github.com/dotnet/runtime/issues/49646)
- [The repo fails to build when source control information isn't available](https://github.com/dotnet/runtime/issues/47130)
- [Trimming tests fail with race condition](https://github.com/dotnet/runtime/issues/40398)

## Unified GitHub project board
Since consolidation of repositories into dotnet/runtime, we used to have different infrastructure boards to track work. We recently consolidated those into a [single Infrastructure board](https://github.com/dotnet/runtime/projects/26) which is kept up-to-date via webhooks.

The consolidated board allows to
- Keep the number of untriaged issues low
- Track Servicing, 6.0 and Future work
- Summarize accomplishments ("Done" column) over the last month which ultimately results in this changelog

As you can see in the image below, the number of infrastructure issues is quite high. We will start triaging these more aggressively starting soon.

![image](https://user-images.githubusercontent.com/7412651/112136833-cd130900-8bcf-11eb-95a3-d632fcafa496.png)

## Default branch has been renamed to main
The default branch in the runtime, runtime-assets, runtimelab, jitutils and other repositories that we own [has been renamed to main](https://github.com/dotnet/runtime/issues/48357).

Besides a call to action to rename forked repositories' default branch to main, there wasn't any noticeable impact. Some team members even reached out to us and expressed their gratitude.

:memo: If you haven't renamed your fork's default branch to main, we strongly advise to do so.

## Improve CoreCLR Windows native build time
Building the CoreClr runtime on Windows was reported as unsatisfying, as the build takes quite long (up to 30 minutes depending on the hardware used) and it makes the machine unusable during that time.

MSBuild's support for effective parallelisation is limited. To make builds as fast as possible, tools like Ninja are recommended to be used. Because of that, support for Ninja over MSBuild was added months ago in the runtime repository via the "-ninja" build switch. Depending on the hardware being used, the improvements vary between 10-50%.

:bulb: What just recently changed is that Ninja is now the [default CMake generator on Windows](https://github.com/dotnet/runtime/pull/49715).

## Enable building repo on Windows ARM64
The runtime repository failed to build on Windows ARM64 devices like the Surface Pro X, mainly because the wrong set of dac tools were chosen. This was  [just recently fixed](https://github.com/dotnet/runtime/pull/49864).

:warning: The x86 emulation of Git for Windows doesn't work yet on Windows ARM64. Native ARM64 execution is expected to be official supported in one of the following month's releases. See https://github.com/git-for-windows/git/issues/3107 for more details.

## Creating a separate pipeline to stage onboarding new test targets
Even though the [runtime-staging pipeline](https://dnceng.visualstudio.com/public/_build?definitionId=924) has already been running as part of the CI for serveral months, we never officially announced it.

In the past, when new configurations (i.e. Windows ARM64 or WASM) were added to the repository, they often regressed reliability of the main runtime pipeline. In addition to that, we often encounter hardware issues (i.e. limited number of devices, malfunctioning hardware, bad rollouts) which require the flexibility to move configurations out of the main runtime pipeline and "quarantine" them into a separate pipeline.

For that purpose the runtime-staging pipeline was added to the repository and runs as part of every Pull Request and Rolling Build.

:information_source: Test failures happening in that pipeline don't impact the overall status of the PR.

:warning: Configurations ("legs") in that pipeline which time out (i.e. because of limited devices being available) count as failures and impact the Pull Requests's status.
