using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson;

namespace Blueshift.EntityFrameworkCore.MongoDB
{
    /// <summary>
    /// Provides methods for converting <see cref="ObjectId"/> instances to other types.
    /// </summary>
    public class ObjectIdTypeConverter : TypeConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="sourceType">A <see cref="Type"/> that represents the type you want to convert from.</param>
        /// <returns><code>true</code> if the value can be converted; otherwise <code>false</code>.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => (sourceType == typeof(string) || base.CanConvertFrom(context, sourceType));

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
        /// <param name="value">The object to convert.</param>
        /// <returns>A new <see cref="ObjectId"/> represented by <paramref name="value"/>.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!CanConvertFrom(context, Check.NotNull(value, nameof(value)).GetType()))
            {
                throw new InvalidOperationException($"Cannot convert {value} to {typeof(ObjectId)}.");
            }
            return ObjectId.Parse(value as string);
        }

        /// <summary>
        /// Returns whether this converter can convert the object to the specified type, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="destinationType">A <see cref="Type"/> that represents the type you want to convert to.</param>
        /// <returns><code>true</code> if the value can be converted; otherwise <code>false</code>.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => (destinationType == typeof(string)) || base.CanConvertTo(context, destinationType);

        /// <summary>
        /// Converts the given value object to the specified type, using the specified context and culture info.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
        /// <param name="value">The object to convert.</param>
        /// <param name="destinationType">The type to convert the value parameter to.</param>
        /// <returns>A new instance of the specified <paramref name="destinationType"/> that represents <paramref name="value"/>.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!CanConvertTo(context, destinationType))
            {
                throw new InvalidOperationException($"Cannot convert {value} to {destinationType}.");
            }
            return value.ToString();
        }
    }
}