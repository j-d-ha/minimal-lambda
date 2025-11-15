#!/bin/bash

# Script to validate that a package version doesn't already exist on NuGet

# Get the directory from the first argument, default to src/AwsLambda.Host
DIR="${1:-src/AwsLambda.Host}"

# Change to the project directory
cd "$DIR" || exit 1

# Extract package ID from .csproj (BSD grep compatible)
PACKAGE_ID=$(grep -o '<PackageId>[^<]*</PackageId>' *.csproj | head -1 | sed 's/<PackageId>\(.*\)<\/PackageId>/\1/')

# Extract version from Directory.Build.props (BSD grep compatible)
VERSION=$(grep -o '<VersionPrefix>[^<]*</VersionPrefix>' ../../Directory.Build.props | head -1 | sed 's/<VersionPrefix>\(.*\)<\/VersionPrefix>/\1/')

if [[ -z "$PACKAGE_ID" ]] || [[ -z "$VERSION" ]]; then
  echo "Error: Could not extract PackageId or VersionPrefix" >&2
  exit 1
fi

echo "Checking if $PACKAGE_ID v$VERSION already exists on NuGet..." 

# Convert package ID to lowercase (compatible with bash 3.2)
PACKAGE_ID_LOWER=$(echo "$PACKAGE_ID" | tr '[:upper:]' '[:lower:]')

# Check if package version exists on NuGet
STATUS_CODE=$(curl -s -o /dev/null -w "%{http_code}" "https://api.nuget.org/v3-flatcontainer/$PACKAGE_ID_LOWER/$VERSION/$PACKAGE_ID_LOWER.$VERSION.nupkg")

if [[ "$STATUS_CODE" = "200" ]]; then
  echo "Error: $PACKAGE_ID v$VERSION already exists on NuGet.org" >&2
  exit 1
elif [[ "$STATUS_CODE" = "404" ]]; then
  echo "✓ $PACKAGE_ID v$VERSION is available (not published yet)"
  exit 0
else
  echo "Warning: Unexpected status code $STATUS_CODE when checking NuGet"
  exit 1
fi