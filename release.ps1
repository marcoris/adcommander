#!/usr/bin/env pwsh
# release.ps1 - bump the .csproj version, commit, tag and push in one step.
#
# Usage (from the repo root):
#   .\release.ps1 0.3.0
#
# This sets <Version> in the .csproj, commits it as "Release v0.3.0",
# creates the tag v0.3.0 and pushes master + the tag. The tag push triggers
# the release workflow on GitHub.

param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version
)

$ErrorActionPreference = 'Stop'
$tag = "v$Version"

# Locate the single application .csproj (ignore the test project if one exists).
$proj = Get-ChildItem -Recurse -Filter *.csproj |
    Where-Object { $_.FullName -notmatch '[\\/].*[Tt]ests?[\\/]' } |
    Select-Object -First 1
if (-not $proj) { throw "No .csproj found." }

# Refuse if the tag already exists (locally or remotely).
if (git tag --list $tag) { throw "Tag $tag already exists locally." }

# Make sure the working tree is clean before we start.
if (git status --porcelain) { throw "Working tree is not clean. Commit or stash first." }

# Update (or insert) <Version>, keeping the file's formatting.
$content = Get-Content -Raw -- $proj.FullName
if ($content -match '<Version>.*?</Version>') {
    $content = [regex]::Replace($content, '<Version>.*?</Version>', "<Version>$Version</Version>")
}
elseif ($content -match '</TargetFramework>') {
    $content = [regex]::Replace($content, '(</TargetFramework>)', "`$1`r`n    <Version>$Version</Version>", 1)
}
else {
    throw "Could not find a place to insert <Version> in $($proj.Name)."
}

# Write back as UTF-8 with BOM (matches the existing file), no extra newline juggling.
[System.IO.File]::WriteAllText($proj.FullName, $content, (New-Object System.Text.UTF8Encoding $true))
Write-Host "Set version $Version in $($proj.Name)" -ForegroundColor Green

# Commit, tag, push master + tag.
git add -- $proj.FullName
git commit -m "Release $tag"
git tag $tag
git push
git push origin $tag

Write-Host "Done. Pushed commit + tag $tag - the release workflow is building now." -ForegroundColor Green
