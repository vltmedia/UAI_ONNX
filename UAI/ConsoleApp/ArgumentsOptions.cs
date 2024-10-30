using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAI.ConsoleApp
{
    [System.Serializable]
    public class ArgumentsOptions
    {
        [Option('i', "input", Required = false, HelpText = "Input File. ")]
        public string inputPath { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output Directory.")]

        public string outputPath { get; set; }

        [Option('t', "temp", Required = false, HelpText = "Temp directory to save files to.")]
        public string tempPath { get; set; }

        [Option('c', "config", Required = false, HelpText = "Config File that overrides all of the other variables. Use this if sending from another application. should be json.")]
        public string configPath { get; set; }

        [Option('m', "model", Required = false, HelpText = "Model File.")]
        public string modelPath { get; set; }

        [Option('d', "device", Required = false, HelpText = "Device to run the model on. Default is CUDA.", Default ="cuda")]
        public string device { get; set; }

        [Option('s', "server", Required = false, HelpText = "Run as a server.", Default =false)]
        public bool isServer { get; set; }

        [Option('r', "saveMasks", Required = false, HelpText = "Save the masks to the output directory.", Default = false)]
        public bool saveMasks { get; set; }

        [Option('j', "saveJson", Required = false, HelpText = "Save a json file containing data to the output directory.", Default = false)]
        public bool saveJson { get; set; }

        [Option('p', "port", Required = false, HelpText = "Port to run the server on. Default is 677.", Default =677)]
        public int port { get; set; }

        [ Option('n', "name", Required = false, HelpText = "Name of the server.")]
        public string name { get ; set; }

     
    }
}
