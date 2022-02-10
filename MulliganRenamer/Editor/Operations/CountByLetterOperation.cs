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
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Rename operation that counts using letters of the alphabet.
    /// </summary>
    [System.Serializable]
    public class CountByLetterOperation : IRenameOperation
    {
        private static readonly string[] UppercaseAlphabet = new string[]
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O",
            "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
        };

        private static readonly string[] LowercaseAlphabet = new string[]
        {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o",
            "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
        };

        [SerializeField]
        private string[] countSequence;

        [SerializeField]
        private int startingCount;

        [SerializeField]
        private int increment;

        [SerializeField]
        private bool doNotCarryOver;

        [SerializeField]
        private bool prepend;

        [SerializeField]
        private StringPreset preset;

        /// <summary>
        /// Presets of possible strings to use instead of custom strings
        /// </summary>
        public enum StringPreset
        {
            Custom = 0,
            LowercaseAlphabet = 1,
            UppercaseAlphabet = 2
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RedBlueGames.MulliganRenamer.CountByLetterOperation"/> class.
        /// </summary>
        public CountByLetterOperation()
        {
            countSequence = new string[0];
            startingCount = 0;
            increment = 1;
            doNotCarryOver = false;
            prepend = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RedBlueGames.MulliganRenamer.CountByLetterOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public CountByLetterOperation(CountByLetterOperation operationToCopy)
        {
            DoNotCarryOver = operationToCopy.DoNotCarryOver;
            StartingCount = operationToCopy.StartingCount;
            Increment = operationToCopy.Increment;
            preset = operationToCopy.preset;
            countSequence = new string[operationToCopy.CountSequence.Length];
            operationToCopy.countSequence.CopyTo(countSequence, 0);
            Prepend = operationToCopy.Prepend;
        }

        /// <summary>
        /// Gets or sets the count sequence, the sequence of strings to apply in order, corresponding
        /// with the count.
        /// </summary>
        public string[] CountSequence
        {
            get
            {
                if (preset == StringPreset.LowercaseAlphabet)
                {
                    return LowercaseAlphabet;
                }
                else if (preset == StringPreset.UppercaseAlphabet)
                {
                    return UppercaseAlphabet;
                }
                else
                {
                    return countSequence;
                }
            }
        }

        /// <summary>
        /// Gets or sets the starting count which offsets all letter assignments.
        /// </summary>
        public int StartingCount
        {
            get
            {
                return startingCount;
            }

            set
            {
                startingCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the increment to use when counting.
        /// </summary>
        public int Increment
        {
            get
            {
                return increment;
            }

            set
            {
                increment = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this
        /// <see cref="T:RedBlueGames.MulliganRenamer.CountByLetterOperation"/> should not carry
        /// over the sequence to another "digit".
        /// </summary>
        public bool DoNotCarryOver
        {
            get
            {
                return doNotCarryOver;
            }

            set
            {
                doNotCarryOver = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this
        /// <see cref="T:RedBlueGames.MulliganRenamer.CountByLetterOperation"/> should prepend the count
        /// to the front of the string
        /// </summary>
        public bool Prepend
        {
            get
            {
                return prepend;
            }

            set
            {
                prepend = value;
            }
        }

        /// <summary>
        /// Gets the current Preset to use for letter counting
        /// </summary>
        public StringPreset Preset
        {
            get
            {
                return preset;
            }
        }

        /// <summary>
        /// Checks if this RenameOperation has errors in its configuration.
        /// </summary>
        /// <returns><c>true</c>, if operation has errors, <c>false</c> otherwise.</returns>
        public bool HasErrors()
        {
            return false;
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public virtual IRenameOperation Clone()
        {
            var clone = new CountByLetterOperation(this);
            return clone;
        }

        /// <summary>
        /// Rename the specified input, using the relativeCount.
        /// </summary>
        /// <param name="input">Input String to rename.</param>
        /// <param name="relativeCount">Relative count. This can be used for enumeration.</param>
        /// <returns>A new string renamed according to the rename operation's rules.</returns>
        public RenameResult Rename(string input, int relativeCount)
        {
            var renameResult = new RenameResult();
            if (!string.IsNullOrEmpty(input) && !Prepend)
            {
                renameResult.Add(new Diff(input, DiffOperation.Equal));
            }

            var offsetCount = (relativeCount * Increment) + StartingCount;
            var stringToInsert = GetStringFromSequenceForIndex(offsetCount);
            if (!string.IsNullOrEmpty(stringToInsert))
            {
                renameResult.Add(new Diff(stringToInsert, DiffOperation.Insertion));
            }

            if (Prepend)
            {
                renameResult.Add(new Diff(input, DiffOperation.Equal));
            }

            return renameResult;
        }

        /// <summary>
        /// Gets the string to use from the sequence based on the index
        /// </summary>
        /// <returns>The string to use for the given index.</returns>
        /// <param name="index">Index to query.</param>
        public string GetStringFromSequenceForIndex(int index)
        {
            string result = string.Empty;
            while (index >= 0 && CountSequence.Length > 0)
            {
                var wrappedIndex = index % CountSequence.Length;
                var value = CountSequence[wrappedIndex];
                result = string.Concat(value, result);
                index = (index / CountSequence.Length) - 1;

                if (DoNotCarryOver)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Sets a custom string sequence to count from.
        /// </summary>
        /// <param name="sequence">Sequence of strings to count from</param>
        public void SetCountSequence(string[] sequence)
        {
            preset = StringPreset.Custom;
            countSequence = sequence;
        }

        /// <summary>
        /// Sets a preset to use when counting.
        /// </summary>
        /// <param name="countPreset">Preset of strings to use when counting.</param>
        public void SetCountSequencePreset(StringPreset countPreset)
        {
            preset = countPreset;
        }

        /// <summary>
        /// Gets the hash code for the operation
        /// </summary>
        /// <returns>A unique hash code from the values</returns>
        public override int GetHashCode()
        {
            // Easy hash method:
            // https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            int hash = 17;
            hash = hash * 23 + CountSequence.GetHashCode();
            hash = hash * 23 + StartingCount.GetHashCode();
            hash = hash * 23 + Increment.GetHashCode();
            hash = hash * 23 + DoNotCarryOver.GetHashCode();
            hash = hash * 23 + Prepend.GetHashCode();
            hash = hash * 23 + Preset.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns whether or not this rename operation is equal to another and returns the result.
        /// </summary>
        /// <returns>True if the operations are equal.true False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var otherAsOp = obj as CountByLetterOperation;
            if (otherAsOp == null)
            {
                return false;
            }

            if (CountSequence.Length != otherAsOp.CountSequence.Length)
            {
                return false;
            }

            for (int i = 0; i < CountSequence.Length; ++i)
            {
                if (CountSequence[i] != otherAsOp.CountSequence[i])
                {
                    return false;
                }
            }

            if (StartingCount != otherAsOp.StartingCount)
            {
                return false;
            }

            if (Increment != otherAsOp.Increment)
            {
                return false;
            }

            if (DoNotCarryOver != otherAsOp.DoNotCarryOver)
            {
                return false;
            }

            if (Prepend != otherAsOp.Prepend)
            {
                return false;
            }

            if (Preset != otherAsOp.Preset)
            {
                return false;
            }

            return true;
        }
    }
}