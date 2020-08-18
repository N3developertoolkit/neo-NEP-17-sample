# Neo Trace Debugging Sample

This repo contains files needed to demonstrate Trace Debugging, a new feature coming in Neo Blockchain Toolkit for Neo 3.

Typically, when debugging a Neo Smart Contract, the debugger launches an instance of [NeoVM](https://github.com/neo-project/neo-vm)
to run execute the contract. While the debugger can simulate much of the Neo blockchain environment while debugging, there is
also a need to be able to debug contracts that are deployed to a production Neo instance such as MainNet or TestNet.
In those cases, traditional debugging is not feasible. You can't pause MainNet in order to attach a debugger!

For these production blockchain scenarios, Neo Blockchain Toolkit for Neo 3 introduces trace debugging. With trace debugging, you
capture a binary trace of the NeoVM execution directly in place. The Neo 3 platform has added a mechanism (`IApplicationEngineProvider`)
to enable plugin developers to replace the NeoVM engine used for contract execution with one that has been updated to capture these
binary trace files. Neo Express is the first tool to support trace capture, but there will eventually be an official Neo plugin for
neo-cli/gui that captures trace files. Once captured, trace files can be used to drive the debug experience. You still need to have
the compiled contract file and debug info files (`.nef` and `.negdbgnfo`) in order to trace debug the contract.

The Neo Debugger has been updated to support these trace capture files. In a way, trace debugging is like debugging a recording of
the contract execution. Because of this recording type nature, trace debugging supports Step Back and Reverse Run in the VS Code
debugger. These features are not supported with traditional contract debugging.

This repo contains a NEP-5 Neo 3 smart contract named [Apoc](https://en.wikipedia.org/wiki/List_of_Matrix_series_characters#Apoc)
along with the trace debug and checkpoint files needed to try out the new Trace Debugger support coming in Neo Blockchain Toolkit
for Neo 3.

## Trace Debugger Prerequisites

In order to try this out, you first have to install the "Neo 3 Preview 3 Refresh" (or later) release of
[neo-debugger](https://github.com/neo-project/neo-debugger/releases). Download the .VSIX file from GitHub
and install it as per the [official VSCode documentation](https://code.visualstudio.com/docs/editor/extension-gallery#_install-from-a-vsix).

> Note, you do not need to manually install the NEON3 package.

Next, clone this repo locally and open it in VSCode. Before we can debug the contract (via traditional or trace
debugging) we need to compile the contract and generate the debug info file. VSCode has a build task named `neon` that
you can execute to compile the contract. This task will do three things:

1. Make sure the Neo 3 preview editions of NEON and Neo Express are installed.
2. Compile the Apoc contract to a managed dll
3. Run NEON on the compiled dll to produce the smart contract binary and other needed files.

If you wish, you can execute these steps manually at the command prompt:

``` shell
> dotnet tool restore
> dotnet build contract
> dotnet neon -f contract/bin/Debug/netstandard2.1/Apoc.dll
```

> Note, you need to prefix `neon` with `dotnet` because this repo is configured to install NEON as a
> [.NET Local Tool](https://docs.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use).

## Trying Out the Trace Debugger

This repo comes with four launch configurations:

- deploy
- deploy (fault)
- deploy (trace)
- deploy (trace + fault)

Each of these configurations runs the `Deploy` contract operation that is intended to be run by the
token owner after the contract has been deployed. The `Deploy` operation can only be run once. It
throws an exception if the token owner attempts to run it more than once.

The four deploy scenarios represent first and second run of the Deploy contract, both via traditional
and trace debugging. As you run the four scenarios, note that the developer experience is basically
identical for traditional and trace debugging of a given scenario. Features like variable inspection,
watch window and disassembler view work the same for traditional and trace debugging.

> Note, Also note that the debugger for Neo 3 has also been updated to support exception breakpoints.
> By default, uncaught exceptions trigger a breakpoint, though you can configure the debugger to
> also break on caught exceptions.

## Capturing a Debug Trace

Neo Express for Neo 3 has been augmented to support trace capture. The `run` and `checkpoint run`
commands have a new `--trace` option to configure trace capture. When tracing is enabled,
Neo Express captures a trace of every Transaction run with the Application trigger. The trace
file is saved in the same location as the `.neo-express` file and uses `{transaction-hash}.neo-trace`
for a naming convention.

> Verification Trigger debugging will be supported in a future version of Neo Debugger.