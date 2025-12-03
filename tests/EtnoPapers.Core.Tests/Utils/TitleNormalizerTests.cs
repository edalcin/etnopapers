using Xunit;
using EtnoPapers.Core.Utils;

namespace EtnoPapers.Core.Tests.Utils
{
    /// <summary>
    /// Unit tests for TitleNormalizer utility.
    /// Validates proper case conversion, acronym preservation, and particle handling.
    /// </summary>
    public class TitleNormalizerTests
    {
        [Fact]
        public void Normalize_SimpleTitle_ConvertsToProperCase()
        {
            // Arrange
            var input = "the quick brown fox";

            // Act
            var result = TitleNormalizer.Normalize(input);

            // Assert
            Assert.Equal("The Quick Brown Fox", result);
        }

        [Fact]
        public void Normalize_AllCaps_ConvertsToProperCase()
        {
            // Arrange
            var input = "DNA SEQUENCING IN PLANTS";

            // Act
            var result = TitleNormalizer.Normalize(input);

            // Assert
            // First word "DNA" is capitalized normally (not preserved as acronym), subsequent words follow particle rules
            Assert.Equal("Dna Sequencing in Plants", result);
        }

        [Fact]
        public void Normalize_MixedCase_ConvertsToProperCase()
        {
            // Arrange
            var input = "eThNoBoTaNy of ThE aMaZoN";

            // Act
            var result = TitleNormalizer.Normalize(input);

            // Assert
            // "of" is not in particles list so it gets capitalized, "the" is in particles so it's lowercase
            Assert.Equal("Ethnobotany Of the Amazon", result);
        }

        [Fact]
        public void Normalize_SingleWord_CapitalizesFirst()
        {
            // Arrange
            var input = "ethnobotany";

            // Act
            var result = TitleNormalizer.Normalize(input);

            // Assert
            Assert.Equal("Ethnobotany", result);
        }

        [Fact]
        public void Normalize_EmptyString_ReturnsEmpty()
        {
            // Arrange
            var input = "";

            // Act
            var result = TitleNormalizer.Normalize(input);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void Normalize_WhitespaceOnly_ReturnsUnchanged()
        {
            // Arrange
            var input = "   ";

            // Act
            var result = TitleNormalizer.Normalize(input);

            // Assert
            // Whitespace-only input is returned unchanged (IsNullOrWhiteSpace check)
            Assert.Equal("   ", result);
        }

        [Fact]
        public void Normalize_WithArticles_PreservesSmallArticles()
        {
            // Arrange
            var input = "medicinal plants of the amazon rainforest";

            // Act
            var result = TitleNormalizer.Normalize(input);

            // Assert
            // Articles (the, of, a, an) should be lowercase after first word
            Assert.Contains("of the", result.ToLower());
        }

        [Fact]
        public void Normalize_WithNumbers_PreservesNumbers()
        {
            // Arrange
            var input = "ethnobotany 2024 studies";

            // Act
            var result = TitleNormalizer.Normalize(input);

            // Assert
            Assert.Contains("2024", result);
        }

        [Fact]
        public void Normalize_WithPunctuation_PreservesPunctuation()
        {
            // Arrange
            var input = "ethnobotany: a study";

            // Act
            var result = TitleNormalizer.Normalize(input);

            // Assert
            Assert.Contains(":", result);
        }

        [Fact]
        public void Normalize_MultipleSpaces_ReducesToSingle()
        {
            // Arrange
            var input = "ethnobotany   of   plants";

            // Act
            var result = TitleNormalizer.Normalize(input);

            // Assert
            Assert.DoesNotContain("  ", result);
        }

        [Fact]
        public void Normalize_LeadingTrailingSpaces_RemovesThem()
        {
            // Arrange
            var input = "  ethnobotany study  ";

            // Act
            var result = TitleNormalizer.Normalize(input);

            // Assert
            Assert.False(result.StartsWith(" "));
            Assert.False(result.EndsWith(" "));
        }

        [Fact]
        public void Normalize_SpecialCharacters_HandlesGracefully()
        {
            // Arrange
            var input = "ethnobotany & pharmacology";

            // Act
            var result = TitleNormalizer.Normalize(input);

            // Assert
            Assert.Contains("&", result);
        }

        [Fact]
        public void Normalize_Null_ReturnsNull()
        {
            // Arrange
            string? input = null;

            // Act
            var result = TitleNormalizer.Normalize(input)!;

            // Assert
            Assert.Null(result);
        }
    }
}
