using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace ZeroG.JSON.Tests
{
    [TestClass]
    public class JSONTextStreamTests
    {
        [TestMethod]
        public void ReadSyncNoBuffer()
        {
            string value = "asdf";
            var text = new JSONTextStream(new StringReader(value), 0);
            Assert.AreEqual('a', (char)text.Peek());
            Assert.AreEqual('a', text.Read());

            Assert.AreEqual('s', (char)text.Peek());
            Assert.AreEqual('s', text.Read());

            Assert.AreEqual('d', (char)text.Peek());
            Assert.AreEqual('d', text.Read());

            Assert.AreEqual('f', (char)text.Peek());
            Assert.AreEqual('f', text.Read());

            Assert.AreEqual(-1, text.Read());
        }

        [TestMethod]
        public void ReadSyncWithBuffer()
        {
            string value = "asdf";
            var text = new JSONTextStream(new StringReader(value), 2);
            Assert.AreEqual('a', (char)text.Peek());
            Assert.AreEqual('a', text.Read());

            Assert.AreEqual('s', (char)text.Peek());
            Assert.AreEqual('s', text.Read());

            Assert.AreEqual('d', (char)text.Peek());
            Assert.AreEqual('d', text.Read());

            Assert.AreEqual('f', (char)text.Peek());
            Assert.AreEqual('f', text.Read());

            Assert.AreEqual(-1, text.Read());
        }

        [TestMethod]
        public async Task ReadAsyncNoBuffer()
        {
            string value = "asdf";
            var text = new JSONTextStream(new StringReader(value), 0);
            Assert.AreEqual('a', (char)text.Peek());
            Assert.AreEqual('a', await text.ReadAsync());

            Assert.AreEqual('s', (char)text.Peek());
            Assert.AreEqual('s', await text.ReadAsync());

            Assert.AreEqual('d', (char)text.Peek());
            Assert.AreEqual('d', await text.ReadAsync());

            Assert.AreEqual('f', (char)text.Peek());
            Assert.AreEqual('f', await text.ReadAsync());

            Assert.AreEqual(-1, await text.ReadAsync());
        }

        [TestMethod]
        public async Task ReadAsyncWithBuffer()
        {
            string value = "asdf";
            var text = new JSONTextStream(new StringReader(value), 2);
            Assert.AreEqual('a', (char)text.Peek());
            Assert.AreEqual('a', await text.ReadAsync());

            Assert.AreEqual('s', (char)text.Peek());
            Assert.AreEqual('s', await text.ReadAsync());

            Assert.AreEqual('d', (char)text.Peek());
            Assert.AreEqual('d', await text.ReadAsync());

            Assert.AreEqual('f', (char)text.Peek());
            Assert.AreEqual('f', await text.ReadAsync());

            Assert.AreEqual(-1, await text.ReadAsync());

            text = new JSONTextStream(new StringReader(value));
            Assert.AreEqual('a', (char)text.Peek());
            await text.ReadAsync();
            await text.ReadAsync();
            await text.ReadAsync();
            Assert.AreEqual('f', (char)text.Peek());
            await text.ReadAsync();

            Assert.AreEqual(-1, await text.ReadAsync());
        }
    }
}
