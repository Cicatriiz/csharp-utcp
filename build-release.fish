#!/usr/bin/env fish

# Build Release Artifacts Script for C# UTCP
# Usage: ./build-release.fish

set script_dir (dirname (status --current-filename))
set project_root $script_dir
set output_dir "$project_root/release-artifacts"
set version "1.0.0"

# Check if dotnet is available
if not command -v dotnet >/dev/null 2>&1
    echo "❌ dotnet CLI not found in PATH"
    echo "Please install .NET SDK or add it to your PATH"
    echo ""
    echo "Common locations to check:"
    echo "  - /usr/share/dotnet/dotnet"
    echo "  - /snap/bin/dotnet"
    echo "  - ~/.dotnet/dotnet"
    echo ""
    echo "To add to PATH temporarily:"
    echo "  set -x PATH /usr/share/dotnet \$PATH"
    exit 1
end

set dotnet_version (dotnet --version 2>/dev/null || echo "unknown")
echo "Using .NET version: $dotnet_version"
echo ""

# Clean previous builds
echo "🧹 Cleaning previous builds..."
if test -d $output_dir
    rm -rf $output_dir
end
mkdir -p $output_dir

# Clean solution
dotnet clean -c Release
if test $status -ne 0
    echo "❌ Failed to clean solution"
    exit 1
end

echo "✅ Cleaned previous builds"
echo ""

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore
if test $status -ne 0
    echo "❌ Failed to restore dependencies"
    exit 1
end
echo "✅ Dependencies restored"
echo ""

# Build solution
echo "🔨 Building solution in Release mode..."
dotnet build -c Release --no-restore
if test $status -ne 0
    echo "❌ Build failed"
    exit 1
end
echo "✅ Solution built successfully"
echo ""

# Create NuGet packages
echo "📦 Creating NuGet packages..."
dotnet pack -c Release --no-build --output $output_dir
if test $status -ne 0
    echo "❌ Failed to create NuGet packages"
    exit 1
end
echo "✅ NuGet packages created"
echo ""

# For class libraries, we mainly need the NuGet package
# But let's create a simple binary release too
echo "📦 Creating binary release..."

set binary_dir "$output_dir/binaries"
mkdir -p $binary_dir

# Find the main project file (assuming single project or main one)
set project_files (find . -name "*.csproj" -not -path "./tests/*" -not -path "./*test*/*")

if test (count $project_files) -eq 0
    echo "⚠️  No .csproj files found, skipping binary release"
else if test (count $project_files) -gt 1
    echo "📋 Multiple projects found, using first one: $project_files[1]"
    set main_project $project_files[1]
else
    set main_project $project_files[1]
end

if test -n "$main_project"
    # Build the library
    dotnet build $main_project -c Release --output $binary_dir
    if test $status -eq 0
        # Create zip of built library
        set binary_zip "csharp-utcp-v$version-library.zip"
        cd $binary_dir
        zip -r "../$binary_zip" ./* >/dev/null 2>&1
        set zip_status $status
        cd $project_root

        if test $zip_status -eq 0
            echo "✅ Library binaries -> $binary_zip"
            # Clean up loose files
            rm -rf $binary_dir
        else
            echo "⚠️  Failed to create binary zip"
        end
    else
        echo "⚠️  Failed to build library binaries"
    end
end

echo ""

# Create source archive
echo "📚 Creating source archive..."
set -l source_zip "csharp-utcp-v$version-source.zip"
git archive --format=zip --output="$output_dir/$source_zip" HEAD
if test $status -eq 0
    echo "✅ Source archive created: $source_zip"
else
    echo "⚠️  Failed to create source archive (git archive failed)"
end

echo ""

# List all artifacts
echo "📋 Release artifacts created:"
echo "================================"
for file in $output_dir/*
    set -l filename (basename $file)
    set -l filesize (du -h $file | cut -f1)
    echo "  📦 $filename ($filesize)"
end

echo ""
echo "🎉 Release build complete!"
echo "📁 Artifacts location: $output_dir"
echo ""
echo "Next steps:"
echo "  1. Test the built artifacts"
echo "  2. Create GitHub release with these files"
echo "  3. Optionally publish NuGet packages:"
echo "     dotnet nuget push $output_dir/*.nupkg --source https://api.nuget.org/v3/index.json"
