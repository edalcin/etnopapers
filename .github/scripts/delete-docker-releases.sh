#!/bin/bash
# Script to delete legacy Docker releases from GitHub
# Usage: GITHUB_TOKEN=your_token bash delete-docker-releases.sh

REPO="edalcin/etnopapers"
GITHUB_API="https://api.github.com/repos/$REPO"

if [ -z "$GITHUB_TOKEN" ]; then
  echo "Error: GITHUB_TOKEN environment variable is not set"
  echo "Usage: GITHUB_TOKEN=your_token bash delete-docker-releases.sh"
  exit 1
fi

echo "Fetching all releases from $REPO..."

# Get all releases
RELEASES=$(curl -s -H "Authorization: token $GITHUB_TOKEN" \
  "$GITHUB_API/releases" | grep '"id"' | awk -F'[: ]' '{print $4}')

if [ -z "$RELEASES" ]; then
  echo "No releases found or API error occurred"
  exit 1
fi

echo "Found releases. Checking each one..."

# For each release, check if it's a docker release and delete it
for RELEASE_ID in $RELEASES; do
  # Get release details
  RELEASE_DATA=$(curl -s -H "Authorization: token $GITHUB_TOKEN" \
    "$GITHUB_API/releases/$RELEASE_ID")

  TAG=$(echo "$RELEASE_DATA" | grep '"tag_name"' | head -1 | cut -d'"' -f4)
  BODY=$(echo "$RELEASE_DATA" | grep '"body"' | head -1 | cut -d'"' -f4)

  # Check if release body contains docker-related keywords
  if echo "$BODY" | grep -qi "docker\|container\|image"; then
    echo "Deleting release: $TAG (ID: $RELEASE_ID)"
    curl -s -X DELETE \
      -H "Authorization: token $GITHUB_TOKEN" \
      "$GITHUB_API/releases/$RELEASE_ID"
    echo " ✓ Deleted"
  else
    echo "Skipping release: $TAG (not a Docker release)"
  fi
done

echo "Done!"
