// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System.Buffers;
using Microsoft.Extensions.ObjectPool;
using TestJSonConversion.SimpleCimJson.Elements;
using TestJSonConversion.SimpleCimJson.Reader;

namespace TestJSonConversion.SimpleCimJson.Factories
{
    internal static class CimJsonObjectPools
    {
        private static ObjectPool<CimStringValueElement> _stringElementsPool = new DefaultObjectPool<CimStringValueElement>(new DefaultPooledObjectPolicy<CimStringValueElement>());
        private static ObjectPool<CimIntValueElement> _intElementsPool = new DefaultObjectPool<CimIntValueElement>(new DefaultPooledObjectPolicy<CimIntValueElement>());
        private static ObjectPool<CimBoolValueElement> _boolElementsPool = new DefaultObjectPool<CimBoolValueElement>(new DefaultPooledObjectPolicy<CimBoolValueElement>());
        private static ObjectPool<CimObjectElement> _objectElementsPool = new DefaultObjectPool<CimObjectElement>(new DefaultPooledObjectPolicy<CimObjectElement>());
        private static ObjectPool<ArrayBufferWriter<byte>> _arrayBufferPool = new DefaultObjectPool<ArrayBufferWriter<byte>>(new DefaultPooledObjectPolicy<ArrayBufferWriter<byte>>());

        public static CimStringValueElement GetStringValueElement(string key, CimXmlReader reader)
        {
            var pooledElement = _stringElementsPool.Get();
            pooledElement.Key = key;
            pooledElement.Value = reader.ReadValue();
            return pooledElement;
        }

        public static CimIntValueElement GetIntValueElement(string key, CimXmlReader reader)
        {
            var pooledElement = _intElementsPool.Get();
            pooledElement.Key = key;
            pooledElement.Value = int.Parse(reader.ReadValue().Span);
            return pooledElement;
        }

        public static CimBoolValueElement GetBoolValueElement(string key, CimXmlReader reader)
        {
            var pooledElement = _boolElementsPool.Get();
            pooledElement.Key = key;
            pooledElement.Value = bool.Parse(reader.ReadValue().Span);
            return pooledElement;
        }

        public static CimObjectElement GetObjectElement(string key, int capacity)
        {
            var pooledElement = _objectElementsPool.Get();
            pooledElement.Key = key;
            pooledElement.SetSize(capacity);
            return pooledElement;
        }

        public static ArrayBufferWriter<byte> GetArrayWriter()
        {
            var pooledBuffer = _arrayBufferPool.Get();
            pooledBuffer.Clear();
            return pooledBuffer;
        }

        public static void ReturnElement(CimStringValueElement element)
        {
            _stringElementsPool.Return(element);
        }

        public static void ReturnElement(CimIntValueElement element)
        {
            _intElementsPool.Return(element);
        }

        public static void ReturnElement(CimBoolValueElement element)
        {
            _boolElementsPool.Return(element);
        }

        public static void ReturnElement(CimObjectElement element)
        {
            _objectElementsPool.Return(element);
        }

        public static void ReturnElement(ArrayBufferWriter<byte> element)
        {
            _arrayBufferPool.Return(element);
        }
    }
}