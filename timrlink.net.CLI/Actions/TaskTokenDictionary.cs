using System;
using System.Collections.Generic;
using System.Linq;
using timrlink.net.Core.API;

namespace timrlink.net.CLI.Actions
{
    internal static class TaskTokenDictionary
    {
        public static IDictionary<string, Task> ToTokenDictionary(this IList<Task> tasks)
        {
            var taskUuidDictionary = tasks.ToDictionary(task => task.uuid);
            return tasks.ToDictionary(task => Tokenize(task, taskUuidDictionary));
        }

        private static string Tokenize(Task task, Dictionary<string, Task> taskUuidDictionary)
        {
            var pathTokens = new List<string>();

            while (task != null)
            {
                pathTokens.Add(task.name);
                task = String.IsNullOrEmpty(task.parentUuid) ? null : taskUuidDictionary[task.parentUuid];
            }

            pathTokens.Reverse();
            return String.Join('|', pathTokens);
        }
    }
}
