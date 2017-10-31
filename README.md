# Bridge CLI

A Command Line Interface for the [Bridge.NET](https://bridge.net) compiler.

### Usage

```
bridge [runtime-options] [path-to-application]
bridge [sdk-options] [command] [arguments] [command-options]
```

#### Example

```
bridge new                  // Create a new project
bridge add package retyped  // Add retyped
bridge Build                // Build the project
bridge run                  // Run the project index.html
```

### SDK Commands

Command | Description
---- | ----
new | Initialize a valid Bridge C# Class Library project.
new &lt;template> | Initialize a new project based on the named Template. Default is classlib.
build | Builds the Bridge project.
run | Compiles and immediately runs the index.html file.
add package &lt;name> | Add a package from NuGet.org

### Common Options

Option | Description
---- | ----
-h or --help | Show help.
-v or --version | Version of Bridge compiler.

### Runtime Options

Option | Description
---- | ----
-c --configuration &lt;name> | Configuration name (Debug/Release etc). Default is none.
-P --platform &lt;name> | Platform name (AnyCPU etc) [default: none].
-S --settings &lt;name:value> | Comma-delimited list of project settings [`-S name1:value1,name2:value2`). List of allowed settings: `AssemblyName`, `CheckForOverflowUnderflow`, `Configuration`, `DefineConstants`, `OutputPath`, `OutDir`, `OutputType`, `Platform`, `RootNamespace`. Options `-c`, `-P` and `-D` have priority over `-S`
-r --rebuild | Force Assembly rebuilding.
--nocore | Do not extract core javascript files.
-D --define &lt;const-list> | Semicolon-delimited list of project constants.
-b --bridge &lt;file> | Bridge.dll file location (currently unused).
-s --source &lt;file> | Source files name/pattern [default: *.cs].
-f --folder &lt;path> | Builder working directory relative to current WD. [default: current wd]
-R --recursive | Recursively search for .cs source files inside current working directory.
--norecursive | Non-recursive search of .cs source files inside current working directory.
-notimestamp --notimestamp | Do not show timestamp in log messages. Default is to show timestamp.
