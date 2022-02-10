/* MIT License

Copyright (c) 2016 Edward Rowe, RedBlueGames

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class RenameSequencePreset : ISerializationCallbackReceiver
    {
        [SerializeField]
        private string name;

        [SerializeField]
        private string serializedOperationSequence;

        private RenameOperationSequence<IRenameOperation> operationSequence;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public RenameOperationSequence<IRenameOperation> OperationSequence
        {
            get
            {
                return operationSequence;
            }

            set
            {
                operationSequence = value;
            }
        }

        public void OnBeforeSerialize()
        {
            if (OperationSequence != null)
            {
                serializedOperationSequence = OperationSequence.ToSerializableString();
            }
            else
            {
                serializedOperationSequence = string.Empty;
            }
        }

        public void OnAfterDeserialize()
        {
            operationSequence = RenameOperationSequence<IRenameOperation>.FromString(
                serializedOperationSequence);
        }

        public override int GetHashCode()
        {
            // I'm never going to hash these so just use base
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var otherPreset = obj as RenameSequencePreset;
            if (otherPreset == null)
            {
                return false;
            }

            if (Name != otherPreset.Name)
            {
                return false;
            }

            return OperationSequence.Equals(otherPreset.OperationSequence);
        }
    }
}