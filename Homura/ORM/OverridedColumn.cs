using System;
using System.Collections.Generic;
using System.Reflection;

namespace Homura.ORM
{
    public class OverridedColumn : BaseColumn
    {
        private string _ColumnName;
        private string _DBDataType;
        private IEnumerable<IDdlConstraint> _Constraints;
        private int? _Order;
        private PropertyInfo _PropInfo;
        private Type _EntityDataType;
        private object _DefaultValue;
        private HandlingDefaultValue _PassType;

        public OverridedColumn(Column baseColumn, string newColumnName = null, string newDataType = null, IEnumerable<IDdlConstraint> newConstraints = null, int? newOrder = null, PropertyInfo newPropInfo = null)
        {
            BaseColumn = baseColumn;
            _ColumnName = newColumnName;
            _DBDataType = newDataType;
            _Constraints = newConstraints;
            _Order = newOrder;
            _PropInfo = newPropInfo;
        }

        public Column BaseColumn { get; private set; }

        public override string ColumnName
        {
            get
            {
                if (_ColumnName != null) return _ColumnName;
                else return BaseColumn.ColumnName;
            }
            protected set { throw new NotSupportedException(); }
        }

        public override string DBDataType
        {
            get
            {
                if (_DBDataType != null) return _DBDataType;
                else return BaseColumn.DBDataType;
            }
            protected set { throw new NotSupportedException(); }
        }

        public override IEnumerable<IDdlConstraint> Constraints
        {
            get
            {
                if (_Constraints != null) return _Constraints;
                else return BaseColumn.Constraints;
            }
            protected set { throw new NotSupportedException(); }
        }

        public override int Order
        {
            get
            {
                if (_Order.HasValue) return _Order.Value;
                else return BaseColumn.Order;
            }
            protected set { throw new NotSupportedException(); }
        }

        public override PropertyInfo PropInfo
        {
            get
            {
                if (_PropInfo != null) return _PropInfo;
                else return BaseColumn.PropInfo;
            }
            protected set { throw new NotSupportedException(); }
        }

        public override Type EntityDataType
        {
            get
            {
                if (_EntityDataType != null) return _EntityDataType;
                else return BaseColumn.EntityDataType;
            }
            protected set { throw new NotSupportedException(); }
        }

        public override object DefaultValue
        {
            get
            {
                if (_DefaultValue != null) return _DefaultValue;
                else return BaseColumn.DefaultValue;
            }
            protected set { throw new NotSupportedException(); }
        }

        public override HandlingDefaultValue PassType
        {
            get
            {
                if (_PassType != null) return _PassType;
                else return BaseColumn.PassType;
            }
            protected set { throw new NotSupportedException(); }
        }
    }
}