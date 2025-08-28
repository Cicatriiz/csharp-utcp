// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Text.Json;
using Utcp.Core.Interfaces;
using Utcp.Core.Models;
using Utcp.Core.Serialization;
using Xunit;

public class DiscoveryTests
{
	private sealed record MyTemplate : CallTemplate { public MyTemplate(){ CallTemplateType = "mine"; } }

	[Fact]
	public void RegisterCallTemplate_AllowsDeserialize()
	{
		Discovery.RegisterCallTemplate("mine", typeof(MyTemplate));
		var json = "{\"call_template_type\":\"mine\",\"CallTemplateType\":\"mine\",\"Name\":\"x\"}";
		var options = new System.Text.Json.JsonSerializerOptions { Converters = { new Utcp.Core.Models.Serialization.CallTemplateJsonConverter() } };
		var value = JsonSerializer.Deserialize<CallTemplate>(json, options);
		Assert.IsType<MyTemplate>(value);
	}
}


