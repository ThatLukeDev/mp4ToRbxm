using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Data;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using System.Threading;

namespace rbxv
{
    internal class Program
    {
        static void Main(string[] args)
        {
            void convertVideo(string inputName, string bufferName, int scaleX, int scaleY, int fps)
            {
                ProcessStartInfo ffmpegprocess = new ProcessStartInfo();
                ffmpegprocess.CreateNoWindow = false;
                ffmpegprocess.UseShellExecute = false;
                ffmpegprocess.FileName = "dependancies/ffmpeg.exe";
                ffmpegprocess.Arguments = $"-i {inputName} -vf scale={scaleX.ToString()}:{scaleY.ToString()},fps={fps.ToString()} {bufferName}";
                Process active = Process.Start(ffmpegprocess);
                active.WaitForExit();
            }

            string processFrame(string fileName)
            {
                string fBuffer = "";
                Bitmap fast = new Bitmap(fileName);
                fBuffer += "{";
                for (int i = 0; i < fast.Width; i++)
                {
                    fBuffer += "{";
                    for (int j = 0; j < fast.Height; j++)
                    {
                        fBuffer += "\"" + fast.GetPixel(i, j).R.ToString("X2") + fast.GetPixel(i, j).G.ToString("X2") + fast.GetPixel(i, j).B.ToString("X2") + "\",";
                    }
                    fBuffer += "},\n";
                }
                fBuffer += "},";
                fBuffer = fBuffer.Replace(",}", "}");
                Console.WriteLine(fileName + " has completed");
                return fBuffer;
            }

            string arginput = "";
            int argscaleX = 16;
            int argscaleY = 9;
            int argfps = 15;
            int argpps = 10;
            if (args.Length == 0) {
                Console.WriteLine("please specify an input argument");
            }
            for (int i = 0; i < args.Length; i += 2)
            {
                switch (args[i]) {
                    case "-input": arginput = args[i+1]; break;
                    case "-pps": argpps = Convert.ToInt32(args[i+1]); break;
                    case "-scaleX": argscaleX = Convert.ToInt32(args[i+1]); break;
                    case "-scaleY": argscaleY = Convert.ToInt32(args[i+1]); break;
                    case "-fps": argfps = Convert.ToInt32(args[i+1]); break;
                }
            }

            foreach (string v in Directory.EnumerateFiles("temp"))
            {
                File.Delete(v);
            }

            convertVideo(arginput, "temp/%05d.bmp", argscaleX*argpps, argscaleY*argpps, argfps);

            Console.WriteLine("Processing images at "+argfps+"fps "+argpps+"pps");

            Parallel.ForEach(Directory.EnumerateFiles("temp"), v =>
            {
                if (v.Substring(10, 4) == ".bmp")
                {
                    File.WriteAllText(v.Substring(0, 10) + ".rbxv", processFrame(v), Encoding.UTF8);
                }
            });

            string buffer = "return {";
            File.WriteAllText("output.rbxv", buffer, Encoding.UTF8);
            buffer = "";
            Console.WriteLine("Stitching output files");
            foreach (string v in Directory.EnumerateFiles("temp"))
            {
                if (v.Substring(10) == ".rbxv")
                {
                    File.AppendAllText("output.rbxv", File.ReadAllText(v), Encoding.UTF8);
                }
            }
            File.AppendAllText("output.rbxv", "}", Encoding.UTF8);
            Console.WriteLine("Done :) (you can delete the stuff in temp now)");
        }
    }
}
