using System;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!" + "Hello World!".WordCount());
            Console.WriteLine(new MyType{Meme = "leet a f "}.TypeExtension().Meme);
            var hashCode = "dasdsa".Length.GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode()
                .GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode()
                .GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode()
                .GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode()
                .GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode()
                .GetHashCode().GetHashCode().GetHashCode().GetHashCode().GetHashCode();
            Console.WriteLine(hashCode);

            Console.ReadKey();

        }
    }


    public class MyType
    {
        public string Meme { get; set; }

    }


    public static class MyExtensions
    {

        public static int WordCountProp { get; set; } = 123;

        public static int WordCount( this string str)
        {
            return str.Split(new[] { ' ', '.', '?' },
                StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static MyType TypeExtension(this MyType t)
        {
            t.Meme = t.Meme.Replace(" ", "");
            return t;
        }
    }
}
