

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Homura.ORM
{
    public class OverridedColumn : BaseColumn
    {
        private string _ColumnName;
        private string _DataType;
        private IEnumerable<IDdlConstraint> _Constraints;
        private int? _Order;
        private PropertyInfo _PropInfo;

        public OverridedColumn(Column baseColumn, string newColumnName = null, string newDataType = null, IEnumerable<IDdlConstraint> newConstraints = null, int? newOrder = null, PropertyInfo newPropInfo = null)
        {
            BaseColumn = baseColumn;
            _ColumnName = newColumnName;
            _DataType = newDataType;
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

        public override string DataType
        {
            get
            {
                if (_DataType != null) return _DataType;
                else return BaseColumn.DataType;
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
    }
}
