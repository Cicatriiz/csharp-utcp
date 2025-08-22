// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Utcp.Text;
using FluentAssertions;
using Xunit;

public class TextCallTemplateTests
{
    [Fact]
    public void Template_HasTypeAndName()
    {
        var t = new TextCallTemplate { CallTemplateType = "text", Name = "manual", FilePath = "a.txt" };
        t.CallTemplateType.Should().Be("text");
        t.Name.Should().Be("manual");
        t.FilePath.Should().Be("a.txt");
    }
}

