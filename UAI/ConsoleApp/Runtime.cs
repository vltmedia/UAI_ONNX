using CommandLine;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Threading;
using static FaceParser.UAI.Server.RuntimeServer;

namespace UAI.ConsoleApp
{
    // Non-generic base class to hold the static instance
    public abstract class RuntimeBase
    {
        public static RuntimeBase Instance { get;  set; }
        public abstract Action<float> OnUpdate { get; set; }
    }

    public class Runtime<T> : RuntimeBase where T : ArgumentsOptions
    {
        private static Runtime<T> _instance;
        public static Runtime<T> Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        public static T args;
        public string[] inputArgs;

        public float _fps = 60;
        public virtual float fps
        {
            get
            {
                return _fps;
            }
            set
            {
                _fps = value;
            }
        }
        public virtual string name
        {
            get { return args.name; }
            set { args.name = value; }
        }
        public virtual int port
        {
            get { return args.port; }
            set { args.port = value; }
        }
        public virtual bool isServer
        {
            get { 
                if(args == null)
                {
                    return false;
                }
                return args.isServer; }
            set { args.isServer = value; }
        }

        public bool _hasMainLoop = true;

        public virtual bool hasMainLoop
        {
            get
            {
                return _hasMainLoop;
            }
            set
            {
                _hasMainLoop = value;
            }
        }


        public List<string> scenes = new List<string>();
        public override Action<float> OnUpdate { get; set; }
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public Runtime(string[] arguments)
        {
            if (Instance == null)
            {
                Instance = this;
                RuntimeBase.Instance = this;
                CommandLine.Parser.Default.ParseArguments<T>(arguments)
.WithParsed(AppLoaded)
.WithNotParsed(HandleParseError);
            }
            else
            {
            Console.WriteLine("Instance already exists");

            }
        }


        public virtual void Start()
        {

        }
        public virtual async void AppLoaded(T opts)
        {
            args = opts;

            RunStartApp();
        }
        public virtual async void RunStartApp()
        {
            await StartApp();
        }
        public virtual async Task StartApp()
        {
            Start();

            if (isServer)
            {
                StartServer();
            }

            if (hasMainLoop)
            {
                RunMainLoop();
            }


        }

        public void StartServer()
        {
            Console.WriteLine($"Is server: {isServer}");
            //var host = Host.CreateDefaultBuilder(inputArgs)
            //    .ConfigureWebHostDefaults(webBuilder =>
            //    {
            //        webBuilder.UseStartup<StartupServer>();
            //        webBuilder.UseUrls($"http://0.0.0.0:{port}");
            //        Console.WriteLine($"Starting server at: http://0.0.0.0:{port}");
            //    })
            //    .Build();
        }

        public virtual void HandleParseError(IEnumerable<Error> errs)
        {
           Console.WriteLine("Error parsing arguments");
            foreach (Error error in errs) {
                Console.WriteLine(error.ToString());
            }
            Console.WriteLine(
                "Usage: FaceParser -i <input> -o <output> -c <config> -m <model> -d <device>");
            Console.WriteLine(
                "Example: FaceParser -i input.jpg -o output.jpg -c config.json -m model.onnx -d CUDA");

        }
        public  virtual void SetArgs(T _args)
        {
            args = _args;
        }

        // Main loop method
        public async void RunMainLoop()
        {
            var delay = (int)(1000 / fps); // Delay in milliseconds between each update
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Console.WriteLine("Starting main loop. Press Escape to exit.");
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                // Calculate delta time
                float deltaTime = stopwatch.ElapsedMilliseconds / 1000f;
                stopwatch.Restart();

                // Invoke the OnUpdate action with deltaTime
                OnUpdate?.Invoke(deltaTime);

                // Check for Escape key to exit
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("Escape key pressed. Exiting main loop.");
                    ExitApp();
                    break;
                }

                // Wait asynchronously for the next frame
                await Task.Delay(delay);
            }
        }

        public virtual void ExitApp()
        {
            _cancellationTokenSource.Cancel();
            Exit();
        }

        public virtual void Exit() {
            Environment.Exit(0);
        }
    }
}
