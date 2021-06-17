//
// Options.cs
//
// Authors:
//  Jonathan Pryor <jpryor@novell.com>
//
// Copyright (C) 2008 Novell (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using Octopus.CommandLine.Commands;

namespace Octopus.CommandLine.OptionParsing
{
    public class OptionValueCollection : IList, IList<string>
    {
        readonly OptionContext c;
        readonly List<string> values = new List<string>();

        internal OptionValueCollection(OptionContext c)
        {
            this.c = c;
        }

        #region ICollection

        void ICollection.CopyTo(Array array, int index)
        {
            (values as ICollection).CopyTo(array, index);
        }

        bool ICollection.IsSynchronized => (values as ICollection).IsSynchronized;

        object ICollection.SyncRoot => (values as ICollection).SyncRoot;

        #endregion

        #region ICollection<T>

        public void Clear()
        {
            values.Clear();
        }

        public int Count => values.Count;

        public bool IsReadOnly => false;

        public void Add(string item)
        {
            values.Add(item);
        }

        public bool Contains(string item)
        {
            return values.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            values.CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            return values.Remove(item);
        }

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        #endregion

        #region IEnumerable<T>

        public IEnumerator<string> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        #endregion

        #region IList

        int IList.Add(object value)
        {
            return (values as IList).Add(value);
        }

        bool IList.Contains(object value)
        {
            return (values as IList).Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return (values as IList).IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            (values as IList).Insert(index, value);
        }

        void IList.Remove(object value)
        {
            (values as IList).Remove(value);
        }

        void IList.RemoveAt(int index)
        {
            (values as IList).RemoveAt(index);
        }

        bool IList.IsFixedSize => false;

        object IList.this[int index]
        {
            get { return this[index]; }
            set { (values as IList)[index] = value; }
        }

        #endregion

        #region IList<T>

        public int IndexOf(string item)
        {
            return values.IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            values.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            values.RemoveAt(index);
        }

        public string this[int index]
        {
            get
            {
                AssertValid(index);
                return index >= values.Count ? null : values[index];
            }
            set { values[index] = value; }
        }

        void AssertValid(int index)
        {
            if (c.Option == null)
                throw new InvalidOperationException("OptionContext.Option is null.");
            if (index >= c.Option.MaxValueCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (c.Option.OptionValueType == OptionValueType.Required &&
                index >= values.Count)
                throw new CommandException($"Missing required value for option '{c.OptionName}'.");
        }

        #endregion

        public List<string> ToList()
        {
            return new List<string>(values);
        }

        public string[] ToArray()
        {
            return values.ToArray();
        }

        public override string ToString()
        {
            return string.Join(", ", values.ToArray());
        }
    }
}
