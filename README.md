# Shader Compile Script
A script that scans a folder and its subfolders for shaders and compiles them using [slangc](https://github.com/shader-slang/slang).
<br>
Using glslc is a legacy feature and won't be supported.
<br>
Tested on Windows 10. Should work fine on other platforms but no guarantee.
<br>

## Usage
-dir `directory path` search directory (Required).
<br>
<br>
-glslc `file path` glslc path (Optional).
<br>
<br>
-slangc `file path` slangc path (Optional).
<br>
<br>
-max_batch_size `uint` maximum amount of shaders compiling at once<sup>1</sup> (Optional).
<br>
#### Slangc exclusive commands:
-slangc_target `slangc target` slangc compile target<sup>2</sup> (Required).
<br>
<br>
-slangc_in_ext `file extension(s)` file extension(s) to search for (Required).
<br>
<br>
-slangc_out_ext `file extension` file extension for the outputted shader files (Required).
<br>
<br>
-slangc_args `slangc argument(s)` passes custom arguments to slangc<sup>2,3</sup> (Optional).
<br>
#### Notes:
<sup>1</sup>Max amount of slangc/glslc instances compiling shaders at once.
Larger values use more memory.
Defaults to 10 if no argument is provided.
<br>
<br>
<sup>2</sup>Slangc command arguments can be found [here](https://github.com/shader-slang/slang/blob/master/docs/command-line-slangc-reference.md).
<br>
<br>
<sup>3</sup>An input path, output path `-o`, and target `-target` are already passed to slangc by the script. I would not recommend passing them using -slangc_args.
<br>
#### Example:
`-dir ./ -slangc "C:\VulkanSDK\1.4.304.1\Bin\slangc.exe" -slangc_target spirv -slangc_in_ext "slang glsl hlsl" -slangc_out_ext spv -slangc_args "-matrix-layout-row-major -fvk-use-entrypoint-name"`
<br>

## Building
1. Install .Net SDK 8.0 or higher.
2. Download source code.
3. Run: `dotnet build "[sourceDir]/Shader Compile Script.csproj" -r [platform]`
