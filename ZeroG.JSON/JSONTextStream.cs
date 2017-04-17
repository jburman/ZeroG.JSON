using System.IO;
using System.Threading.Tasks;

namespace ZeroG.JSON
{
    public class JSONTextStream
    {
        private char[] _buffer;
        private bool _isBuffered;
        private int _bufferSize;
        private int _currentBufferSize = 0;
        private int _position = 0;
        private char[] _throwAway;

        private TextReader _textSource;

        public JSONTextStream(TextReader text, int bufferSize = 4096)
        {
            _bufferSize = bufferSize;
            if (bufferSize > 0)
            {
                _buffer = new char[bufferSize];
                _isBuffered = true;
            }

            _textSource = text;
            _throwAway = new char[1];
        }

        public int Read() => _isBuffered ? _ReadBuffer() : _textSource.Read();
        public int Peek() => _isBuffered ? _PeekBuffer() : _textSource.Peek();

        private bool _EnsureBuffer()
        {
            if (_position == _currentBufferSize)
            {
                _currentBufferSize = _textSource.ReadBlock(_buffer, 0, _bufferSize);
                _position = 0;
            }
            return _currentBufferSize > 0;
        }

        private int _ReadBuffer() => _EnsureBuffer() ? _buffer[_position++] : -1;
        private int _PeekBuffer() => _CanReadBuffer() ? _buffer[_position] : _textSource.Peek();

        public async Task<int> ReadAsync()
        {
            if (_isBuffered)
                return await _ReadBufferAsync().ConfigureAwait(false);
            else
            {
                if (await _textSource.ReadAsync(_throwAway, 0, 1).ConfigureAwait(false) == 1)
                    return _throwAway[0];
                else
                    return -1;
            }
        }

        private bool _CanReadBuffer() => _position < _currentBufferSize;

        private async Task<bool> _EnsureBufferAsync()
        {
            if (_position == _currentBufferSize)
            {
                _currentBufferSize = await _textSource.ReadBlockAsync(_buffer, 0, _bufferSize).ConfigureAwait(false);
                _position = 0;
            }
            return _currentBufferSize > 0;
        }

        private async Task<int> _ReadBufferAsync() => await _EnsureBufferAsync().ConfigureAwait(false) ? 
            _buffer[_position++] : -1;
    }
}
