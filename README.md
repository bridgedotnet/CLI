# Bridge CLI

A Command Line Interface for the [Bridge.NET](https://bridge.net) compiler.

### SDK Commands

Command | Description
---- | ----
new | Initialize a valid Bridge C# Class Library project. 
new <template> | Initialize a new project based on the named Template. Default is `classlib`.
build | Builds the Bridge project. 
run | Compiles and immediately runs the index.html file.
add package <name> | Add a package from NuGet.org

### Usage

```
bridge [runtime-options] [path-to-application]
bridge [sdk-options] [command] [arguments] [command-options]
```

### Common Options

Option | Description
---- | ----
-h or --help | Show help.

### Runtime Options

Option | Description
---- | ----
-c --configuration <name> | Configuration name (Debug/Release etc). [default: none]
-P --platform <name> | Platform name (AnyCPU etc) [default: none].
-S --settings <name:value> | Comma-delimited list of project settings [`-S name1:value1,name2:value2`). List of allowed settings: `AssemblyName`, `CheckForOverflowUnderflow`, `Configuration`, `DefineConstants`, `OutputPath`, `OutDir`, `OutputType`, `Platform`, `RootNamespace`. Options `-c`, `-P` and `-D` have priority over `-S`
-r --rebuild | Force assembly rebuilding.
--nocore | Do not extract core javascript files.
-D --define <const-list> | Semicolon-delimited list of project constants.
-b --bridge <file> | Bridge.dll file location (currently unused).
-s --source <file> | Source files name/pattern [default: *.cs].
-f --folder <path> | Builder working directory relative to current WD. [default: current wd]
-R --recursive | Recursively search for .cs source files inside current working directory.
--norecursive |Non-recursive search of .cs source files inside current working directory.
-v --version | Version of Bridge compiler.
-notimestamp --notimestamp | Do not show timestamp in log messages. [default: shows timestamp]
