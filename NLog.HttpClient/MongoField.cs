using System;
using System.ComponentModel;
using NLog.Config;
using NLog.Layouts;

namespace NLog.HttpClient
{
    /// <summary>
    /// A configuration item for MongoDB target.
    /// </summary>
    [NLogConfigurationItem]
    [ThreadAgnostic]
    public sealed class JsonField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonField"/> class.
        /// </summary>
        public JsonField()
            : this(null, null, "String")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonField"/> class.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="layout">The layout used to generate the value for the field.</param>
        public JsonField(string name, Layout layout)
            : this(name, layout, "String")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonField" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="layout">The layout.</param>
        /// <param name="dataType">The bson type.</param>
        public JsonField(string name, Layout layout, string dataType)
        {
            Name = name;
            Layout = layout;
            DataType = dataType ?? "String";
        }

        /// <summary>
        /// Gets or sets the name of the MongoDB field.
        /// </summary>
        /// <value>
        /// The name of the MongoDB field.
        /// </value>
        [RequiredParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the layout used to generate the value for the field.
        /// </summary>
        /// <value>
        /// The layout used to generate the value for the field.
        /// </value>
        [RequiredParameter]
        public Layout Layout { get; set; }

        /// <summary>
        /// Gets or sets the bson type of the field. Possible values are Boolean, DateTime, Double, Int32, Int64 and String
        /// </summary>
        /// <value>
        /// The bson type of the field..
        /// </value>
        [DefaultValue("String")]
        public string DataType
        {
            get => _dataType;
            set
            {
                _dataType = value;
                HttpContentTypeCode = ConvertToTypeCode(value?.Trim() ?? string.Empty);
            }
        }
        private string _dataType;

        internal TypeCode HttpContentTypeCode { get; private set; } = TypeCode.String;

        private TypeCode ConvertToTypeCode(string bsonType)
        {
            if (string.IsNullOrEmpty(bsonType) || string.Equals(bsonType, "String", StringComparison.OrdinalIgnoreCase))
                return TypeCode.String;

            if (string.Equals(bsonType, "Boolean", StringComparison.OrdinalIgnoreCase))
                return TypeCode.Boolean;

            if (string.Equals(bsonType, "DateTime", StringComparison.OrdinalIgnoreCase))
                return TypeCode.DateTime;

            if (string.Equals(bsonType, "Double", StringComparison.OrdinalIgnoreCase))
                return TypeCode.Double;

            if (string.Equals(bsonType, "Int32", StringComparison.OrdinalIgnoreCase))
                return TypeCode.Int32;

            if (string.Equals(bsonType, "Int64", StringComparison.OrdinalIgnoreCase))
                return TypeCode.Int64;

            if (string.Equals(bsonType, "Object", StringComparison.OrdinalIgnoreCase))
                return TypeCode.Object;

            return TypeCode.String;
        }
    }
}