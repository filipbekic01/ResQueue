using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ResQueue
{
    public static class UserAvatarGenerator
    {
        // List of colors
        private static readonly string[] Colors = new[]
        {
            "#e57373", // Red
            "#f06292", // Pink
            "#ba68c8", // Purple
            "#9575cd", // Deep Purple
            "#7986cb", // Indigo
            "#64b5f6", // Blue
            "#4fc3f7", // Light Blue
            "#4dd0e1", // Cyan
            "#4db6ac", // Teal
            "#81c784", // Green
            "#aed581", // Light Green
            "#dce775", // Lime
            "#fff176", // Yellow
            "#ffd54f", // Amber
            "#ffb74d", // Orange
            "#ff8a65", // Deep Orange
            "#a1887f", // Brown
            "#e0e0e0", // Grey
            "#90a4ae"  // Blue Grey
        };

        // Thread-safe random number generator
        private static readonly Random GlobalRandom = new Random();
        [ThreadStatic]
        private static Random _localRandom;

        /// <summary>
        /// Generates an avatar with a random color from the predefined array and a random pattern.
        /// </summary>
        /// <param name="uniqueId">A unique identifier (e.g., user ID or email).</param>
        /// <param name="size">The width and height of the SVG (default is 100).</param>
        /// <returns>A Base64-encoded data URI containing the SVG markup.</returns>
        public static string GenerateUniqueAvatar(string uniqueId, int size = 100)
        {
            if (string.IsNullOrWhiteSpace(uniqueId))
                throw new ArgumentException("Unique ID must be a non-empty string.", nameof(uniqueId));

            if (size <= 0)
                throw new ArgumentException("Size must be a positive integer.", nameof(size));

            var gridSize = 5; // Grid size

            // Initialize thread-local random
            if (_localRandom == null)
            {
                int seed;
                lock (GlobalRandom)
                {
                    seed = GlobalRandom.Next();
                }
                _localRandom = new Random(seed);
            }

            // Select a random color from the Colors array
            var colorIndex = _localRandom.Next(Colors.Length);
            var color = Colors[colorIndex];

            // Generate random bytes for the pattern
            var totalCells = gridSize * gridSize;
            var randomBytes = new byte[totalCells];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // Create the SVG builder
            var svgBuilder = new StringBuilder();
            svgBuilder.AppendLine(
                $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{size}\" height=\"{size}\" viewBox=\"0 0 {size} {size}\">");

            var cellSize = size / (double)gridSize;
            var byteIndex = 0;

            // Generate the symmetric grid pattern
            for (var row = 0; row < gridSize; row++)
            {
                for (var col = 0; col < (gridSize + 1) / 2; col++) // Only half columns for symmetry
                {
                    // Ensure we stay within the randomBytes array
                    if (byteIndex >= randomBytes.Length)
                    {
                        // Restart from the beginning if we've used all bytes
                        byteIndex = 0;
                    }

                    // Decide whether to fill the square based on the random byte
                    var fill = (randomBytes[byteIndex++] % 2) == 0;

                    if (fill)
                    {
                        var x = col * cellSize;
                        var y = row * cellSize;

                        // Draw the square and its mirrored counterpart
                        svgBuilder.AppendLine(
                            $"  <rect x=\"{x.ToString("F2", CultureInfo.InvariantCulture)}\" y=\"{y.ToString("F2", CultureInfo.InvariantCulture)}\" width=\"{cellSize.ToString("F2", CultureInfo.InvariantCulture)}\" height=\"{cellSize.ToString("F2", CultureInfo.InvariantCulture)}\" fill=\"{color}\" />");

                        if (col != gridSize - col - 1) // Avoid duplicating the center column
                        {
                            var mirrorX = (gridSize - col - 1) * cellSize;
                            svgBuilder.AppendLine(
                                $"  <rect x=\"{mirrorX.ToString("F2", CultureInfo.InvariantCulture)}\" y=\"{y.ToString("F2", CultureInfo.InvariantCulture)}\" width=\"{cellSize.ToString("F2", CultureInfo.InvariantCulture)}\" height=\"{cellSize.ToString("F2", CultureInfo.InvariantCulture)}\" fill=\"{color}\" />");
                        }
                    }
                }
            }

            svgBuilder.AppendLine("</svg>");

            // Convert SVG content to Base64-encoded data URI
            var svgContent = svgBuilder.ToString();
            var svgBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svgContent));
            var dataUri = $"data:image/svg+xml;base64,{svgBase64}";

            return dataUri;
        }
    }
}
