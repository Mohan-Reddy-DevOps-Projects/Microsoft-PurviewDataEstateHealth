// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Extension methods to resolve task dependencies
/// </summary>
public static class DependencyExtensions
{
    /// <summary>
    /// Determines execution order based on task dependencies.
    /// This is parallel scheduling algorithm using topological sort.
    /// </summary>
    /// <param name="dependsOn">The dictionary representing tasks and their dependencies.</param>
    /// <returns>The tasks execution order.</returns>
    public static List<List<string>> ResolveDependencies(this Dictionary<string, List<string>> dependsOn)
    {
        if (dependsOn == null)
        {
            return null;
        }

        List<List<string>> output = new List<List<string>>();
        Dictionary<string, HashSet<string>> dependencies = new Dictionary<string, HashSet<string>>();

        foreach (KeyValuePair<string, List<string>> entry in dependsOn)
        {
            string dependent = entry.Key.ToLowerInvariant();
            HashSet<string> existingEntry = dependencies.TryGetValue(dependent, out HashSet<string> entries) ? entries : null;
            if (existingEntry == null)
            {
                dependencies.Add(dependent, new HashSet<string>());
            }

            foreach (string parent in entry.Value)
            {
                string normalizedParent = parent.ToLowerInvariant();
                HashSet<string> existingDependents = dependencies.TryGetValue(normalizedParent, out HashSet<string> dependents) ? dependents : new HashSet<string>();
                existingDependents.Add(dependent);
                dependencies.Remove(normalizedParent);
                dependencies.Add(normalizedParent, existingDependents);
            }
        }

        Dictionary<string, int> indegrees = new Dictionary<string, int>();

        foreach (KeyValuePair<string, HashSet<string>> entry in dependencies)
        {
            string parent = entry.Key.ToLowerInvariant();
            int currentParentIndegree = indegrees.TryGetValue(parent, out int parentIndegree) ? parentIndegree : 0;
            indegrees.Remove(parent);
            indegrees.Add(parent, currentParentIndegree);

            foreach (string dependent in entry.Value)
            {
                string normalizedDependent = dependent.ToLowerInvariant();
                int currentIndegree = indegrees.TryGetValue(normalizedDependent, out int indegree) ? indegree : 0;
                indegrees.Remove(normalizedDependent);
                indegrees.Add(normalizedDependent, currentIndegree + 1);
            }
        }

        List<string> rootTasks = FindIndependentTasks(indegrees);

        output.Add(new List<string>(rootTasks));
        Queue<string> queue = new Queue<string>(rootTasks);

        while (queue.Count != 0)
        {
            int currentCount = queue.Count;
            List<string> independentTasks = new List<string>();
            for (int i = 0; i < currentCount; i++)
            {
                string task = queue.Dequeue();
                HashSet<string> currentDependents = dependencies.TryGetValue(task, out HashSet<string> dependents) ? dependents : null;
                if (currentDependents != null)
                {
                    foreach (string dependentTask in currentDependents)
                    {
                        int currentIndegree = indegrees.TryGetValue(dependentTask, out int indegree) ? indegree : 0;
                        if (currentIndegree == 1)
                        {
                            queue.Enqueue(dependentTask);
                            independentTasks.Add(dependentTask);
                        }
                        indegrees.Remove(dependentTask);
                        indegrees.Add(dependentTask, currentIndegree - 1);
                    }
                }
            }

            if (independentTasks.Count != 0)
            {
                output.Add(new List<string>(independentTasks));
            }
        }

        return output;
    }

    /// <summary>
    /// Finds tasks with no dependencies.
    /// </summary>
    /// <param name="indegrees">The dictionary representing indegrees for given tasks.</param>
    /// <returns>The list of independent tasks.</returns>
    private static List<string> FindIndependentTasks(Dictionary<string, int> indegrees)
    {
        if (indegrees.Count == 0)
        {
            return new List<string>();
        }

        List<string> independentTasks = indegrees.Where(task => task.Value.Equals(0)).Select(entry => entry.Key).ToList();
        if (independentTasks.Count == 0)
        {
            throw new ArgumentException("There is cyclic dependency for given tasks. Not possible to execute these tasks.");
        }
        return independentTasks;
    }
}
