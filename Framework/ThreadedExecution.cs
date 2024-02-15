using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Framework;

namespace Framework
{
    public class ThreadedExecution<TInput, TOutput>
        where TInput: class
    {
        /// <summary>The default value for ProgressInfoSteps property.</summary>
        public const int PROGRESS_INFO_STEPS = 20;

        /// <summary>Delegate for the task execution method.</summary>
        /// <param name="input">input data packet</param>
        /// <param name="parameters">global computation parameters, <b>not thread-safe, USE FOR READING ONLY!!!</b></param>
        /// <returns>task result</returns>
        public delegate TOutput ExecBody(TInput input, int threadId, params object[] parameters);

        
        private TInput[] _in;
        private TOutput[] _out;
        private ExecBody _body;
        private int _numDone, _numAssigned;
        private Thread[] _threads;
        private bool _aborted;
        private int _piSteps = PROGRESS_INFO_STEPS;
        private int _piStepSize;
        private object[] _params;


        public int ProgressInfoSteps
        {
            get { return _piSteps; }
            set { _piSteps = value; }
        }


        /// <summary>
        /// Constructs a new instance of ThreadedExecution class with the specified number of threads.
        /// </summary>
        /// <param name="input">array of input data packets</param>
        /// <param name="execBody">task execution method</param>
        /// <param name="threadCount">number of threads to run</param>
        /// <param name="parameters">global computation parameters</param>
        public ThreadedExecution(TInput[] input, ExecBody execBody, int threadCount, params object[] parameters)
        {
            if (threadCount < 0)
                threadCount = Environment.ProcessorCount;
            _in = input;
            _out = new TOutput[input.Length];
            _body = execBody;

            _params = parameters;

            _numDone = 0;
            _numAssigned = 0;
            _aborted = false;

            _threads = new Thread[threadCount];
            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i] = new Thread(ThreadBody);
            }
        }

        /// <summary>
        /// Constructs a new instance of ThreadedExecution class with the specified number of threads.
        /// </summary>
        /// <param name="input">array of input data packets</param>
        /// <param name="execBody">task execution method</param>
        /// <param name="threadCount">number of threads to run</param>
        public ThreadedExecution(TInput[] input, ExecBody execBody, int threadCount)
            : this(input, execBody, threadCount, null)
        { }

        /// <summary>
        /// Constructs a new instance of ThreadedExecution class with the number of threads matching the number of available processors/cores.
        /// </summary>
        /// <param name="input">array of input data packets</param>
        /// <param name="execBody">task execution method</param>
        public ThreadedExecution(TInput[] input, ExecBody execBody)
            : this(input, execBody, Environment.ProcessorCount, null)
        { }

        private TInput NextTask(TOutput prevResult, ref int taskId)
        {
            lock (_threads)
            {
                if (_aborted) return default(TInput);

                if (taskId >= 0)
                {
                    _out[taskId] = prevResult;
                    _numDone++;
                    //Console.WriteLine("{0}/{1}", _numDone, _in.Length);
                    //Console.Write(".");
                }

                if (_numAssigned < _in.Length)
                {
                    taskId = _numAssigned++;
                    
                    return _in[taskId];
                }

                return default(TInput);
            }
        }

        private void ThreadBody(object parameter)
        {
            int threadId = (int)parameter;

            int taskId = -1;
            TInput task;
            TOutput result = default(TOutput);

            while (true)
            {
                task = NextTask(result, ref taskId);
                if (task == null) break;
                result = _body(task, threadId, _params);
            }
        }

        /// <summary>
        /// Starts the execution of tasks. Blocks until all the input packets have been processed.
        /// </summary>
        /// <returns>array of results in the same order as the input data</returns>
        public TOutput[] Execute()
        {
            if (2 * _piSteps < _in.Length) _piStepSize = _in.Length / _piSteps;
            else _piStepSize = 1;

            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i].Start((object)i);
            }

            for (int i = 0; i < _threads.Length; i++)
            {
                try
                {
                    _threads[i].Join();
                }
                catch (Exception)
                {
                    Console.WriteLine("exeption");
                }
            }

            if (_aborted) return null;
            //Console.WriteLine();
            return _out;
        }

        /// <summary>
        /// Aborts the execution before it has finished.
        /// </summary>
        public void Abort()
        {
            lock (_threads)
            {
                _aborted = true;
            }

            for (int i = 0; i < _threads.Length; i++)
            {
                try
                {
                    _threads[i].Join();
                }
                catch (Exception)
                { }
            }
        }
    }
}
