using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Delpic
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            
            string content = "";
            using (StreamReader fa = new StreamReader("kickus.txt"))
            {
                content = fa.ReadToEnd();
            }

            foreach (var item in Directory.GetFiles("UploadFile"))
            {
                Console.Write("*");
                if (!content.Contains(Path.GetFileName(item)))
                {
                    try
                    {
                        File.Delete(item);
                        Console.WriteLine(item);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(item + "(error)");
                    }
                }
            }
            Console.ReadKey();
        }
    }
}
