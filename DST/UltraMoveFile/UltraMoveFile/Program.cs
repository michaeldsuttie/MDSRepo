using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UltraMoveFile
{
    class Program
    {
        static void Main(string[] args)
        {
            var files = Directory.GetFiles(args[0], args[1], SearchOption.TopDirectoryOnly);

            if (args[4].ToString() == "r")
            {
                files = Directory.GetFiles(args[0], args[1], SearchOption.AllDirectories);
            }

            if (files != null)
            {
                var waitMS = Int32.Parse(args[3]) * 60000;
                Thread.Sleep(waitMS);
            }

            foreach (var f in files)
            {
                try
                {
                    var dest = Path.GetDirectoryName(f);
                    var fileName = f.Substring(dest.Count());

                    File.Move(f, args[2] + fileName);
                    Console.WriteLine("Moved: " + f + " -> " + args[2] + fileName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
