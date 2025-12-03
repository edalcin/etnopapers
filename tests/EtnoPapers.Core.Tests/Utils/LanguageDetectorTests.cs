using Xunit;
using EtnoPapers.Core.Utils;

namespace EtnoPapers.Core.Tests.Utils
{
    /// <summary>
    /// Unit tests for LanguageDetector utility.
    /// Validates language detection for Portuguese, English, and Spanish text.
    /// </summary>
    public class LanguageDetectorTests
    {
        [Fact]
        public void DetectLanguage_PortugueseText_ReturnsPortuguese()
        {
            // Arrange
            var input = "O etnobotânica é o estudo das relações entre plantas e as pessoas";

            // Act
            var result = LanguageDetector.DetectLanguage(input);

            // Assert
            Assert.Equal("pt", result);
        }

        [Fact]
        public void DetectLanguage_EnglishText_ReturnsEnglish()
        {
            // Arrange
            var input = "The study of ethnobotany is the investigation of plants and traditional societies";

            // Act
            var result = LanguageDetector.DetectLanguage(input);

            // Assert
            Assert.Equal("en", result);
        }

        [Fact]
        public void DetectLanguage_SpanishText_ReturnsPortugueseOrSpanish()
        {
            // Arrange
            // Spanish and Portuguese share many words, so this may return either
            var input = "La etnobotánica es el estudio de las plantas y las relaciones de las comunidades";

            // Act
            var result = LanguageDetector.DetectLanguage(input);

            // Assert
            // Spanish/Portuguese ambiguity - can return either, defaults to Portuguese
            Assert.True(result == "es" || result == "pt");
        }

        [Fact]
        public void DetectLanguage_PortugueseAbstract_ReturnsPortuguese()
        {
            // Arrange
            var input = "Este trabalho apresenta uma análise das plantas medicinais utilizadas " +
                        "pelas comunidades indígenas da região Amazônica para o tratamento de doenças";

            // Act
            var result = LanguageDetector.DetectLanguage(input);

            // Assert
            Assert.Equal("pt", result);
        }

        [Fact]
        public void DetectLanguage_EnglishAbstract_ReturnsEnglish()
        {
            // Arrange
            var input = "This study investigates the medicinal plants used by indigenous communities " +
                        "in the Amazon region for treating various diseases and health conditions";

            // Act
            var result = LanguageDetector.DetectLanguage(input);

            // Assert
            Assert.Equal("en", result);
        }

        [Fact]
        public void DetectLanguage_SpanishAbstract_ReturnsPortugueseOrSpanish()
        {
            // Arrange
            // Spanish and Portuguese share many words, so this may return either
            var input = "Este estudio investiga las plantas medicinales utilizadas por comunidades indígenas " +
                        "en la región amazónica para tratar enfermedades y condiciones de salud";

            // Act
            var result = LanguageDetector.DetectLanguage(input);

            // Assert
            // Spanish/Portuguese ambiguity - can return either, defaults to Portuguese
            Assert.True(result == "es" || result == "pt");
        }

        [Fact]
        public void DetectLanguage_EmptyString_ReturnsPortugueseDefault()
        {
            // Arrange
            var input = "";

            // Act
            var result = LanguageDetector.DetectLanguage(input);

            // Assert
            Assert.Equal("pt", result);
        }

        [Fact]
        public void DetectLanguage_WhitespaceOnly_ReturnsPortugueseDefault()
        {
            // Arrange
            var input = "   ";

            // Act
            var result = LanguageDetector.DetectLanguage(input);

            // Assert
            Assert.Equal("pt", result);
        }

        [Fact]
        public void DetectLanguage_ShortWords_ReturnsPortugueseDefault()
        {
            // Arrange
            // Words with fewer than 3 characters are ignored
            var input = "a b c d e f g";

            // Act
            var result = LanguageDetector.DetectLanguage(input);

            // Assert
            // No words with 3+ characters means default Portuguese
            Assert.Equal("pt", result);
        }

        [Fact]
        public void DetectLanguage_AmbiguousText_ReturnsPortugueseAsDefault()
        {
            // Arrange
            // Text with very few language-specific words defaults to Portuguese
            var input = "methodology data research results";

            // Act
            var result = LanguageDetector.DetectLanguage(input);

            // Assert
            // Should default to Portuguese as no clear language match
            Assert.Equal("pt", result);
        }

        [Fact]
        public void DetectLanguage_MixedLanguages_ReturnsDominantLanguage()
        {
            // Arrange
            var input = "The Portuguese word para means for, and the word de means of";

            // Act
            var result = LanguageDetector.DetectLanguage(input);

            // Assert
            // English words are more prevalent, so should return English
            Assert.Equal("en", result);
        }

        [Fact]
        public void GetLanguageName_PortugueseCode_ReturnsPortugueseName()
        {
            // Arrange
            var code = "pt";

            // Act
            var result = LanguageDetector.GetLanguageName(code);

            // Assert
            Assert.Equal("Portuguese", result);
        }

        [Fact]
        public void GetLanguageName_EnglishCode_ReturnsEnglishName()
        {
            // Arrange
            var code = "en";

            // Act
            var result = LanguageDetector.GetLanguageName(code);

            // Assert
            Assert.Equal("English", result);
        }

        [Fact]
        public void GetLanguageName_SpanishCode_ReturnsSpanishName()
        {
            // Arrange
            var code = "es";

            // Act
            var result = LanguageDetector.GetLanguageName(code);

            // Assert
            Assert.Equal("Spanish", result);
        }

        [Fact]
        public void GetLanguageName_UnknownCode_ReturnsUnknown()
        {
            // Arrange
            var code = "fr";

            // Act
            var result = LanguageDetector.GetLanguageName(code);

            // Assert
            Assert.Equal("Unknown", result);
        }

        [Fact]
        public void GetLanguageName_UppercaseCode_ReturnsLanguageName()
        {
            // Arrange
            var code = "PT";

            // Act
            var result = LanguageDetector.GetLanguageName(code);

            // Assert
            // Should be case-insensitive
            Assert.Equal("Portuguese", result);
        }

        [Fact]
        public void GetLanguageName_Null_ReturnsUnknown()
        {
            // Arrange
            string? code = null;

            // Act
            var result = LanguageDetector.GetLanguageName(code!);

            // Assert
            Assert.Equal("Unknown", result);
        }

        [Fact]
        public void DetectLanguage_TextWithPunctuation_HandlesCorrectly()
        {
            // Arrange
            var input = "O estudo da etnobotânica, que investiga as plantas medicinais, é muito importante";

            // Act
            var result = LanguageDetector.DetectLanguage(input);

            // Assert
            Assert.Equal("pt", result);
        }
    }
}
