using System;
using System.IO;
using System.Text;

//https://stackoverflow.com/questions/10341318/whats-the-recommended-way-to-prefix-console-write
namespace TeamSpeak3ModularBot
{
    public class PrefixedWriter : TextWriter
    {
        private readonly TextWriter _originalOut;

        public PrefixedWriter()
        {
            _originalOut = Console.Out;
        }

        public override Encoding Encoding => new UTF8Encoding();

        public override void WriteLine(string message)
        {
            _originalOut.WriteLine($"[{DateTime.Now:T}]: {message}");
        }
        public void WriteWithTime(string message)
        {
            _originalOut.Write($"[{DateTime.Now:T}]: {message}");
        }
    }
}
