// <copyright file="UIGenerationResult.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI
{
    /// <summary>
    /// Result of UI generation for a table.
    /// </summary>
    public class UIGenerationResult
    {
        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether generation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message if failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the generated TypeScript types code.
        /// </summary>
        public string? TypesCode { get; set; }

        /// <summary>
        /// Gets or sets the generated API client code.
        /// </summary>
        public string? ApiCode { get; set; }

        /// <summary>
        /// Gets or sets the generated React hooks code.
        /// </summary>
        public string? HooksCode { get; set; }

        /// <summary>
        /// Gets or sets the generated entity form code.
        /// </summary>
        public string? FormCode { get; set; }

        /// <summary>
        /// Gets or sets the generated collection grid code.
        /// </summary>
        public string? GridCode { get; set; }

        /// <summary>
        /// Gets or sets the generated page code.
        /// </summary>
        public string? PageCode { get; set; }

        /// <summary>
        /// Gets the number of generated files.
        /// </summary>
        /// <returns>File count.</returns>
        public int GetFileCount()
        {
            var count = 0;
            if (!string.IsNullOrEmpty(TypesCode))
            {
                count++;
            }

            if (!string.IsNullOrEmpty(ApiCode))
            {
                count++;
            }

            if (!string.IsNullOrEmpty(HooksCode))
            {
                count++;
            }

            if (!string.IsNullOrEmpty(FormCode))
            {
                count++;
            }

            if (!string.IsNullOrEmpty(GridCode))
            {
                count++;
            }

            if (!string.IsNullOrEmpty(PageCode))
            {
                count++;
            }

            return count;
        }

        /// <summary>
        /// Gets the total lines of generated code.
        /// </summary>
        /// <returns>Line count.</returns>
        public int GetTotalLines()
        {
            var total = 0;

            if (!string.IsNullOrEmpty(TypesCode))
            {
                total += TypesCode.Split('\n').Length;
            }

            if (!string.IsNullOrEmpty(ApiCode))
            {
                total += ApiCode.Split('\n').Length;
            }

            if (!string.IsNullOrEmpty(HooksCode))
            {
                total += HooksCode.Split('\n').Length;
            }

            if (!string.IsNullOrEmpty(FormCode))
            {
                total += FormCode.Split('\n').Length;
            }

            if (!string.IsNullOrEmpty(GridCode))
            {
                total += GridCode.Split('\n').Length;
            }

            if (!string.IsNullOrEmpty(PageCode))
            {
                total += PageCode.Split('\n').Length;
            }

            return total;
        }
    }
}
