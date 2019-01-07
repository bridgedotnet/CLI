# Bridge CLI

A Command Line Interface for the [Bridge.NET](https://bridge.net) compiler.

## Usage

```
bridge [commands] [options]
bridge [options] (<name>|<path>|<list>)
bridge [-h|--help]
```

The following steps outline the commands for creating a new C# project using the Bridge CLI, then opening the project in **Visual Studio Code**, and then running your app in a web browser. Here's what we'll do:

1. Create a new folder called **Demo1** (rename to whatever you want)
2. Move the command prompt into the folder
3. Create a new Bridge project using the default project template
4. Open the project in **Visual Studio Code**
5. Build (compile) the project
6. Run the app in your default web browser

[Bridge CLI](https://bridge.net/download) and [Visual Studio Code](https://code.visualstudio.com/) must have already been installed for this sample to work. We use **Visual Studio Code** in this sample, but you can substitue for your editor of choice.

#### Example (Windows Command Prompt)

```
md Demo1                    // Create a new folder
cd Demo1                    // Move into the folder
bridge new                  // Create a new project
code .                      // Open in Visual Studio Code
bridge build                // Build the project, or
bridge run                  // Build and Run the index.html
```

#### Example (Mac Terminal)

```
mkdir Demo1                 // Create a new folder
cd Demo1                    // Move into the folder
bridge new                  // Create a new project
code .                      // Open in Visual Studio Code
bridge build                // Build the project, or
bridge run                  // Build and Run the index.html
```

After successfully building your project, you should see a new **/dist** folder in your project root. These are the Bridge generated files, including your **index.html** file which can be opened in a browser, or just use the `bridge run` command from the Windows Command line or Mac Terminal.

**PRO TIP:** If you're using **Visual Studio Code**, after opening the project, you can use <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>B</kbd> to build your project without leaving VS Code.

## Commands

Command | Description
---- | ----
new | Initialize a valid Bridge C# Class Library project.
new &lt;template> | Initialize a new project based on the named Template. Default is classlib.
build | Builds the Bridge project.
run | Compiles and immediately runs the index.html file.
add package &lt;name> | Add a package from NuGet.org to the project.
remove package &lt;name> | Removes a Nuget package from the project.
add repo "&lt;path>" | Add a package source location.
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
