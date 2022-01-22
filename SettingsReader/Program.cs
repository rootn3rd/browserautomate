using System;
using System.IO;
using System.Text;
using System.Linq;

namespace SettingsReader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var path = @"D:\delete\cc\data_1";

            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new BinaryReader(fileStream, Encoding.UTF8, false);
            var ending = "\x00";

            var endb = Encoding.UTF8.GetBytes(ending);
            var str = new StringBuilder();
            var bytes = reader.ReadBytes(4);
            while (bytes.Length != 0)
            {
                if(bytes.SequenceEqual(endb))
                {
                    Console.WriteLine(str.ToString());
                }

                bytes = reader.ReadBytes(4);
                str.Append(Encoding.UTF8.GetString(bytes));
            }
            var fullFile = reader.ReadString();

            Console.WriteLine(reader.ReadString());


            Console.ReadLine();

        }
    }
}
