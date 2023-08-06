using Homura.ORM;
using Homura.ORM.Mapping;
using Reactive.Bindings;
using System;
using System.Linq;
using System.Reflection;

namespace Homura.Test.UnitTest.Enum
{
    public class SettingEntry : EntityBaseObject
    {

        [Column("Id", "NUMERIC", 0), PrimaryKey, Index]
        public ReactivePropertySlim<Guid> Id { get; } = new();

        [Column("Title", "TEXT", 1)]
        public ReactivePropertySlim<string> Title { get; } = new();

        [Column("Type", "NUMERIC", 2)]
        public ReactivePropertySlim<Type> Type { get; } = new();

        [Column("Raw", "NUMERIC", 3)]
        public ReactivePropertySlim<object> Raw { get; } = new();


        public SettingEntry SugarCoating<T>()
        {
            if (typeof(T) != Type.Value)
            {
                throw new TypeMismatchException($"desired: {typeof(T).FullName}, but actual: {Type.Value.FullName}");
            }

            if (typeof(T).IsEnum)
            {
                Type dynamicType = Type.Value;
                Type genericClass = typeof(EnumSettingEntry<>);
                Type constructedClass = genericClass.MakeGenericType(dynamicType);
                EnumSettingEntry<T> instance = (EnumSettingEntry<T>)Activator.CreateInstance(constructedClass);
                instance.Id.Value = Id.Value;
                instance.Title.Value = Title.Value;
                instance.Raw.Value = Raw.Value;
                instance.Type.Value = Type.Value;
                return instance;
            }
            else
            {
                return new SettingEntry()
                {
                    Id =
                    {
                        Value = Id.Value
                    },
                    Title =
                    {
                        Value = Title.Value
                    },
                    Type =
                    {
                        Value = Type.Value
                    },
                    Raw =
                    {
                        Value = Raw.Value
                    }
                };
            }
        }

        public EnumSettingEntry SugarCoatingEnum()
        {
            Type methodClass = this.GetType();
            MethodInfo sugarCoatingMethod = methodClass.GetMethods().Single(m => m.Name == "SugarCoating" && m.GetGenericArguments().Length == 1).MakeGenericMethod(Type.Value);
            return (EnumSettingEntry)sugarCoatingMethod.Invoke(this, Array.Empty<object>());
        }
    }

    public class EnumSettingEntry : SettingEntry
    {

    }

    public class EnumSettingEntry<T> : EnumSettingEntry
    {
        public object Object
        {
            get => Raw.Value;
            set => Raw.Value = value;
        }

        public T Value
        {
            get => Object is not null ? (T)System.Enum.ToObject(typeof(T), Convert.ToInt32(Object)) : default;
            set => Object = (int)System.Enum.ToObject(typeof(T), value);
        }

        public ReactiveCollection<T> Options { get; } = new();

        public EnumSettingEntry() : base()
        {
            Id.Value = Guid.NewGuid();
            Type.Value = typeof(T);
            if (typeof(T).IsEnum)
            {
                Options.Clear();
                System.Enum.GetValues(typeof(T)).OfType<T>().AddTo(Options);
            }
        }

        public EnumSettingEntry(string guidStr, string title, T @default) : this()
        {
            Id.Value = Guid.Parse(guidStr);
            Title.Value = title;
            Value = @default;
            Type.Value = typeof(T);
            if (typeof(T).IsEnum)
            {
                Options.Clear();
                System.Enum.GetValues(typeof(T)).OfType<T>().AddTo(Options);
            }
        }
    }
}
