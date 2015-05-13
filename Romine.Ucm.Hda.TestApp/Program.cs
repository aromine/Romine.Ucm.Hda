using Romine.Ucm.Hda;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Romine.Ucm.Hda.Data;

namespace Romine.Ucm.Hda.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Stream stream = File.OpenRead("search.hda");

            HdaReader reader = new HdaReader();
            Stopwatch sw = Stopwatch.StartNew();
            HdaDataBinder binder = reader.Read(stream);
            sw.Stop();
            Console.WriteLine("Took " + sw.ElapsedMilliseconds + " to read the HDA file.");

            sw.Reset();
            sw.Start();
            writeBinderToFile(binder, "new.hda");
            sw.Stop();
            Console.WriteLine("Took " + sw.ElapsedMilliseconds + " to write the HDA file.");

            binder = reader.Read(binder.ToString());

            writeBinderToFile(binder, "new2.hda");
            
            Console.WriteLine("Are the written files identical? " + (FilesAreEqual(new FileInfo("new.hda"), new FileInfo("new2.hda")) ? "Yes" : "No"));

            Console.ReadKey(true);

        }

        const int BYTES_TO_READ = sizeof(Int64);

        static bool FilesAreEqual(FileInfo first, FileInfo second)
        {
            if (first.Length != second.Length)
                return false;

            int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BYTES_TO_READ);
                    fs2.Read(two, 0, BYTES_TO_READ);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        return false;
                }
            }

            return true;
        }

        static void writeBinderToFile(HdaDataBinder imp, string filename)
        {
            Stream stream = File.OpenWrite(filename);
            new HdaWriter(imp).WriteToStream(stream, true);
        }

        

        static void TestDataType()
        {
            int v = int.Parse("6");
            ResultSetColumn.DataType t = (ResultSetColumn.DataType)Enum.GetValues(typeof(ResultSetColumn.DataType)).GetValue(1);
            Array stuff = Enum.GetValues(typeof(ResultSetColumn.DataType));
            Console.Write(t.ToString());
            ResultSetColumn.DataType tg = (ResultSetColumn.DataType)Enum.ToObject(typeof(ResultSetColumn.DataType), 2);
            Console.ReadKey();
        }
    }
}
