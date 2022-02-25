﻿using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace NQuery.Data
{
    /// <summary>
    /// Provides a way of reading a forward-only stream of rows from a <see cref="Query"/>. This class cannot be inherited.
    /// </summary>
    public sealed class QueryDataReader : IDataReader
    {
        private QueryReader _queryReader;

        internal QueryDataReader(QueryReader queryReader)
        {
            _queryReader = queryReader;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_queryReader == null)
                return;

            _queryReader.Dispose();
            _queryReader = null;
        }

        /// <summary>
        /// Closes the <see cref="T:System.Data.IDataReader" /> Object.
        /// </summary>
        void IDataReader.Close()
        {
            Dispose();
        }

        private void EnsureNotDisposed()
        {
            if (_queryReader == null)
                throw new ObjectDisposedException(nameof(QueryDataReader));
        }

        /// <summary>
        ///  Advances the data reader to the next result, when reading the results of batch SQL statements.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> if there are more rows; otherwise, <see langword="false" />.
        /// </returns>
        bool IDataReader.NextResult()
        {
            EnsureNotDisposed();
            return false;
        }

        /// <summary>
        /// Advances the <see cref="T:System.Data.IDataReader" /> to the next record.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> if there are more rows; otherwise, <see langword="false" />.
        /// </returns>
        public bool Read()
        {
            EnsureNotDisposed();
            return _queryReader.Read();
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.DataTable" /> that describes the column metadata
        /// of the <see cref="T:System.Data.IDataReader" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.DataTable" /> that describes the column metadata.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.IDataReader" /> is closed.</exception>
        public DataTable GetSchemaTable()
        {
            EnsureNotDisposed();
            return _queryReader.CreateSchemaTable();
        }

        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row.
        /// </summary>
        int IDataReader.Depth
        {
            get
            {
                EnsureNotDisposed();
                return 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        bool IDataReader.IsClosed
        {
            get { return _queryReader == null; }
        }

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        int IDataReader.RecordsAffected
        {
            get
            {
                EnsureNotDisposed();
                return 0;
            }
        }

        /// <summary>
        /// Gets the name for the field to find.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The name of the field or the empty string (""), if there is no value to return.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public string GetName(int i)
        {
            EnsureNotDisposed();
            return _queryReader.GetColumnName(i);
        }

        /// <summary>
        /// Gets the data type information for the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The data type information for the specified field.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public string GetDataTypeName(int i)
        {
            EnsureNotDisposed();
            return _queryReader.GetColumnType(i).FullName;
        }

        /// <summary>
        /// Gets the <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" />
        /// that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" /> that would be
        /// returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)" /> .
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public Type GetFieldType(int i)
        {
            EnsureNotDisposed();
            return _queryReader.GetColumnType(i);
        }

        /// <summary>
        /// Return the value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The <see cref="T:System.Object" /> which will contain the field value upon return.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.
        /// </exception>
        public object GetValue(int i)
        {
            EnsureNotDisposed();
            return _queryReader[i];
        }

        /// <summary>
        /// Gets all the attribute fields in the collection for the current record.
        /// </summary>
        /// <param name="values">An array of <see cref="T:System.Object" /> to copy the attribute fields into.</param>
        /// <returns>
        /// The number of instances of <see cref="T:System.Object" /> in the array.
        /// </returns>
        public int GetValues(object[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            EnsureNotDisposed();

            var upperBound = Math.Min(values.Length, _queryReader.ColumnCount);

            for (var i = 0; i < upperBound; i++)
                values[i] = _queryReader[i];

            return upperBound;
        }

        /// <summary>
        /// Return the index of the named field.
        /// </summary>
        /// <param name="name">The name of the field to find. </param>
        /// <returns>
        /// The index of the named field.
        /// </returns>
        public int GetOrdinal(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            EnsureNotDisposed();

            for (var i = 0; i < _queryReader.ColumnCount; i++)
            {
                var columnName = _queryReader.GetColumnName(i);
                if (string.Compare(columnName, name, StringComparison.OrdinalIgnoreCase) == 0)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <param name="i"> The zero-based column ordinal.</param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public bool GetBoolean(int i)
        {
            return (bool)GetValue(i);
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column.
        /// </summary>
        /// <param name="i"> The zero-based column ordinal.</param>
        /// <returns>
        /// The 8-bit unsigned integer value of the specified column.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        /// <summary>
        /// Reads a stream of bytes from the specified column offset into the buffer as
        /// an array, starting at the given buffer offset.
        /// </summary>
        /// <param name="i">The zero-based column ordinal. </param>
        /// <param name="fieldOffset">The index within the field from which to begin the read operation. </param>
        /// <param name="buffer">The buffer into which to read the stream of bytes. </param>
        /// <param name="bufferoffset">The index for <paramref name="buffer" /> to begin the read operation. </param>
        /// <param name="length">The number of bytes to read. </param>
        /// <returns>
        /// The actual number of bytes read.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception>
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            var fieldData = (byte[])GetValue(i);

            var dataLength = fieldOffset + length;
            if (dataLength > fieldData.LongLength)
                dataLength = fieldData.LongLength - fieldOffset;

            if (dataLength <= 0)
                return 0;

            Array.Copy(fieldData, fieldOffset, buffer, bufferoffset, dataLength);
            return dataLength;
        }

        /// <summary>
        /// Gets the character value of the specified column.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>
        /// The character value of the specified column.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        /// <summary>
        /// Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <param name="i">The zero-based column ordinal. </param>
        /// <param name="buffer">The buffer into which to read the stream of bytes. </param>
        /// <param name="length">The number of bytes to read. </param>
        /// <param name="bufferoffset">The index for <paramref name="buffer" /> to begin the read operation. </param>
        /// <param name="fieldoffset">The index within the row from which to begin the read operation. </param>
        /// <returns>
        /// The actual number of characters read.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception>
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            var fieldData = (char[])GetValue(i);

            var dataLength = fieldoffset + length;
            if (dataLength > fieldData.LongLength)
                dataLength = fieldData.LongLength - fieldoffset;

            if (dataLength <= 0)
                return 0;

            Array.Copy(fieldData, fieldoffset, buffer, bufferoffset, dataLength);
            return dataLength;
        }

        /// <summary>
        /// Returns the guid value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The guid value of the specified field.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.
        /// </exception>
        public Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The 16-bit signed integer value of the specified field.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public short GetInt16(int i)
        {
            return (short)GetValue(i);
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The 32-bit signed integer value of the specified field.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public int GetInt32(int i)
        {
            return (int)GetValue(i);
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The 64-bit signed integer value of the specified field.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public long GetInt64(int i)
        {
            return (long)GetValue(i);
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The single-precision floating point number of the specified field.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public float GetFloat(int i)
        {
            return (float)GetValue(i);
        }

        /// <summary>
        /// Gets the double-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The double-precision floating point number of the specified field.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The string value of the specified field.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public string GetString(int i)
        {
            return (string)GetValue(i);
        }

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The fixed-position numeric value of the specified field.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public decimal GetDecimal(int i)
        {
            return (decimal)GetValue(i);
        }

        /// <summary>
        /// Gets the date and time data value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The date and time data value of the specified field.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Data.IDataReader" /> to be used when the field points to more remote structured data.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// An <see cref="T:System.Data.IDataReader" /> to be used when the field points to more remote structured data.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        IDataReader IDataRecord.GetData(int i)
        {
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Return whether the specified field is set to null.
        /// </summary>
        /// <param name="i">The index of the field to find. </param>
        /// <returns>
        /// <see langword="true" /> if the specified field is set to null, otherwise <see langword="false" />.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception>
        public bool IsDBNull(int i)
        {
            return GetValue(i) == null;
        }

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        public int FieldCount
        {
            get
            {
                EnsureNotDisposed();
                return _queryReader.ColumnCount;
            }
        }

        /// <summary>
        /// Gets the column located at the specified index.
        /// </summary>
        /// <param name="i">The index of the column to get.</param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.</exception>
        public object this[int i]
        {
            get { return GetValue(i); }
        }

        /// <summary>
        /// Gets the column with the specified name.
        /// </summary>
        /// <param name="name">The name of the column to find.</param>
        /// <exception cref="T:System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public object this[string name]
        {
            get
            {
                var index = GetOrdinal(name);
                if (index >= 0 && index <= FieldCount)
                    return GetValue(index);

                throw new IndexOutOfRangeException();
            }
        }
    }
}