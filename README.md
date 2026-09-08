# Idlegame

A WPF desktop app targeting .NET Framework 4.7.2.

## Requirements

- Windows
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with the ".NET desktop development" workload
- .NET Framework 4.7.2 Developer Pack (installed automatically by the VS workload if missing)

## Build & run (Visual Studio)

1. Open `Idlegame/Idlegame.slnx` in Visual Studio.
2. Press `F5` (or `Ctrl+F5` to run without debugging).

## Build & run (command line)

From a Developer Command Prompt / Developer PowerShell for VS 2022:

```
msbuild Idlegame\Idlegame.slnx /p:Configuration=Debug
Idlegame\Idlegame\bin\Debug\Idlegame.exe
```

Use `/p:Configuration=Release` for a release build.
