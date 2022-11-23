namespace Quickwire.Tests;

using Attributes;

public partial class InjectConfigurationAttributeTests
{
    class TestConfig
    {
        [InjectConfiguration("NotExist:value")]
        public string Value { get; set; }

        [InjectConfiguration("NotExist:value2")]
        public int Value2 { get; set; } = 300;
    }
}
