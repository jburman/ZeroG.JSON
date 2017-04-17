#region License, Terms and Conditions
// Copyright (c) 2017 Jeremy Burman
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System.Collections.Generic;

namespace ZeroG.JSON
{
    using JSONTokenStream = IEnumerator<JSONToken>;

    /// <summary>
    /// Walks the tokens returned from a JSONTokenizer and validates that they form a valid 
    /// JSON construct.
    /// Additionally, if a JSONWalkingEvents instance is supplied, then its events will be 
    /// raised as the JSON tokens are consumed by the validator.
    /// </summary>
    /// <seealso cref="ZeroG.Lang.JSON.JSONWalkingEvents"/>
    public class JSONWalkingValidator
    {
        #region Private helpers

        /// <summary>
        /// Start walking the JSON structure.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="events"></param>
        private void _Start(JSONTokenStream tokens, JSONWalkingEvents events)
        {
            tokens.MoveNext();
            JSONToken next = tokens.Current;

            if (next.Is(JSONTokenType.OBJECT_START))
                _Object(tokens, events);
            else if (next.Is(JSONTokenType.ARRAY_START))
                _Array(tokens, events);
            else
                throw new JSONValidationException("Expected OBJECT or ARRAY but got: " + next.Type);
        }

        /// <summary>
        /// Walk an Object.  Fires ObjectStart, ObjectKey, ObjectNext, and ObjectEnd
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="events"></param>
        private void _Object(JSONTokenStream tokens, JSONWalkingEvents events)
        {
            
            events?.RaiseObjectStart();

            tokens.MoveNext();
            JSONToken next = tokens.Current;

            while (!next.Is(JSONTokenType.OBJECT_END))
            {
                _ObjectField(tokens, events);

                tokens.MoveNext();
                next = tokens.Current;

                if (next.Is(JSONTokenType.COMMA))
                {
                    tokens.MoveNext();
                    next = tokens.Current;

                    events?.RaiseObjectNext();
                }
            }

            if (next.Is(JSONTokenType.OBJECT_END))
            {
                events?.RaiseObjectEnd();
                return;
            }
            else
                throw new JSONValidationException("Expected VALUE or end of OBJECT but got: " + next.Type);
        }

        private void _DispatchEvent(JSONTokenStream tokens, JSONWalkingEvents events)
        {
            JSONToken next = tokens.Current;

            switch (next.Type)
            {
                case JSONTokenType.STRING:
                    events?.RaiseString(next.StrValue);
                    break;
                case JSONTokenType.NUMBER:
                    events?.RaiseNumber(next.NumValue);
                    break;
                case JSONTokenType.KEYWORD_TRUE:
                    events?.RaiseBoolean(true);
                    break;
                case JSONTokenType.KEYWORD_FALSE:
                    events?.RaiseBoolean(false);
                    break;
                case JSONTokenType.KEYWORD_NULL:
                    events?.RaiseNull();
                    break;
                case JSONTokenType.OBJECT_START:
                    _Object(tokens, events);
                    break;
                case JSONTokenType.ARRAY_START:
                    _Array(tokens, events);
                    break;
                default:
                    throw new JSONValidationException("Expected value but got: " + next.Type);
            }
        }

        private void _ObjectField(JSONTokenStream tokens, JSONWalkingEvents events)
        {
            JSONToken next = tokens.Current;

            if (next.Is(JSONTokenType.STRING))
            {
                events?.RaiseObjectKey(next.StrValue);

                tokens.MoveNext();
                next = tokens.Current;
                if (next.Is(JSONTokenType.COLON))
                {
                    tokens.MoveNext();

                    _DispatchEvent(tokens, events);
                }
                else
                    throw new JSONValidationException("Expected COLON but got: " + next.Type);
            }
            else
                throw new JSONValidationException("Expected STRING but got: " + next.Type);
        }

        /// <summary>
        /// Reads an Array.  Fires ArrayStart, ArrayNext, and ArrayEnd.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="events"></param>
        private void _Array(JSONTokenStream tokens, JSONWalkingEvents events)
        {
            events?.RaiseArrayStart();

            tokens.MoveNext();
            JSONToken next = tokens.Current;

            while (!next.Is(JSONTokenType.ARRAY_END))
            {
                _ArrayElement(tokens, events);

                tokens.MoveNext();
                next = tokens.Current;

                if (next.Is(JSONTokenType.COMMA))
                {
                    tokens.MoveNext();
                    next = tokens.Current;
                    events?.RaiseArrayNext();
                }
            }

            if (next.Is(JSONTokenType.ARRAY_END))
            {
                events?.RaiseArrayEnd();
                return;
            }
            else
                throw new JSONValidationException("Expected VALUE or end of ARRAY but got: " + next.Type);
        }

        /// <summary>
        /// Reads a String, Number, keyword, Object, or Array element within an Array.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="events"></param>
        private void _ArrayElement(JSONTokenStream tokens, JSONWalkingEvents events) => _DispatchEvent(tokens, events);
        #endregion

        /// <summary>
        /// Walks JSON tokens and validates but does not fire any events.
        /// </summary>
        /// <param name="tokens"></param>
        public void Walk(JSONTokenStream tokens) => Walk(tokens, null);

        /// <summary>
        /// Walks JSON tokens and validates and fires events on the supplied events instance.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="events"></param>
        public void Walk(JSONTokenStream tokens, JSONWalkingEvents events)
        {
            _Start(tokens, events);

            tokens.MoveNext();

            if (!tokens.Current.Is(JSONTokenType.EOF))
                throw new JSONValidationException("Expected EOF but got: " + tokens.Current.Type);
        }
    }
}
