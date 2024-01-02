#This script initializes a developer environment
git pull origin main

#This adds the share solution as a safe directory - needed for recent git update.

$gitDir = git rev-parse --show-toplevel
git config --global --add safe.directory $gitDir