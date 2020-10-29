
namespace Homura.Core
{
    /*
     * Interface defining a constructor signature? https://stackoverflow.com/questions/619856/interface-defining-a-constructor-signature
     * quizzer Boris Callens https://stackoverflow.com/users/11333/boris-callens
     * answerer Dan https://stackoverflow.com/users/3988984/dan
     */

    public abstract class MustInitialize<T1>
    {
        public MustInitialize(T1 p1)
        { }
    }

    public abstract class MustInitialize<T1, T2>
    {
        public MustInitialize(T1 p1, T2 p2)
        { }
    }

    public abstract class MustInitialize<T1, T2, T3>
    {
        public MustInitialize(T1 p1, T2 p2, T3 p3)
        { }
    }

    public abstract class MustInitialize<T1, T2, T3, T4>
    {
        public MustInitialize(T1 p1, T2 p2, T3 p3, T4 p4)
        { }
    }

    public abstract class MustInitialize<T1, T2, T3, T4, T5>
    {
        public MustInitialize(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        { }
    }
}
