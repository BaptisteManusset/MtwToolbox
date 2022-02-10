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
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// RenameOperation used to replace substrings from the rename string.
    /// </summary>
    [System.Serializable]
    public class RemoveCharactersOperation : IRenameOperation
    {
        private static readonly RenameOptions Symbols = new RenameOptions()
        {
            CharactersToRemove = "^\\s\\w",
            CharactersAreRegex = true,
            IsCaseSensitive = false
        };

        private static readonly RenameOptions Numbers = new RenameOptions()
        {
            CharactersToRemove = "\\d",
            CharactersAreRegex = true,
            IsCaseSensitive = false
        };

        private static readonly RenameOptions Whitespace = new RenameOptions()
        {
            CharactersToRemove = "\\s",
            CharactersAreRegex = true,
            IsCaseSensitive = false
        };

        private static readonly Dictionary<PresetID, RenameOptions> optionsPresets = new Dictionary<PresetID, RenameOptions>()
        {
            {PresetID.Numbers, Numbers},
            {PresetID.Whitespace, Whitespace},
            {PresetID.Symbols, Symbols}
        };

        [SerializeField]
        private RenameOptions customOptions;

        [SerializeField]
        private PresetID currentPresetID;

        private ReplaceStringOperation internalReplaceStringOperation;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveCharactersOperation"/> class.
        /// </summary>
        public RemoveCharactersOperation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveCharactersOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public RemoveCharactersOperation(RemoveCharactersOperation operationToCopy)
        {
            CopyFrom(operationToCopy);
        }

        public enum PresetID
        {
            Custom = 0,
            Numbers = 1,
            Symbols = 2,
            Whitespace = 3,
        }

        /// <summary>
        /// Gets or sets the characters to remove.
        /// </summary>
        /// <value>The characters to remove.</value>
        public string CharactersToRemove
        {
            get
            {
                return CurrentOptions.CharactersToRemove;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this
        /// <see cref="RemoveCharactersOperation+RemoveCharactersOperationOptions"/>
        /// characters are regex symbols.
        /// </summary>
        /// <value><c>true</c> if characters are regex; otherwise, <c>false</c>.</value>
        public bool CharactersAreRegex
        {
            get
            {
                return CurrentOptions.CharactersAreRegex;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the characters are matched using case sensitivity.
        /// </summary>
        /// <value><c>true</c> if search is case sensitive; otherwise, <c>false</c>.</value>
        public bool IsCaseSensitive
        {
            get
            {
                return CurrentOptions.IsCaseSensitive;
            }
        }

        public PresetID CurrentPresetID
        {
            get
            {
                return currentPresetID;
            }
        }

        private RenameOptions CurrentOptions
        {
            get
            {
                if (currentPresetID == PresetID.Custom)
                {
                    return customOptions;
                }
                else
                {
                    return optionsPresets[currentPresetID];
                }
            }
        }

        private ReplaceStringOperation InternalReplaceStringOperation
        {
            get
            {
                if (internalReplaceStringOperation == null)
                {
                    internalReplaceStringOperation = new ReplaceStringOperation();
                }

                internalReplaceStringOperation.SearchIsCaseSensitive = IsCaseSensitive;
                internalReplaceStringOperation.UseRegex = true;

                var regexPattern = CharactersToRemove;
                if (!CharactersAreRegex)
                {
                    regexPattern = Regex.Escape(regexPattern);
                }

                var charactersAsRegex = $"[{regexPattern}]";
                internalReplaceStringOperation.SearchString = charactersAsRegex;
                internalReplaceStringOperation.ReplacementString = string.Empty;

                return internalReplaceStringOperation;
            }
        }

        /// <summary>
        /// Checks if the operation has errors and returns true if it does.
        /// </summary>
        /// <returns><c>true</c>, if errors exist, <c>false</c> otherwise.</returns>
        public bool HasErrors()
        {
            return false;
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public IRenameOperation Clone()
        {
            var clone = new RemoveCharactersOperation(this);
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
            if (string.IsNullOrEmpty(input))
            {
                return RenameResult.Empty;
            }

            return InternalReplaceStringOperation.Rename(input, relativeCount);
        }

        /// <summary>
        /// Copies the state from one operation into this one
        /// </summary>
        /// <param name="other">Other.</param>
        private void CopyFrom(RemoveCharactersOperation other)
        {
            SetOptions(other.CurrentOptions);
            currentPresetID = other.currentPresetID;
        }

        public void SetOptions(RenameOptions other)
        {
            var optionsCopy = new RenameOptions();
            optionsCopy.CharactersToRemove = other.CharactersToRemove;
            optionsCopy.CharactersAreRegex = other.CharactersAreRegex;
            optionsCopy.IsCaseSensitive = other.IsCaseSensitive;

            customOptions = optionsCopy;
            currentPresetID = PresetID.Custom;
        }

        public void SetOptionPreset(PresetID preset)
        {
            currentPresetID = preset;
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
            hash = hash * 23 + customOptions.GetHashCode();
            hash = hash * 23 + currentPresetID.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns whether or not this rename operation is equal to another and returns the result.
        /// </summary>
        /// <returns>True if the operations are equal.true False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var otherAsOp = obj as RemoveCharactersOperation;
            if (otherAsOp == null)
            {
                return false;
            }

            if (!customOptions.Equals(otherAsOp.customOptions))
            {
                return false;
            }

            if (currentPresetID != otherAsOp.currentPresetID)
            {
                return false;
            }

            return true;
        }

        [System.Serializable]
        public struct RenameOptions
        {
            [SerializeField]
            private string charactersToRemove;

            [SerializeField]
            private bool charactersAreRegex;

            [SerializeField]
            private bool isCaseSensitive;

            public string CharactersToRemove
            {
                get
                {
                    if (string.IsNullOrEmpty(charactersToRemove))
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return charactersToRemove;
                    }
                }

                set
                {
                    charactersToRemove = value;
                }
            }

            public bool IsCaseSensitive
            {
                get
                {
                    return isCaseSensitive;
                }

                set
                {
                    isCaseSensitive = value;
                }
            }

            public bool CharactersAreRegex
            {
                get
                {
                    return charactersAreRegex;
                }

                set
                {
                    charactersAreRegex = value;
                }
            }
        }
    }
}