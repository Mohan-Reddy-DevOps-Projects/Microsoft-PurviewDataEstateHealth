// <copyright file="LocationUtils.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetLocation
{
    using System;

    public static class LocationUtils
    {
        public static string BuildQualifiedPathForWildcardLocation(string root, string wildcardPath)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (string.IsNullOrEmpty(wildcardPath))
            {
                return $"{root.ToLowerInvariant()}/";
            }
            var pathSegments = wildcardPath.Split("/");
            int i = 0;
            for (i = 0; i < pathSegments.Length; i++)
            {
                if (pathSegments[i].Contains('*', StringComparison.OrdinalIgnoreCase) ||
                    pathSegments[i].Contains('?', StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }

            // if the path is like a/b/c/*, we want to return a/b/c/
            if (i < pathSegments.Length)
            {
                pathSegments[i] = string.Empty;
                ++i;
            }
            pathSegments = pathSegments[0..i];
            return $"{root}/{string.Join("/", pathSegments)}";
        }
    }
}
