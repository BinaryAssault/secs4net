﻿using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;

namespace Secs4Net
{
    internal sealed class StringItem<TFormat> : SecsItem<TFormat, string>
         where TFormat : IFormat<string>
    {
        private readonly Pool<StringItem<TFormat>> _pool;
        private string _str = string.Empty;

        internal StringItem(Pool<StringItem<TFormat>> pool = null)
        {
            _pool = pool;
        }

        internal void SetValue(string itemValue)
        {
            _str = itemValue;
        }

        internal override void Release()
        {
            _pool?.Return(this);
        }

        protected override ArraySegment<byte> GetEncodedBytes()
        {
            if (string.IsNullOrEmpty(_str))
                return EncodEmpty(Format);

            var bytelength = _str.Length;
            var result = GetEncodedBuffer(Format, bytelength, out var headerLength);
            var encoder = Format == SecsFormat.ASCII ? Encoding.ASCII : SecsExtension.JIS8Encoding;
            encoder.GetBytes(_str, 0, _str.Length, result, headerLength);
            return new ArraySegment<byte>(result, 0, headerLength + bytelength);
        }

        public override int Count => _str.Length;
        public override IEnumerable Values => _str;
        public override string GetString() => _str;

        public override bool IsMatch(SecsItem target)
        {
            if (ReferenceEquals(this, target))
                return true;

            if (Format != target.Format)
                return false;

            if (target.Count == 0)
                return true;

            if (Count != target.Count)
                return false;

            return _str == Unsafe.As<StringItem<TFormat>>(target)._str;
        }

        public override string ToString()
            => $"<{(Format == SecsFormat.ASCII ? "A" : "J")} [{_str.Length}] {_str} >";
    }
}
