using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace Parallelism
{
    class Program
    {
        static void Main(string[] args)
        { 
        }

        /// <summary>
        /// "For" method that makes iterations so that every itertion is parallel to each other.
        /// </summary>
        /// <param name="fromInclusive">Start index of iteration(inclusive).</param>
        /// <param name="toExclusive">End index of iteration(exclusive).</param>
        /// <param name="body">Action<int> delegate which is invokes on every iteration.</param>
        static void ParallelFor(int fromInclusive, int toExclusive, Action<int> body)
        {
            if(body == null)
            {
                throw new ArgumentNullException("Action");
            }

            Task[] tasks = new Task[toExclusive - fromInclusive];
            for(int i = fromInclusive; i < toExclusive; i++)
            {
                int j = i;
                tasks[j] = Task.Factory.StartNew(() => body(j));
            }

            Task.WaitAll(tasks);
        }

        /// <summary>
        /// "Foreach" method that makes iterations over the source so that every iteration is parallel to each other.
        /// </summary>
        /// <typeparam name="TSource">Type of source enumerable.</typeparam>
        /// <param name="source">The source enumerable.</param>
        /// <param name="body">Action delegate which is inveking on every itartion.</param>
        static void ParallelForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body)
        {
            if(source == null)
            {
                throw new ArgumentNullException("Source");
            }

            if (body == null)
            {
                throw new ArgumentNullException("Action");
            }

            List<Task> tasks = new List<Task>();

            foreach(var item in source)
            {
                tasks.Add(Task.Factory.StartNew(() => body(item)));
            }

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// "Foreach" method that makes iterations over the source so that every iteration is parallel to each other, it also has an option to configure iterations.
        /// </summary>
        /// <typeparam name="TSource">Type of the source enumerable.</typeparam>
        /// <param name="source">The source enumerable.</param>
        /// <param name="parallelOptions">Options for configurations.</param>
        /// <param name="body">Action delegate which is inveking on every itartion.</param>
        static void ParallelForEachWithOptions<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource> body)
        {
            if(source == null)
            {
                throw new ArgumentNullException("Source");
            }

            if(parallelOptions == null)
            {
                throw new ArgumentNullException("Options");
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException("Action");
            }

            int maxDegreeOfParallelism = parallelOptions.MaxDegreeOfParallelism;

            if (parallelOptions.MaxDegreeOfParallelism == -1)
            {
                List<Task> tasks = new List<Task>();

                foreach (var item in source)
                {
                    tasks.Add(Task.Factory.StartNew(() => body(item)));
                }

                Task.WaitAll(tasks.ToArray());
            }
            else
            {
                List<Task> tasks = new List<Task>();
                foreach(var item in source)
                {
                    parallelOptions.CancellationToken.ThrowIfCancellationRequested();

                    if (tasks.Count < maxDegreeOfParallelism)
                    {
                        tasks.Add(Task.Factory.StartNew(() => body(item)));
                    }
                    else
                    {
                        tasks.Remove(tasks[Task.WaitAny(tasks.ToArray())]);

                        tasks.Add(Task.Factory.StartNew(() => body(item)));
                    }
                }

                Task.WaitAll(tasks.ToArray());
            }
        }
    }
}
