# Bridge CLI

A Command Line Interface for the [Bridge.NET](https://bridge.net) compiler.

## Usage

```
bridge [commands] [options]
bridge [options] (<name>|<path>|<list>)
bridge [-h|--help]
```

#### Example

```
md Demo1                    // Create a new folder
cd Demo1                    // Move into the folder
bridge new                  // Create a new project
bridge add package retyped  // Add retyped
bridge build                // Build the project
bridge run                  // Run the project index.html
```

## Commands

Command | Description
---- | ----
new | Initialize a valid Bridge C# Class Library project.
new &lt;template> | Initialize a new project based on the named Template. Default is classlib.
build | Builds the Bridge project.
run | Compiles and immediately runs the index.html file.
add package &lt;name> | Add a package from NuGet.org to the project.
remove package &lt;name> | Removes a Nuget package from the project.
restore | Restores all NuGet packages.


## Common Options

Option | Description
---- | ----
-h or --help | Show help.
-v or --version | Show version.

## Options

Option | Description
---- | ----
-c --configuration &lt;name> | Configuration name (Debug/Release etc).
-D --define &lt;list> | Semicolon-delimited list of project constants.
-f --folder &lt;path> | Builder working directory relative to current dir.
-h --help | Show help.
-p --project &lt;path> | The .csproj file location.
-P --platform &lt;name> | Platform name (AnyCPU etc).
-r --rebuild | Force Assembly rebuilding.
-R --recursive | Recursively search for .cs source files.
-s --source &lt;file> | Source files name/pattern [default: *.cs].
-S --settings &lt;name:value> Comma-delimited list of project settings.
-v --version | Show version.
--nocore | Do not extract core javascript files.
--norecursive | Non-recursive search for .cs source files.
--notimestamp | Do not show timestamp in log messages.

 More information on getting started with Bridge.NET at https://bridge.net/docs.
