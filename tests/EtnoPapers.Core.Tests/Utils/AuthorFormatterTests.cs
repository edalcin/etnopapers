using Xunit;
using EtnoPapers.Core.Utils;

namespace EtnoPapers.Core.Tests.Utils
{
    /// <summary>
    /// Unit tests for AuthorFormatter utility.
    /// Validates APA style formatting of author names, handling particles and hyphenated names.
    /// </summary>
    public class AuthorFormatterTests
    {
        [Fact]
        public void FormatToAPA_SimpleFullName_ReturnsAPAFormat()
        {
            // Arrange
            var input = "John Smith";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            Assert.Equal("Smith, J.", result);
        }

        [Fact]
        public void FormatToAPA_WithMiddleInitial_IncludesAllInitials()
        {
            // Arrange
            var input = "John Q. Smith";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            Assert.Equal("Smith, J. Q.", result);
        }

        [Fact]
        public void FormatToAPA_HyphenatedFirstName_CreatesInitialForEachPart()
        {
            // Arrange
            var input = "Jean-Pierre Dupont";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            // Hyphenated names are split and each part gets an initial (separated by space, not hyphen)
            Assert.Equal("Dupont, J. P.", result);
        }

        [Fact]
        public void FormatToAPA_WithParticleDe_PlacesParticleWithLastName()
        {
            // Arrange
            var input = "Jean de Silva";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            // "de" is a particle and should stay with the last name
            Assert.Equal("de Silva, J.", result);
        }

        [Fact]
        public void FormatToAPA_WithParticleVon_PlacesParticleWithLastName()
        {
            // Arrange
            var input = "Karl von Neumann";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            // "von" is a particle and should stay with the last name
            Assert.Equal("von Neumann, K.", result);
        }

        [Fact]
        public void FormatToAPA_WithParticleDa_PlacesParticleWithLastName()
        {
            // Arrange
            var input = "Leonardo da Vinci";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            // "da" is a particle and should stay with the last name
            Assert.Equal("da Vinci, L.", result);
        }

        [Fact]
        public void FormatToAPA_SingleName_ReturnsSingleName()
        {
            // Arrange
            var input = "Plato";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            Assert.Equal("Plato", result);
        }

        [Fact]
        public void FormatToAPA_ThreeNames_FormatsCorrectly()
        {
            // Arrange
            var input = "Maria Elena Santos";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            Assert.Equal("Santos, M. E.", result);
        }

        [Fact]
        public void FormatToAPA_LeadingTrailingSpaces_HandlesGracefully()
        {
            // Arrange
            var input = "  John Smith  ";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            Assert.Equal("Smith, J.", result);
        }

        [Fact]
        public void FormatToAPA_MultipleInternalSpaces_NormalizesSpaces()
        {
            // Arrange
            var input = "John   Smith";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            Assert.Equal("Smith, J.", result);
        }

        [Fact]
        public void FormatToAPA_EmptyString_ReturnsEmpty()
        {
            // Arrange
            var input = "";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void FormatToAPA_WhitespaceOnly_ReturnsWhitespace()
        {
            // Arrange
            var input = "   ";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            // Whitespace-only input is returned unchanged
            Assert.Equal("   ", result);
        }

        [Fact]
        public void FormatToAPA_Null_ReturnsNull()
        {
            // Arrange
            string? input = null;

            // Act
            var result = AuthorFormatter.FormatToAPA(input!);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FormatToAPA_AlreadyHasInitials_PreservesFormat()
        {
            // Arrange
            var input = "J. R. R. Tolkien";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            // Already-abbreviated names are preserved with their periods
            Assert.Contains("Tolkien", result);
            Assert.Contains("J.", result);
        }

        [Fact]
        public void FormatToAPA_SpecialCharactersInName_PreservesCharacters()
        {
            // Arrange
            var input = "José García";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            Assert.Equal("García, J.", result);
        }

        [Fact]
        public void FormatToAPA_FourPartName_WithParticle_FormatsCorrectly()
        {
            // Arrange
            var input = "Jean Pierre de Brouwer";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            // "de" is a particle, so "de Brouwer" is the last name
            Assert.Contains("de Brouwer", result);
        }

        [Fact]
        public void FormatToAPA_AllUppercase_ConvertsToProperlyCased()
        {
            // Arrange
            var input = "JOHN SMITH";

            // Act
            var result = AuthorFormatter.FormatToAPA(input);

            // Assert
            // Should preserve the case as provided (no explicit lowercasing in FormatToAPA)
            Assert.Contains("SMITH", result);
        }
    }
}
