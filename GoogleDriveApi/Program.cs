using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveApi
{
    public class Program
    {
         static void Main(string[] strings)
        {
            GoogleDriveStats test = new(Environment.CurrentDirectory+"\\"+ "client_secret_962208404895-0upedvipigcorrt3sc7tkd1e82e0484n.apps.googleusercontent.com.json");
            var tt = test.UpdateFile(@"C:\Users\psair\Desktop\xz.txt", test.GetFileOrFolderID("TestDocument"));
           Console.ReadLine();
        }
    }
}
