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
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Preview of all of the resultant renames from a BulkRename.
    /// </summary>
    public class BulkRenamePreview
    {
        private Dictionary<Object, RenamePreview> renamePreviews;
        private List<RenamePreview> renamePreviewsList;

        private HashSet<RenamePreview> duplicatePreviews;

        /// <summary>
        /// Gets the number of objects in the preview.
        /// </summary>
        public int NumObjects
        {
            get
            {
                return renamePreviewsList.Count;
            }
        }

        /// <summary>
        /// Gets the number of steps in the rename sequence
        /// </summary>
        public int NumSteps
        {
            get
            {
                if (NumObjects == 0)
                {
                    return 0;
                }

                // All rename results sequences should have the same number of steps so just grab the first
                return renamePreviewsList[0].RenameResultSequence.NumSteps;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this preview contains objects with warnings.
        /// </summary>
        public bool HasWarnings
        {
            get
            {
                for (int i = 0; i < NumObjects; ++i)
                {
                    var preview = GetPreviewAtIndex(i);
                    if (preview.HasWarnings)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RedBlueGames.MulliganRenamer.BulkRenamePreview"/> class.
        /// </summary>
        /// <param name="previews">Previews in the collection.</param>
        public BulkRenamePreview(RenamePreview[] previews, AssetCache existingAssets)
        {
            renamePreviews = new Dictionary<Object, RenamePreview>();
            renamePreviewsList = new List<RenamePreview>(previews.Length);
            duplicatePreviews = new HashSet<RenamePreview>();

            for (int i = 0; i < previews.Length; ++i)
            {
                AddEntry(previews[i]);
            }

            var previewsWithDuplicateNames = GetPreviewsWithDuplicateNames(renamePreviewsList, existingAssets);
            foreach (var preview in previewsWithDuplicateNames)
            {
                duplicatePreviews.Add(preview);
            }
        }

        /// <summary>
        /// Gets the preview for the object at the supplied index.
        /// </summary>
        /// <returns>The preview at index.</returns>
        /// <param name="index">Index to query.</param>
        public RenamePreview GetPreviewAtIndex(int index)
        {
            return renamePreviewsList[index];
        }

        /// <summary>
        /// Determines if the BulkPreview contains a preview for the specified object.
        /// </summary>
        /// <returns><c>true</c>, if object is in the bulk rename, <c>false</c> otherwise.</returns>
        /// <param name="obj">Object to query.</param>
        public bool ContainsPreviewForObject(Object obj)
        {
            return renamePreviews.ContainsKey(obj);
        }

        /// <summary>
        /// Gets the preview for the specified object.
        /// </summary>
        /// <returns>The preview for object.</returns>
        /// <param name="obj">Object to query.</param>
        public RenamePreview GetPreviewForObject(Object obj)
        {
            return renamePreviews[obj];
        }

        /// <summary>
        /// Check if the RenamePreview's final name will match an existing asset's name. This means it will fail
        /// to be renamed if we try.
        /// </summary>
        /// <returns><c>true</c>, if the renamed object's name will collide with existing asset, <c>false</c> otherwise.</returns>
        /// <param name="preview">Preview to check.</param>
        public bool WillRenameCollideWithExistingAsset(RenamePreview preview)
        {
            return duplicatePreviews.Contains(preview);
        }

        private void AddEntry(RenamePreview singlePreview)
        {
            // Keeping a list and a dictionary for fast access by index and by object...
            renamePreviews.Add(singlePreview.ObjectToRename, singlePreview);
            renamePreviewsList.Add(singlePreview);
        }

        private static List<RenamePreview> GetPreviewsWithDuplicateNames(IList<RenamePreview> previews, AssetCache assetCache)
        {
            var assetPreviews = new List<RenamePreview>();
            for (int i = 0; i < previews.Count; ++i)
            {
                var previewForObject = previews[i];
                if (previewForObject.ObjectToRename.IsAsset())
                {
                    assetPreviews.Add(previewForObject);
                }
            }

            // Get all the cached file paths, but remove any that are in the preview
            // because those names could be different. We want to test that NEW names
            // don't collide with existing assets.
            HashSet<string> allFinalFilePaths = assetCache.GetAllPathsHashed();
            foreach (var assetPreview in assetPreviews)
            {
                allFinalFilePaths.Remove(assetPreview.OriginalPathToSubAsset);
            }

            // Now hash the new names and check if they collide with the existing assets
            var problemPreviews = new List<RenamePreview>();
            var unchangedAssetPreviews = new List<RenamePreview>();
            var changedAssetPreviews = new List<RenamePreview>();

            // Separate unchangedAssets from changedAsests
            foreach (var assetPreview in assetPreviews)
            {
                var thisObject = assetPreview.ObjectToRename;
                var thisResult = assetPreview.RenameResultSequence;
                if (thisResult.NewName == thisResult.OriginalName)
                {
                    unchangedAssetPreviews.Add(assetPreview);
                }
                else
                {
                    changedAssetPreviews.Add(assetPreview);
                }
            }

            // First add all the unchanged results, so that we collide on the
            // first time adding new names. This fixes an issue where
            // you'd rename one object which now matches a second, but the second gets
            // the warning instead of the first.
            var previewsSorted = new List<RenamePreview>();
            previewsSorted.AddRange(unchangedAssetPreviews);
            previewsSorted.AddRange(changedAssetPreviews);
            foreach (var renamePreview in previewsSorted)
            {
                var resultingPath = renamePreview.GetResultingPath();
                if (allFinalFilePaths.Contains(resultingPath))
                {
                    problemPreviews.Add(renamePreview);
                }
                else
                {
                    allFinalFilePaths.Add(resultingPath);
                }
            }

            return problemPreviews;
        }
    }
}