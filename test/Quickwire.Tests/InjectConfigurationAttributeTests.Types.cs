namespace Quickwire.Tests;

using System;
using System.Collections.Generic;
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
    class TestListConfig
    {
        [InjectConfiguration("List:Values")]
        public List<string> Value { get; set; }
        [InjectConfiguration("List:Values")]
        public string[] Value2 { get; set; }

        [InjectConfiguration("List:Values")]
        public DateTime[] Value3 { get; set; }

        [InjectConfiguration("List:Integers")]
        public int[] Value4 { get; set; }

        [InjectConfiguration("List:Integers")]
        public List<int> Value5 { get; set; }
    }
}
