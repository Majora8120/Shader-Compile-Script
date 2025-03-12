A script that scans a folder and its subfolders for shaders and compiles them to spir-v with glslc or slangc.
Tested on Windows and Ubuntu. Should work fine on other platforms but no guarantee.

Usage:
-dir [directory path] search directory (Required).
-glslc [file path] glslc path (Optional).
-slangc [file path] slangc path (Optional).

Outputs .spv file in the same directory as the inputted shader file.

Notice:
Does not support custom arguments for glslc or slangc.
Does pass some hardcoded arguments to slangc. You probably need to modify them to compile your shaders.
Error messages from glslc and slangc will not be in order.

Supported input file extensions:
Glslc:
.vert
.frag
.tesc
.tese
.geom
.comp
Slangc:
.slang

Building:
1. Install .Net SDK 8.0 or higher.
2. Download source code.
3. Run: dotnet build "[sourceDir]/Shader Compile Script.csproj" -r [platform]