
namespace Homura.ORM
{
    public class NotNull : IDdlConstraint
    {
        public string ToSql()
        {
            return "NOT NULL";
        }
    }
}
