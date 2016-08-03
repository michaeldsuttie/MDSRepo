using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_Class
{
    #region Extensions
    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        //extensions
    //        string mystring = "Hello!";
    //        var len = mystring.getLength();
    //        Console.WriteLine($"StringLength: {len}");
    //    }
    //}
    ////extensions
    //static class myExtension
    //{
    //    public static int getLength(this string _string)
    //    {
    //        return _string.Length;
    //    }
    //}
    #endregion

    #region Interfaces
    //class Program
    //{

    //    static void Main(string[] args)
    //    {
    //        var employee = new employee();
    //        Console.WriteLine($"Alan is confused: {((iAlan)employee).Confusion}");
    //        Console.WriteLine($"Anu is confused: {employee.Confusion}");
    //        Console.ReadKey();
    //    }
    //}

    //public class employee : iJoe, iAnu, iAlan
    //{
    //    public bool Confusion
    //    {
    //        get
    //        {
    //            return true;
    //        }
    //    }
    //    bool iAlan.Confusion
    //    {
    //        get { return false; }
    //    }
    //}

    //public interface iJoe
    //{
    //    bool Confusion { get; }
    //}
    //public interface iAnu
    //{
    //    bool Confusion { get; }
    //}
    //public interface iAlan
    //{
    //    bool Confusion { get; }
    //}
    #endregion

    #region Abstract Classes and Protected Notes
    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        var employee = new DSTEmployee();
    //        Console.WriteLine($"Alan is confused: {((iAlan)employee).Confusion}");
    //        Console.WriteLine($"Anu is confused: {employee.Confusion}");
    //        Console.WriteLine($"Employee slacking: {employee.slacking()}");
    //        Console.WriteLine($"EmployeeBase slacking: {((employeeBase)employee).slacking()}");
    //        Console.WriteLine($"Employee working: {((employeeBase)employee).working()}");
    //        var test = employee.referencedCanYouSeeMee;

    //        Console.ReadKey();
    //    }
    //}

    //public class DSTEmployee : employeeBase
    //{
    //    public bool slacking()
    //    {
    //        return true;
    //    }

    //    public bool referencedCanYouSeeMee { get { return CanYouSeeMee; } }

    //}

    //public abstract class employeeBase : iJoe, iAnu, iAlan
    //{
    //    protected bool CanYouSeeMee;
    //    public virtual bool slacking()
    //    {
    //        return false;
    //    }

    //    public bool working()
    //    {
    //        return true;
    //    }

    //    public bool Confusion
    //    {
    //        get
    //        {
    //            return true;
    //        }
    //    }
    //    bool iAlan.Confusion
    //    {
    //        get { return false; }
    //    }
    //}

    //public interface iJoe
    //{
    //    bool Confusion { get; }
    //}
    //public interface iAnu
    //{
    //    bool Confusion { get; }
    //}
    //public interface iAlan
    //{
    //    bool Confusion { get; }
    //}
    #endregion

    #region LISKOV
    class Program
    {
        //If a behavior is changed in an derived class, it's implementation should still work for references to the base class.
        static void Main(string[] args)
        {
            NetworkPing np = new NetworkPing();
            Console.WriteLine($"NetworkPing: {np.PingServer()}"); //true
            np = new PingPing();
            Console.WriteLine($"PingPing w/o Connect(): {np.PingServer()}"); //false
            PingPing pp = new PingPing();
            pp.Connect();
            Console.WriteLine($"PingPing w Connect(): {pp.PingServer()}"); //true

            Console.ReadKey();
        }
    }

    class NetworkPing
    {
        protected bool Connected = true;
        internal bool PingServer()
        {
            return Connected;
        }
    }

    class PingPing : NetworkPing
    {
        internal void Connect()
        {
            Connected = false;
        }
    }

    #endregion
}
