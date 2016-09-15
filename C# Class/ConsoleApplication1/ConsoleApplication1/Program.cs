using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConsoleApplication1
{
    public class Program
    {
        static void Main(string[] args)
        {

            #region oldstuff
            ////using bit flags
            //var x = planets.mercury | planets.mars;

            //var y = (int)x;
            //var z = (planets)8;

            //Console.WriteLine(x);
            //Console.WriteLine(y);
            //Console.WriteLine(z);
            //var interpolation = $"Does X == Mars:{x == planets.mars}";
            //Console.WriteLine(interpolation);

            //var q = true & true;
            //var e = true && true;
            //var w = 12 & 12;
            //var t = 12 && 12;


            ////named vs positional
            //mymethod(1);
            //mymethod(10, 11);
            //mymethod(10, myvar_w: 2, myvar_y: 3);
            //mymethod(10, 11, 12);
            //mymethod(myvar_w: 12, 13, myvar_y: 14);

            ////generics
            //var l = new List<string>();
            //var filteredList = l.Where(string.IsNullOrEmpty).Where(f => string.IsNullOrEmpty(f));



            ////extensions

            //string s = "four";

            //string y = s.return5();
            //Console.WriteLine($"before: {s}, after: {y}");

            //var li = new List<int>() {1,2,3 };
            //foreach(var l in li)
            //{
            //    Console.WriteLine(l);
            //}
            //var editedLI = li.InstigateRevolt();
            //foreach (var l in editedLI)
            //{
            //    Console.WriteLine(l);
            //}

            #endregion

            ////2.2 Boxing unboxing implicit and explicit conversions
            //int i = 1;
            //double d = 2.2;

            //var x = (int)d;
            //var y = Convert.ToInt32(d);
            //Console.WriteLine(i);
            //Console.WriteLine(d);
            //Console.WriteLine(x);
            //Console.WriteLine(y);

            //dynamic z = "hello";
            //var w = Convert.ToDouble(z); // throws format exception
            //Console.WriteLine(w);

            //2.3 encapsulation
            //enum = public, cannot edit access modifiers on members

            //private int myVar;

            //public int MyProperty
            //{
            //    get { return myVar; }
            //    set { myVar = value; }
            //}
            //var x = new thing();
            //left l = new thing();
            //right r = new thing();

            //x.move();
            //l.move();
            //r.move();

            //var plist = new Pitchforks();
            //plist.forks = new List<string>() { "---C", "test" }; 
            //var ptype = plist.GetType();

            var x = new SillyExample() { x = 1,  y = 2, zed = "Why Hello!" };
            var y = new SillyExample();

            foreach(var prop in x.GetType().GetFields())
            {
                prop.SetValue(y, prop.GetValue(x));
            }
            Console.ReadLine();
        }

        ////C#6 shorthand for prop initialization
        //public int MyProp { get; set; } = 5;

        //[Flags]
        //public enum planets
        //{
        //    //base 2 for bit flags
        //    none = 0,
        //    mercury = 1,
        //    venus = 2,
        //    earth = 4,
        //    mars = 8
        //}

        ////Named vs positional
        //private static void mymethod(int x, int myvar_y = 2, int myvar_w = 3)
        //{

        //}


        //MOAR ON THURSDAY REFREXION!!
    }

    public static class MikesFancyExtensionLibrary
    {
        public static string return5(this string x)
        {
            return "five";
        }

        public static IEnumerable<string> InstigateRevolt<T>(this IEnumerable<T> x)
        {
            var l = new List<string>();
            foreach (var s in x)
            {
                l.Add("---E");
            }
            return l;
        }
    }

    interface left
    {
        void move();
    }
    public interface right
    {
        void move();
    }

    public class thing : left, right
    {
        void left.move()
        {
            Console.WriteLine("left");
        }
        void right.move()
        {
            Console.WriteLine("right");
        }
        public void move()
        {

            ((left)this).move();
            ((right)this).move();
        }
    }

    public abstract class PitchforkEmporium
    {
        public string American { get; } = "---E";
        public string Euro { get; } = "---€";
        public string DiscountAmerican_60 { get; } = "---F";

        public virtual List<string> Buy(string type, int qty)
        {
            var order = new List<string>();
            for (int i = 0; i <= qty; i++)
            {
                order.Add(type);
            }
            return order;
        }


    }

    public class PoorMansPitchforkEmporium : PitchforkEmporium
    {
        public override List<string> Buy(string type, int qty)
        {
            qty *= 2;
            return base.Buy(type, qty);
        }
    }

    abstract class PIInterface
    {
        protected abstract void ProcessReports(List<string> reports);
        protected virtual void AcceptReport(List<string> reports) { }

        private void buffer()
        {

        }
    }

    [Serializable]
    public class Pitchforks
    {
        [XmlElement(ElementName = "ForkList")]
        public IEnumerable<string> forks;


    }

    public class SillyExample
    {
        public int x;
        public int y;
        public string zed;
    }
}
